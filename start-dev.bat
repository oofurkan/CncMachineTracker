@echo off
echo 🚀 Starting CNC Machine Tracker Development Environment...

echo.
echo 📦 Installing React dependencies...
cd CncMachineTracker.UI
call npm install

echo.
echo 🔧 Building API...
cd ..\CncMachineTracker.Api.Clean
call dotnet build

echo.
echo 🎯 Starting applications...
echo API will be available at: http://localhost:5217
echo React app will be available at: http://localhost:3000
echo Swagger UI will be available at: http://localhost:5217/swagger

echo.
echo ⏳ Starting API...
start "API" cmd /k "cd CncMachineTracker.Api.Clean && dotnet run"

echo ⏳ Starting React app...
start "React" cmd /k "cd CncMachineTracker.UI && npm start"

echo.
echo ✅ Both applications are starting...
echo Please wait a moment for the applications to fully load.
pause
