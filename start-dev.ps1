# CNC Machine Tracker - Development Startup Script
# This script starts both the API and React frontend

Write-Host "🚀 Starting CNC Machine Tracker Development Environment..." -ForegroundColor Green

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js found: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js not found. Please install Node.js 16+." -ForegroundColor Red
    exit 1
}

# Check if npm is installed
try {
    $npmVersion = npm --version
    Write-Host "✅ npm found: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ npm not found. Please install npm." -ForegroundColor Red
    exit 1
}

Write-Host "`n📦 Installing React dependencies..." -ForegroundColor Yellow
Set-Location "CncMachineTracker.UI"
npm install

Write-Host "`n🔧 Building API..." -ForegroundColor Yellow
Set-Location "..\CncMachineTracker.Api"
dotnet build

Write-Host "`n🎯 Starting applications..." -ForegroundColor Yellow
Write-Host "API will be available at: https://localhost:7000" -ForegroundColor Cyan
Write-Host "React app will be available at: http://localhost:3000" -ForegroundColor Cyan
Write-Host "Swagger UI will be available at: https://localhost:7000/swagger" -ForegroundColor Cyan

Write-Host "`n⏳ Starting API in background..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'CncMachineTracker.Api'; dotnet run"

Write-Host "⏳ Starting React app in background..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'CncMachineTracker.UI'; npm start"

Write-Host "`n✅ Both applications are starting..." -ForegroundColor Green
Write-Host "Please wait a moment for the applications to fully load." -ForegroundColor Yellow
Write-Host "Press any key to exit this script (applications will continue running)..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
