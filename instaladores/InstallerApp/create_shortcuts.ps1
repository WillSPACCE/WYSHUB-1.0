$ErrorActionPreference = 'Stop'
$targetDir = Join-Path $env:ProgramFiles 'WYSHUB'
$desktop = [Environment]::GetFolderPath('Desktop')
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\WYSHUB'
New-Item -ItemType Directory -Force -Path $startMenu | Out-Null
$wsh = New-Object -ComObject WScript.Shell

$desktopShortcut = $wsh.CreateShortcut((Join-Path $desktop 'WYSHUB.lnk'))
$desktopShortcut.TargetPath = Join-Path $targetDir 'WYSHUB.exe'
$desktopShortcut.WorkingDirectory = $targetDir
$desktopShortcut.IconLocation = Join-Path $targetDir 'WYSHUB.exe'
$desktopShortcut.Save()

$startShortcut = $wsh.CreateShortcut((Join-Path $startMenu 'WYSHUB.lnk'))
$startShortcut.TargetPath = Join-Path $targetDir 'WYSHUB.exe'
$startShortcut.WorkingDirectory = $targetDir
$startShortcut.IconLocation = Join-Path $targetDir 'WYSHUB.exe'
$startShortcut.Save()
