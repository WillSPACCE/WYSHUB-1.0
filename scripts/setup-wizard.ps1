param(
    [string]$SourcePath = ''
)

$ErrorActionPreference = 'Stop'

$AppFolder = Join-Path $env:ProgramFiles 'WYSHUB'
$ExeName = 'WYSHUB.exe'
$SourceAppPath = if ($SourcePath) { $SourcePath } else { Split-Path $PSScriptRoot -Parent }
$SourceExePath = Join-Path $SourceAppPath $ExeName
$TargetExePath = Join-Path $AppFolder $ExeName
$RequirementsPath = Join-Path (Split-Path $SourceAppPath -Parent) 'requirements.txt'
$DocumentsFolder = [Environment]::GetFolderPath('MyDocuments')
$SetupWizardLogFolder = Join-Path $DocumentsFolder 'SystemWM\Logs'
$SetupWizardLogPath = Join-Path $SetupWizardLogFolder ('SetupWizard_{0:yyyyMMdd_HHmmss}.log' -f (Get-Date))

New-Item -ItemType Directory -Path $SetupWizardLogFolder -Force | Out-Null
Start-Transcript -Path $SetupWizardLogPath -Force | Out-Null

function Write-SetupWizardLog {
    param(
        [string]$Message
    )

    Write-Host $Message
}

if (-not $SourceExePath -or -not (Test-Path $SourceExePath)) {
    $SourceAppPath = (Get-Location).Path
    $SourceExePath = Join-Path $SourceAppPath $ExeName
}

if (-not (Test-Path $SourceExePath)) {
    throw "Executável não encontrado para instalação: $SourceExePath"
}

function Test-DotNet8Runtime {
    try {
        $output = & dotnet --list-runtimes 2>$null
        return $output -match 'Microsoft.NETCore.App 8'
    }
    catch {
        return $false
    }
}

function Test-VisualCppRedist {
    try {
        $subkey = Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue
        return ($subkey | Where-Object { $_.DisplayName -match 'Microsoft Visual C\+\+ 2015-2022 Redistributable' }).Count -gt 0
    }
    catch {
        return $false
    }
}

function Test-WmiService {
    try {
        $status = sc.exe query winmgmt 2>$null
        return ($status -match 'RUNNING') -or ($status -match 'START_PENDING')
    }
    catch {
        return $false
    }
}

function Install-RequirementItem {
    param(
        [string]$ItemName,
        [scriptblock]$CheckScript,
        [scriptblock]$InstallScript
    )

    Write-SetupWizardLog "Verificando: $ItemName"
    if (& $CheckScript) {
        Write-SetupWizardLog "OK: $ItemName já está presente no sistema."
        return
    }

    Write-SetupWizardLog "Faltando: $ItemName. Iniciando instalação..."
    try {
        & $InstallScript
        Write-SetupWizardLog "Instalação finalizada para: $ItemName"
    }
    catch {
        Write-SetupWizardLog ("Falha ao instalar {0}: {1}" -f $ItemName, $_.Exception.Message)
        throw
    }
}

try {
    New-Item -ItemType Directory -Path $AppFolder -Force | Out-Null
    Write-SetupWizardLog '== Assistente de instalação do WYSHUB =='
    Write-SetupWizardLog "Log do assistente: $SetupWizardLogPath"
    Write-SetupWizardLog '1) Instalando arquivos no diretório do programa...'

    if ($SourceAppPath -ne $AppFolder) {
        Copy-Item -Path (Join-Path $SourceAppPath '*') -Destination $AppFolder -Recurse -Force
    }

    Write-SetupWizardLog '2) Criando atalhos...'
    try {
        $shell = New-Object -ComObject WScript.Shell
        $desktopPath = [Environment]::GetFolderPath('Desktop')
        $startMenuPath = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\WYSHUB'
        New-Item -ItemType Directory -Path $startMenuPath -Force | Out-Null

        $desktopShortcut = Join-Path $desktopPath 'WYSHUB.lnk'
        $shortcut = $shell.CreateShortcut($desktopShortcut)
        $shortcut.TargetPath = $TargetExePath
        $shortcut.WorkingDirectory = $AppFolder
        $shortcut.IconLocation = $TargetExePath
        $shortcut.Save()

        $startMenuShortcut = Join-Path $startMenuPath 'WYSHUB.lnk'
        $shortcut = $shell.CreateShortcut($startMenuShortcut)
        $shortcut.TargetPath = $TargetExePath
        $shortcut.WorkingDirectory = $AppFolder
        $shortcut.IconLocation = $TargetExePath
        $shortcut.Save()

        Write-SetupWizardLog 'Atalhos criados.'
    }
    catch {
        Write-Warning "Não foi possível criar atalhos: $($_.Exception.Message)"
    }

    Write-SetupWizardLog '3) Verificando o checklist de requisitos item por item...'
    Install-RequirementItem -ItemName '.NET 8 Desktop Runtime x64' -CheckScript { Test-DotNet8Runtime } -InstallScript {
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            winget install --id Microsoft.DotNet.Runtime.8 --source winget --accept-source-agreements --accept-package-agreements -e | Out-Null
        }
        else {
            Write-Warning 'winget não foi encontrado. Instale o runtime .NET 8 manualmente.'
        }
    }

    Install-RequirementItem -ItemName 'Visual C++ Redistributable 2015-2022 x64' -CheckScript { Test-VisualCppRedist } -InstallScript {
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            winget install --id Microsoft.VCRedist.2015+.x64 --source winget --accept-source-agreements --accept-package-agreements -e | Out-Null
        }
        else {
            Write-Warning 'winget não foi encontrado. Instale o Visual C++ Redistributable manualmente.'
        }
    }

    Install-RequirementItem -ItemName 'Serviço WMI' -CheckScript { Test-WmiService } -InstallScript {
        sc.exe config winmgmt start= auto | Out-Null
        net start winmgmt | Out-Null
        Write-Host 'WMI habilitado.'
    }

    Write-SetupWizardLog '4) Aplicando exclusões do Defender...'
    try {
        Add-MpPreference -ExclusionPath $AppFolder -ErrorAction SilentlyContinue
        if (Test-Path $TargetExePath) {
            Add-MpPreference -ExclusionProcess $TargetExePath -ErrorAction SilentlyContinue
        }
        Write-SetupWizardLog 'Exclusões aplicadas.'
    }
    catch {
        Write-Warning "Falha ao aplicar exclusões do Defender: $($_.Exception.Message)"
    }

    Write-SetupWizardLog 'Assistente concluído.'
}
finally {
    Stop-Transcript | Out-Null
}
