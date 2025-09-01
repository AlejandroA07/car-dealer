@echo off
echo Starting Ultimate Development Environment...
echo.

echo [1/4] Opening project in Visual Studio Code...
code .
echo.

echo [2/4] Checking for Docker Desktop and starting if necessary...
tasklist /FI "IMAGENAME eq Docker Desktop.exe" | find /I "Docker Desktop.exe" > nul
if errorlevel 1 (
    echo Docker Desktop not found, attempting to start it...
    start "" /B "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    echo Please wait a moment for Docker Desktop to initialize...
) else (
    echo Docker Desktop is already running.
)
echo.

echo [3/4] Opening new PowerShell terminal and running 'docker-compose up --build'...
rem -NoExit keeps the window open after the command runs
start "Docker Compose" powershell -NoExit -Command "docker-compose up --build"

echo [4/4] Opening extra general-purpose PowerShell terminal...
start "General" powershell

echo.
echo Script finished. Your environment is ready!