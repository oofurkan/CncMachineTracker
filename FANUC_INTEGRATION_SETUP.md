# 🔌 FANUC Integration Setup Guide

## 🎯 Overview

This guide will help you set up real FANUC CNC machine integration with your CNC Machine Tracker application. Once configured, you'll be able to read real-time data from actual FANUC controllers instead of using simulated data.

## 📋 Prerequisites

### **Hardware Requirements**
- ✅ **FANUC CNC Machine(s)** with network connectivity
- ✅ **Network access** to CNC machine IP addresses
- ✅ **FANUC FOCAS library files** (Focas1.dll, Focas2.dll)

### **Software Requirements**
- ✅ **.NET 9.0 Runtime** (already installed)
- ✅ **FANUC FOCAS SDK** (download from FANUC)
- ✅ **Network connectivity** to CNC machines

## 📥 **Step 1: Download FANUC FOCAS Libraries**

### **Option A: Official FANUC SDK**
1. **Contact FANUC Support** for official FOCAS SDK
2. **Download FOCAS library files** (Focas1.dll, Focas2.dll)
3. **Verify library versions** match your CNC controller

### **Option B: FANUC Website**
1. **Visit** [FANUC FOCAS Downloads](https://www.fanuc.eu/ro/en/support/downloads)
2. **Download** FOCAS library for your controller series
3. **Extract** DLL files to your project

## 📁 **Step 2: Install FOCAS Libraries**

### **Copy Libraries to Project**
```bash
# Create lib folder in your project
mkdir CncMachineTracker.Infrastructure\lib

# Copy FOCAS libraries
copy Focas1.dll CncMachineTracker.Infrastructure\lib\
copy Focas2.dll CncMachineTracker.Infrastructure\lib\
```

### **Verify Library Files**
```
CncMachineTracker.Infrastructure/
├── lib/
│   ├── Focas1.dll          # ✅ FANUC FOCAS Library 1
│   └── Focas2.dll          # ✅ FANUC FOCAS Library 2
├── Fanuc/
│   ├── FocasFanucClient.cs # ✅ Real FANUC integration
│   └── MockFanucClient.cs  # ✅ Fallback for testing
```

## ⚙️ **Step 3: Configure CNC Machine Settings**

### **Update appsettings.json**
```json
{
  "UseFanuc": true,
  "HistoryWindowMinutes": 10,
  "FanucMachines": {
    "M001": {
      "IpAddress": "192.168.1.100",
      "Port": 8193,
      "Username": "FANUC",
      "Password": "FANUC",
      "Description": "CNC Machine 1 - Main Production Line"
    },
    "M002": {
      "IpAddress": "192.168.1.101",
      "Port": 8193,
      "Username": "FANUC",
      "Password": "FANUC",
      "Description": "CNC Machine 2 - Secondary Line"
    }
  }
}
```

### **Configuration Parameters**
| Parameter | Description | Example | Required |
|-----------|-------------|---------|----------|
| **IpAddress** | CNC machine IP address | `192.168.1.100` | ✅ Yes |
| **Port** | FOCAS communication port | `8193` | ✅ Yes |
| **Username** | FANUC login username | `FANUC` | ✅ Yes |
| **Password** | FANUC login password | `FANUC` | ✅ Yes |
| **Description** | Machine description | `Main Production Line` | ❌ No |

## 🔧 **Step 4: Test Network Connectivity**

### **Ping CNC Machines**
```bash
# Test basic network connectivity
ping 192.168.1.100
ping 192.168.1.101

# Test FOCAS port connectivity
telnet 192.168.1.100 8193
telnet 192.168.1.101 8193
```

### **Verify FANUC Settings**
1. **Check CNC machine network settings**
2. **Verify FOCAS communication is enabled**
3. **Confirm username/password credentials**
4. **Test FOCAS connection from machine**

## 🚀 **Step 5: Enable FANUC Integration**

### **Set Configuration Flag**
```json
{
  "UseFanuc": true  // Change from false to true
}
```

### **Restart Application**
```bash
# Stop current API
Ctrl+C

# Rebuild and restart
dotnet build
dotnet run
```

## 🧪 **Step 6: Test Real FANUC Integration**

### **Test API Endpoints**
```bash
# Test machine list (should show real data)
GET http://localhost:5217/api/machines

# Test real machine refresh
POST http://localhost:5217/api/machines/M001/refresh

# Test machine history
GET http://localhost:5217/api/machines/M001/history?minutes=15
```

### **Expected Results**
- ✅ **Real machine status** from FANUC controller
- ✅ **Actual production counts** from machine
- ✅ **Real cycle times** from CNC parameters
- ✅ **Live position data** from machine axes
- ✅ **Real-time alarm information** if any

## 🔍 **Step 7: Monitor and Debug**

### **Check Application Logs**
```bash
# Look for FANUC connection messages
dotnet run --environment Development
```

### **Common Log Messages**
```
✅ "Successfully connected to machine M001 with handle 12345"
✅ "Successfully read data from machine M001: Status=Running, Production=1234"
⚠️ "Connection to machine M001 lost, recreating"
❌ "Failed to connect to machine M001: Authentication failed (Code: -9)"
```

### **Troubleshooting Common Issues**

#### **Issue 1: Connection Failed**
```
Error: "FANUC connection failed: Authentication failed (Code: -9)"
Solution: Check username/password in CNC machine settings
```

#### **Issue 2: Network Timeout**
```
Error: "FANUC connection failed: Timeout (Code: -5)"
Solution: Verify network connectivity and firewall settings
```

#### **Issue 3: Library Not Found**
```
Error: "Unable to load DLL 'Focas1.dll'"
Solution: Ensure FOCAS libraries are in the correct lib folder
```

## 📊 **Step 8: Verify Real Data**

### **Compare Simulated vs Real Data**
| Data Type | Simulated | Real FANUC |
|-----------|-----------|------------|
| **Status** | Markov chain | Actual machine state |
| **Production** | Incremental | Real part count |
| **Cycle Time** | ±10% noise | Actual machine time |
| **Position** | N/A | Real axis positions |
| **Alarms** | N/A | Actual machine alarms |

### **Benefits of Real Integration**
- ✅ **Live production monitoring**
- ✅ **Real-time status updates**
- ✅ **Actual machine data**
- ✅ **Production planning**
- ✅ **Quality control**
- ✅ **Maintenance scheduling**

## 🔄 **Step 9: Fallback Configuration**

### **Keep Simulation Available**
```json
{
  "UseFanuc": false,  // Fallback to simulation
  "FanucMachines": {  // Keep configuration for later
    "M001": { ... }
  }
}
```

### **Switch Between Modes**
- **Development/Testing**: `"UseFanuc": false` (uses MockFanucClient)
- **Production**: `"UseFanuc": true` (uses FocasFanucClient)

## 📚 **Additional Resources**

### **FANUC Documentation**
- [FANUC FOCAS Manual](https://www.fanuc.eu/ro/en/support/documentation)
- [FOCAS API Reference](https://www.fanuc.eu/ro/en/support/software)
- [Network Configuration Guide](https://www.fanuc.eu/ro/en/support/network)

### **Troubleshooting**
- [FANUC Support Portal](https://www.fanuc.eu/ro/en/support)
- [FOCAS Error Codes](https://www.fanuc.eu/ro/en/support/error-codes)
- [Network Troubleshooting](https://www.fanuc.eu/ro/en/support/network-troubleshooting)

## 🎉 **Success Criteria**

You've successfully integrated real FANUC CNC machines when:

1. ✅ **FOCAS libraries** are properly installed
2. ✅ **Machine configuration** is set in appsettings.json
3. ✅ **UseFanuc** is set to `true`
4. ✅ **API endpoints** return real machine data
5. ✅ **Production counts** show actual values
6. ✅ **Machine status** reflects real controller state
7. ✅ **Cycle times** are actual machine measurements
8. ✅ **No connection errors** in application logs

## 🚀 **Next Steps**

After successful FANUC integration:

1. **Monitor real production data**
2. **Set up production dashboards**
3. **Implement alert systems**
4. **Add machine maintenance tracking**
5. **Create production reports**
6. **Integrate with MES systems**

---

**🎯 Your CNC Machine Tracker is now connected to real FANUC controllers!**

For additional support or questions, refer to the FANUC documentation or contact your system administrator.
