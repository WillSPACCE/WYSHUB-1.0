$ErrorActionPreference = 'Stop'

$source = 'C:\Users\willy\OneDrive\Documentos\WYSHUB\instaladores\WYSHUB_Portavel'
$target = Join-Path $env:ProgramFiles 'WYSHUB'
$desktop = [Environment]::GetFolderPath('Desktop')
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\WYSHUB'

if (-not (Test-Path $source)) {
    throw "Pasta de origem não encontrada: $source"
}

New-Item -ItemType Directory -Force -Path $target | Out-Null
New-Item -ItemType Directory -Force -Path $startMenu | Out-Null

Copy-Item "$source\*" -Destination $target -Recurse -Force

$shortcutPathDesktop = Join-Path $desktop 'WYSHUB.lnk'
$shortcutPathStart = Join-Path $startMenu 'WYSHUB.lnk'
$wsh = New-Object -ComObject WScript.Shell

$desktopShortcut = $wsh.CreateShortcut($shortcutPathDesktop)
$desktopShortcut.TargetPath = Join-Path $target 'WYSHUB.exe'
$desktopShortcut.WorkingDirectory = $target
$desktopShortcut.IconLocation = Join-Path $target 'WYSHUB.exe'
$desktopShortcut.Save()

$startShortcut = $wsh.CreateShortcut($shortcutPathStart)
$startShortcut.TargetPath = Join-Path $target 'WYSHUB.exe'
$startShortcut.WorkingDirectory = $target
$startShortcut.IconLocation = Join-Path $target 'WYSHUB.exe'
$startShortcut.Save()

Write-Host "Instalação concluída em: $target"
