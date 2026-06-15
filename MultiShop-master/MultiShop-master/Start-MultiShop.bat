@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%Run-MultiShopLocal.ps1"

if not exist "%PS_SCRIPT%" (
  echo [HATA] Script bulunamadi: "%PS_SCRIPT%"
  exit /b 1
)

set "MODE=%~1"
if "%MODE%"=="" set "MODE=run"

set "EXTRA_ARGS="

if /I "%~2"=="--skipbuild" (
  set "EXTRA_ARGS=%EXTRA_ARGS% -SkipBuild"
)

if /I "%~3"=="--skipbuild" (
  set "EXTRA_ARGS=%EXTRA_ARGS% -SkipBuild"
)

if /I "%~2"=="--skipinfra" (
  set "EXTRA_ARGS=%EXTRA_ARGS% -SkipInfraChecks"
)

if /I "%~3"=="--skipinfra" (
  set "EXTRA_ARGS=%EXTRA_ARGS% -SkipInfraChecks"
)

if /I "%~2"=="--headless" (
  set "EXTRA_ARGS=%EXTRA_ARGS% -Headless"
)

if /I "%~3"=="--headless" (
  set "EXTRA_ARGS=%EXTRA_ARGS% -Headless"
)

echo [INFO] MultiShop baslatiliyor... Mode=%MODE%
powershell -ExecutionPolicy Bypass -File "%PS_SCRIPT%" -Mode %MODE% %EXTRA_ARGS%

endlocal
