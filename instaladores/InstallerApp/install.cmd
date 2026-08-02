@echo off
setlocal
set TARGET=%ProgramFiles%\WYSHUB
set SOURCE=%~dp0
if not exist "%TARGET%" mkdir "%TARGET%"
xcopy "%SOURCE%*" "%TARGET%" /E /I /Y >nul
powershell -NoProfile -ExecutionPolicy Bypass -File "%SOURCE%create_shortcuts.ps1"
if exist "%TARGET%\WYSHUB.exe" (
  echo Instalacao concluida com sucesso.
) else (
  echo Falha na instalacao.
  exit /b 1
)
endlocal
