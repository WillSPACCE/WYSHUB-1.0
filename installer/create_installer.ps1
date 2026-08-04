$ErrorActionPreference = 'Stop'

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot '..\WYSHUB')
$publishDir = Join-Path $projectRoot 'bin\Release\net8.0-windows\win-x64\publish'
$exePath = Join-Path $publishDir 'WYSHUB.exe'
$installerDir = Join-Path $PSScriptRoot 'output'
$iconPath = Join-Path $projectRoot 'icons\Light.ico'

Write-Host 'Publicando aplicação em Release win-x64...'
Push-Location $projectRoot
try {
    dotnet publish .\SystemWM.csproj -c Release -r win-x64 /p:PublishSingleFile=true /p:SelfContained=true /p:EnableCompressionInSingleFile=true | Write-Host
}
finally {
    Pop-Location
}

if (-not (Test-Path $exePath)) {
    throw "Executável não encontrado: $exePath"
}

if (-not (Test-Path $exePath)) {
    throw "Executável não encontrado: $exePath"
}

if (-not (Test-Path $iconPath)) {
    $iconPath = $exePath
}

New-Item -ItemType Directory -Force -Path $installerDir | Out-Null

$packageDir = Join-Path $installerDir 'WYSHUB_Installer'
Remove-Item -Recurse -Force $packageDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

Copy-Item $exePath -Destination (Join-Path $packageDir 'WYSHUB.exe')
Copy-Item $iconPath -Destination (Join-Path $packageDir 'WYSHUB.ico') -ErrorAction SilentlyContinue

$readme = @"
WYSHUB

Aplicativo desktop para suporte técnico e visitas de campo.

Instalação:
1. Extraia a pasta para o computador do cliente.
2. Execute WYSHUB.exe.
3. Se o Windows mostrar "App não é confiável", clique em Mais informações e depois em Executar mesmo assim.
4. O app pode solicitar permissão de Administrador para funções de firewall e sensores.

Observações:
- O programa foi publicado em modo self-contained para reduzir dependências.
- Para reduzir bloqueios do Defender, recomende instalar em C:\Program Files\WYSHUB.
"@
Set-Content -Path (Join-Path $packageDir 'Leia-me.txt') -Value $readme

$shortcutPath = Join-Path $packageDir 'WYSHUB.lnk'
$wsh = New-Object -ComObject WScript.Shell
$shortcut = $wsh.CreateShortcut($shortcutPath)
$shortcut.TargetPath = (Join-Path $packageDir 'WYSHUB.exe')
$shortcut.WorkingDirectory = $packageDir
$shortcut.IconLocation = (Join-Path $packageDir 'WYSHUB.ico')
$shortcut.Save()

$zipPath = Join-Path $installerDir 'WYSHUB_installer.zip'
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $packageDir '*') -DestinationPath $zipPath -Force

Write-Host "Instalador criado em: $zipPath"
