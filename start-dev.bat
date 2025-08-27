@echo off
echo 🚀 Starting CNC Machine Tracker Development Environment...

echo.
echo 📦 Installing React dependencies...
cd CncMachineTracker.UI
call npm install

echo.
echo 🔧 Building API...
cd ..\CncMachineTracker.Api
call dotnet build

echo.
echo 🎯 Starting applications...
echo API will be available at: https://localhost:7000
echo React app will be available at: http://localhost:3000
echo Swagger UI will be available at: https://localhost:7000/swagger

echo.
echo ⏳ Starting API...
start "API" cmd /k "cd CncMachineTracker.Api && dotnet run"

echo ⏳ Starting React app...
start "React" cmd /k "cd CncMachineTracker.UI && npm start"

echo.
echo ✅ Both applications are starting...
echo Please wait a moment for the applications to fully load.
pause
