$ErrorActionPreference = 'Stop'

param(
    [string]$SourcePath = ''
)

$AppFolder = Join-Path $env:ProgramFiles 'WYSHUB'
$ExeName = 'WYSHUB.exe'
$SourceAppPath = if ($SourcePath) { $SourcePath } else { Split-Path $PSScriptRoot -Parent }
$SourceExePath = Join-Path $SourceAppPath $ExeName
$TargetExePath = Join-Path $AppFolder $ExeName

if (-not $SourceExePath -or -not (Test-Path $SourceExePath)) {
    $SourceAppPath = (Get-Location).Path
    $SourceExePath = Join-Path $SourceAppPath $ExeName
}

if (-not (Test-Path $SourceExePath)) {
    throw "Executável não encontrado para instalação: $SourceExePath"
}

New-Item -ItemType Directory -Path $AppFolder -Force | Out-Null

Write-Host '== Assistente de instalação do WYSHUB =='
Write-Host '1) Instalando arquivos no diretório do programa...'

if ($SourceAppPath -ne $AppFolder) {
    Copy-Item -Path (Join-Path $SourceAppPath '*') -Destination $AppFolder -Recurse -Force
}

Write-Host '2) Criando atalhos...'
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

    Write-Host 'Atalhos criados.'
}
catch {
    Write-Warning "Não foi possível criar atalhos: $($_.Exception.Message)"
}

Write-Host '3) Verificando runtime .NET 8...'
$dotnetInstalled = $false
try {
    $dotnetVersion = (& dotnet --list-runtimes 2>$null | Select-String 'Microsoft.NETCore.App 8' -SimpleMatch)
    if ($dotnetVersion) {
        $dotnetInstalled = $true
    }
}
catch {}

if (-not $dotnetInstalled) {
    Write-Host 'O runtime .NET 8 não foi encontrado. O programa pode continuar se estiver self-contained.'
}

Write-Host '4) Habilitando WMI...'
try {
    sc.exe config winmgmt start= auto | Out-Null
    net start winmgmt | Out-Null
    Write-Host 'WMI habilitado.'
}
catch {
    Write-Warning "Não foi possível ajustar o WMI: $($_.Exception.Message)"
}

Write-Host '5) Adicionando exclusões do Defender...'
try {
    Add-MpPreference -ExclusionPath $AppFolder -ErrorAction SilentlyContinue
    if (Test-Path $TargetExePath) {
        Add-MpPreference -ExclusionProcess $TargetExePath -ErrorAction SilentlyContinue
    }
    Write-Host 'Exclusões aplicadas.'
}
catch {
    Write-Warning "Falha ao aplicar exclusões do Defender: $($_.Exception.Message)"
}

Write-Host 'Assistente concluído.'
