@REM @echo off
@REM setlocal

@cd /d %~dp0

@set "PKG=%USERPROFILE%\.nuget\packages\makekits.tools"
@if not exist "%PKG%" (
    echo ERROR: Package not found:
    echo %PKG%
)

@set "MAKEKITS_TOOLS="
@for /f "delims=" %%i in ('powershell -NoProfile -Command "Get-ChildItem -Path '%PKG%' -Directory | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1 -ExpandProperty Name"') do (
    @set "MAKEKITS_TOOLS=%PKG%\%%i"
)

@if not defined MAKEKITS_TOOLS (
    echo ERROR: No installed version found.
)

@set "MAKEKITS_EXE=%MAKEKITS_TOOLS%\build\makekits.exe"
@if not exist "%MAKEKITS_EXE%" (
    echo ERROR: %MAKEKITS_EXE% not found.
)

"%MAKEKITS_EXE%" %*
@pause
