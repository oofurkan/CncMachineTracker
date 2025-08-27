# CNC Machine Tracker Application

A full-stack application for tracking CNC machine status and production data with real-time updates and visualization.

## 🚀 Features

### Backend (.NET Core Web API)
- **Machine Management**: CRUD operations for CNC machines
- **Real-time Data**: Simulated machine data with random generation
- **RESTful API**: Clean, documented endpoints with Swagger
- **Status Tracking**: Monitor machine status (Çalışıyor, Duruşta, Alarm)

### Frontend (React)
- **Responsive Design**: Modern UI with Tailwind CSS
- **Real-time Updates**: Auto-refresh every 5 seconds
- **Machine List**: Table view with color-coded status indicators
- **Machine Details**: Detailed view with charts and metrics
- **Interactive Charts**: Production count visualization using Recharts

## 🛠️ Technologies Used

### Backend
- **.NET 8** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Swagger/OpenAPI** - API documentation
- **Dependency Injection** - Clean architecture
- **CORS** - Cross-origin resource sharing

### Frontend
- **React 18** - Modern React with hooks
- **React Router** - Client-side routing
- **Tailwind CSS** - Utility-first CSS framework
- **Recharts** - Chart library for data visualization
- **Fetch API** - HTTP client

## 📋 Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 16+** - [Download here](https://nodejs.org/)
- **npm** or **yarn** - Package managers

## 🚀 Setup Instructions

### 1. Clone the Repository
```bash
git clone <repository-url>
cd CncMachineTracker.Api
```

### 2. Backend Setup (API)

#### Navigate to API Project
```bash
cd CncMachineTracker.Api
```

#### Restore Dependencies
```bash
dotnet restore
```

#### Run the API
```bash
dotnet run
```

The API will be available at:
- **HTTPS**: https://localhost:7000
- **HTTP**: http://localhost:5000
- **Swagger UI**: https://localhost:7000/swagger

### 3. Frontend Setup (React)

#### Navigate to UI Project
```bash
cd CncMachineTracker.UI
```

#### Install Dependencies
```bash
npm install
```

#### Start the React App
```bash
npm start
```

The React app will be available at:
- **http://localhost:3000**

## 📡 API Endpoints

### Base URL
- **Development**: `https://localhost:7000/api`

### Available Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/machines` | Get all machines |
| `GET` | `/machines/{id}` | Get machine by ID |
| `POST` | `/machines/simulate` | Generate new simulated machine |

### Machine Model
```json
{
  "id": "string",
  "status": "Çalışıyor|Duruşta|Alarm",
  "productionCount": "number",
  "cycleTime": "number (seconds)",
  "timestamp": "datetime"
}
```

## 🎨 UI Features

### Machine List View
- **Status Indicators**: Color-coded badges
  - 🟢 Green: Çalışıyor (Working)
  - 🟡 Yellow: Duruşta (Stopped)
  - 🔴 Red: Alarm
- **Auto-refresh**: Updates every 5 seconds
- **Simulate Button**: Generate new test machines

### Machine Detail View
- **Detailed Information**: All machine properties
- **Status Summary**: Working status, production rate, efficiency
- **Production Chart**: Line chart showing production count over time
- **Real-time Updates**: Auto-refresh with latest data

## 🔧 Configuration

### Environment Variables

#### Frontend (.env)
Create a `.env` file in the `CncMachineTracker.UI` directory:
```env
REACT_APP_API_URL=https://localhost:7000/api
```

#### Backend (appsettings.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## 🏗️ Project Structure

```
CncMachineTracker.Api/
├── Controllers/
│   └── MachinesController.cs
├── Models/
│   └── Machine.cs
├── Services/
│   ├── IMachineService.cs
│   └── MachineService.cs
├── Program.cs
└── appsettings.json

CncMachineTracker.UI/
├── src/
│   ├── components/
│   │   ├── MachineList.js
│   │   ├── MachineDetail.js
│   │   └── Navbar.js
│   ├── services/
│   │   └── api.js
│   ├── App.js
│   └── index.js
├── public/
│   └── index.html
└── package.json
```

## 🚀 Development

### Running Both Applications

1. **Terminal 1** - Start the API:
```bash
cd CncMachineTracker.Api
dotnet run
```

2. **Terminal 2** - Start the React app:
```bash
cd CncMachineTracker.UI
npm start
```

### Building for Production

#### Backend
```bash
cd CncMachineTracker.Api
dotnet publish -c Release
```

#### Frontend
```bash
cd CncMachineTracker.UI
npm run build
```

## 🔍 Testing

### API Testing
- Use Swagger UI at `https://localhost:7000/swagger`
- Test endpoints directly in the browser
- Use tools like Postman or curl

### Frontend Testing
```bash
cd CncMachineTracker.UI
npm test
```

## 🐛 Troubleshooting

### Common Issues

1. **CORS Errors**
   - Ensure the API is running on port 7000
   - Check that CORS is properly configured in Program.cs

2. **Port Conflicts**
   - API: Change ports in `Properties/launchSettings.json`
   - React: Change port in `package.json` or use `PORT=3001 npm start`

3. **SSL Certificate Issues**
   - For development, accept the self-signed certificate
   - Or use HTTP endpoints for testing

## 📝 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📞 Support

For questions or issues, please create an issue in the repository.
