$ErrorActionPreference = 'Stop'

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot '..\WYSHUB')
$publishDir = Join-Path $projectRoot 'bin\Release\net8.0-windows\win-x64\publish'
$exePath = Join-Path $publishDir 'WYSHUB.exe'
$installerDir = Join-Path $PSScriptRoot 'output'

if (-not (Test-Path $exePath)) {
    throw "Executável não encontrado: $exePath"
}

New-Item -ItemType Directory -Force -Path $installerDir | Out-Null

$packageDir = Join-Path $installerDir 'WYSHUB_Installer'
Remove-Item -Recurse -Force $packageDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

Copy-Item $exePath -Destination (Join-Path $packageDir 'WYSHUB.exe')

$readme = @"
WYSHUB

Aplicativo desktop para suporte técnico e visitas de campo.

Instalação:
1. Copie a pasta para o computador do cliente.
2. Execute WYSHUB.exe.
3. O app solicitará permissão de Administrador, o que é necessário para algumas funções.
"@
Set-Content -Path (Join-Path $packageDir 'Leia-me.txt') -Value $readme

$shortcutPath = Join-Path $packageDir 'WYSHUB.lnk'
$wsh = New-Object -ComObject WScript.Shell
$shortcut = $wsh.CreateShortcut($shortcutPath)
$shortcut.TargetPath = (Join-Path $packageDir 'WYSHUB.exe')
$shortcut.WorkingDirectory = $packageDir
$shortcut.IconLocation = (Join-Path $packageDir 'WYSHUB.exe')
$shortcut.Save()

$zipPath = Join-Path $installerDir 'WYSHUB_installer.zip'
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $packageDir '*') -DestinationPath $zipPath -Force

Write-Host "Instalador criado em: $zipPath"
