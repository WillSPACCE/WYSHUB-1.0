param(
    [string]$AppFolder = "C:\Program Files\WYSHUB"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -Path $AppFolder)) {
    New-Item -ItemType Directory -Path $AppFolder -Force | Out-Null
}

$exePath = Join-Path $AppFolder 'WYSHUB.exe'

try {
    Add-MpPreference -ExclusionPath $AppFolder -ErrorAction Stop
    Write-Host "Exclusão adicionada para a pasta: $AppFolder"
}
catch {
    Write-Warning "Não foi possível adicionar a exclusão da pasta: $($_.Exception.Message)"
}

if (Test-Path -Path $exePath) {
    try {
        Add-MpPreference -ExclusionProcess $exePath -ErrorAction Stop
        Write-Host "Exclusão adicionada para o processo: $exePath"
    }
    catch {
        Write-Warning "Não foi possível adicionar a exclusão do processo: $($_.Exception.Message)"
    }
}
else {
    Write-Warning "O executável WYSHUB.exe não foi encontrado em $AppFolder"
}

Write-Host "Se o Defender continuar bloqueando, mova o app para C:\Program Files\WYSHUB e execute novamente este script."
