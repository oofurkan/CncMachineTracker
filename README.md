# 🏭 CNC Machine Tracker

A modern, clean architecture solution for tracking CNC machine status, production data, and FANUC integration.

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    CncMachineTracker.UI                     │
│                    (React Frontend)                        │
└─────────────────────┬───────────────────────────────────────┘
                      │ HTTP/API Calls
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                CncMachineTracker.Api.Clean                 │
│                (Web API Layer)                             │
└─────────────────────┬───────────────────────────────────────┘
                      │ Dependency Injection
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              CncMachineTracker.Application                 │
│              (Business Logic Layer)                        │
│  • MachineService (Simulation & Business Rules)           │
│  • Ports (IMachineRepository, IFanucClient)               │
│  • DTOs (MachineDto, MachineHistoryDto)                   │
└─────────────────────┬───────────────────────────────────────┘
                      │ Interface Implementation
                      ▼
┌─────────────────────────────────────────────────────────────┐
│             CncMachineTracker.Infrastructure               │
│             (Data & External Integration)                  │
│  • InMemoryMachineRepository                               │
│  • MockFanucClient / FocasFanucClient                     │
└─────────────────────┬───────────────────────────────────────┘
                      │ Entity Usage
                      ▼
┌─────────────────────────────────────────────────────────────┐
│               CncMachineTracker.Domain                     │
│               (Core Entities & Enums)                     │
│  • Machine, MachineSample, MachineStatus                  │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0 SDK** (latest version)
- **Node.js 16+** and **npm**
- **Modern web browser** (Chrome, Firefox, Edge)

### Option 1: Use Start Scripts (Recommended)

#### Windows Batch File
```bash
start-dev.bat
```

#### PowerShell Script
```powershell
.\start-dev.ps1
```

### Option 2: Manual Startup

#### 1. Start the API
```bash
cd CncMachineTracker.Api.Clean
dotnet run
```
**API will be available at:** http://localhost:5217
**Swagger UI:** http://localhost:5217/swagger

#### 2. Start the React Frontend
```bash
cd CncMachineTracker.UI
npm install
npm start
```
**Frontend will be available at:** http://localhost:3000

## 🔧 Configuration

### API Configuration (`appsettings.json`)
```json
{
  "UseFanuc": false,           // Enable real FANUC integration
  "HistoryWindowMinutes": 10   // History retention window
}
```

### Frontend Configuration
- **API Port:** Automatically configured to use port 5217
- **Proxy:** Set to http://localhost:5217 for development
- **Environment Variables:** Set `REACT_APP_API_URL` to override API URL

## 📊 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/machines` | Get all machines current snapshots |
| `GET` | `/api/machines/{id}` | Get latest snapshot for specific machine |
| `GET` | `/api/machines/{id}/history?minutes=10` | Get machine history (last N minutes) |
| `POST` | `/api/machines/{id}/simulate` | Simulate realistic state changes |
| `POST` | `/api/machines/{id}/refresh` | Refresh from FANUC (when enabled) |

### Example API Calls

#### Simulate Machine M001
```bash
curl -X POST "http://localhost:5217/api/machines/M001/simulate"
```

#### Get Machine History
```bash
curl "http://localhost:5217/api/machines/M001/history?minutes=15"
```

#### Get All Machines
```bash
curl "http://localhost:5217/api/machines"
```

## 🎮 Frontend Features

### Machine List View
- **Real-time Updates:** Auto-refresh every 5 seconds
- **Individual Simulation:** Simulate specific machines
- **New Machine Creation:** Generate new machines with sequential IDs
- **Status Indicators:** Visual status with colors and icons

### Machine Detail View
- **Live Data:** Real-time machine status and metrics
- **History Charts:** Production count over time visualization
- **History Table:** Detailed historical data view
- **Action Buttons:** Simulate and refresh functionality
- **Responsive Design:** Works on desktop and mobile

### Data Visualization
- **Production Charts:** Line charts showing production trends
- **Status Tracking:** Real-time status monitoring
- **Performance Metrics:** Cycle time and efficiency calculations

## 🧪 Testing

### Run All Tests
```bash
dotnet test CncMachineTracker.Clean.sln
```

### Test Coverage
- ✅ **22/22 tests passing**
- ✅ **Unit Tests:** Business logic, repository operations
- ✅ **Integration Tests:** API endpoints, end-to-end workflows
- ✅ **Simulation Tests:** State machine behavior validation

## 🔌 FANUC Integration

### Current Status
- **Mock Implementation:** ✅ Ready for development/testing
- **Real Integration:** 🔧 Scaffold ready for FOCAS library

### Enable Real FANUC Integration

1. **Install FANUC FOCAS Library**
   - Download Focas1.dll and Focas2.dll
   - Place in project's lib folder

2. **Update Configuration**
   ```json
   {
     "UseFanuc": true
   }
   ```

3. **Implement FOCAS Calls**
   - Edit `FocasFanucClient.cs`
   - Add P/Invoke declarations
   - Implement connection management

### FOCAS Function Examples
```csharp
// Connect to machine
cnc_allclibhndl3(ip, port, user, pass, out handle);

// Get status information
cnc_statinfo(handle, out status);

// Read parameters
cnc_rdparam(handle, param, axis, out value);

// Cleanup
cnc_freelibhndl(handle);
```

## 🎯 Realistic Simulation Features

### State Machine Rules
- **Per-machine baselines** for consistent behavior
- **Markov chain transitions** with realistic probabilities
- **Production count progression** based on status
- **Cycle time stability** with ±10% noise

### Simulation Logic
```csharp
// Status transition probabilities
Running → Running: 80%
Running → Stopped: 15%
Running → Alarm: 5%

// Production count changes
Running: +1 to +5 parts
Stopped: +0 (or rarely +1)
Alarm: +0

// Cycle time simulation
Base cycle time ± 10% noise
Clamped between 10-120 seconds
```

## 🚀 Development Workflow

### 1. **Start Development Environment**
```bash
.\start-dev.ps1
```

### 2. **Make Changes**
- Edit API code in `CncMachineTracker.Api.Clean`
- Edit business logic in `CncMachineTracker.Application`
- Edit UI components in `CncMachineTracker.UI`

### 3. **Test Changes**
```bash
# Test API
dotnet test CncMachineTracker.Tests

# Test UI (in separate terminal)
cd CncMachineTracker.UI
npm test
```

### 4. **Build & Deploy**
```bash
# Build API
dotnet build CncMachineTracker.Api.Clean

# Build UI
cd CncMachineTracker.UI
npm run build
```

## 📁 Project Structure

```
CncMachineTracker/
├── CncMachineTracker.Domain/           # Core entities & enums
├── CncMachineTracker.Application/      # Business logic & ports
├── CncMachineTracker.Infrastructure/   # Data & external adapters
├── CncMachineTracker.Api.Clean/        # Web API layer
├── CncMachineTracker.UI/               # React frontend
├── CncMachineTracker.Tests/            # Test suite
├── start-dev.bat                       # Windows startup script
├── start-dev.ps1                       # PowerShell startup script
└── README.md                           # This file
```

## 🔍 Troubleshooting

### Common Issues

#### API Not Starting
- Check .NET 9.0 SDK installation: `dotnet --version`
- Verify port 5217 is not in use
- Check build errors: `dotnet build`

#### Frontend Not Connecting
- Verify API is running on port 5217
- Check browser console for CORS errors
- Verify proxy setting in package.json

#### Tests Failing
- Ensure all NuGet packages are restored
- Check .NET version compatibility
- Run tests individually to isolate issues

### Port Configuration
- **API:** http://localhost:5217
- **Frontend:** http://localhost:3000
- **Swagger:** http://localhost:5217/swagger

## 🤝 Contributing

1. **Fork the repository**
2. **Create feature branch:** `git checkout -b feature/amazing-feature`
3. **Commit changes:** `git commit -m 'Add amazing feature'`
4. **Push to branch:** `git push origin feature/amazing-feature`
5. **Open Pull Request**

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Built with ❤️ using .NET 9 and Clean Architecture principles**
