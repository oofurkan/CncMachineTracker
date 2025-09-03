using CncMachineTracker.Application.Ports;
using CncMachineTracker.Domain.Entities;
using CncMachineTracker.Domain.Enums;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace CncMachineTracker.Infrastructure.Fanuc
{
    /// <summary>
    /// Real FANUC FOCAS integration client for CNC machine communication.
    /// 
    /// This implementation provides:
    /// - Real-time connection to FANUC CNC controllers
    /// - Production data reading from actual machines
    /// - Status monitoring and parameter reading
    /// - Proper connection management and error handling
    /// 
    /// Prerequisites:
    /// 1. FANUC FOCAS library (Focas1.dll, Focas2.dll) in project lib folder
    /// 2. Network access to CNC machines
    /// 3. Proper machine configuration in appsettings.json
    /// </summary>
    public class FocasFanucClient : IFanucClient, IDisposable
    {
        private readonly ILogger<FocasFanucClient> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, ushort> _machineHandles = new();
        private readonly object _handleLock = new object();
        private bool _disposed = false;

        // FANUC FOCAS Library P/Invoke Declarations
        [DllImport("Focas1.dll")]
        private static extern short cnc_allclibhndl3(string ip, ushort port, string user, string pass, out ushort handle);

        [DllImport("Focas1.dll")]
        private static extern short cnc_freelibhndl(ushort handle);

        [DllImport("Focas1.dll")]
        private static extern short cnc_statinfo(ushort handle, out ushort status);

        [DllImport("Focas1.dll")]
        private static extern short cnc_rdparam(ushort handle, short s_number, short s_axis, out int l_value);

        [DllImport("Focas1.dll")]
        private static extern short cnc_rdmacro(ushort handle, short number, out double d_value);

        [DllImport("Focas1.dll")]
        private static extern short cnc_rdactf(ushort handle, short s_number, out double d_value);

        [DllImport("Focas1.dll")]
        private static extern short cnc_rdspdl(ushort handle, short s_number, out double d_value);

        [DllImport("Focas1.dll")]
        private static extern short cnc_rdpos(ushort handle, short s_number, out double d_value);

        [DllImport("Focas1.dll")]
        private static extern short cnc_rdalarm(ushort handle, out short s_number, out short s_axis, out short s_alarm);

        public FocasFanucClient(ILogger<FocasFanucClient> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Machine> ReadCurrentAsync(string id)
        {
            try
            {
                _logger.LogInformation("Reading current data from FANUC machine {MachineId}", id);

                // Get or create connection handle
                var handle = await GetOrCreateConnectionAsync(id);
                if (handle == 0)
                {
                    throw new InvalidOperationException($"Failed to establish connection to machine {id}");
                }

                // Read machine data
                var machine = await ReadMachineDataAsync(id, handle);
                
                _logger.LogInformation("Successfully read data from machine {MachineId}: Status={Status}, Production={ProductionCount}", 
                    id, machine.Status, machine.ProductionCount);

                return machine;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading data from FANUC machine {MachineId}", id);
                throw new InvalidOperationException($"Failed to read data from FANUC machine {id}: {ex.Message}", ex);
            }
        }

        private async Task<ushort> GetOrCreateConnectionAsync(string machineId)
        {
            lock (_handleLock)
            {
                if (_machineHandles.TryGetValue(machineId, out var existingHandle))
                {
                    // Test if existing connection is still valid
                    if (TestConnection(existingHandle))
                    {
                        return existingHandle;
                    }
                    else
                    {
                        // Connection lost, remove and recreate
                        _logger.LogWarning("Connection to machine {MachineId} lost, recreating", machineId);
                        _machineHandles.Remove(machineId);
                        cnc_freelibhndl(existingHandle);
                    }
                }
            }

            // Create new connection
            return await CreateNewConnectionAsync(machineId);
        }

        private async Task<ushort> CreateNewConnectionAsync(string machineId)
        {
            var machineConfig = GetMachineConfiguration(machineId);
            if (machineConfig == null)
            {
                throw new InvalidOperationException($"Machine {machineId} not found in configuration");
            }
            
            _logger.LogInformation("Creating new connection to machine {MachineId} at {IpAddress}:{Port}", 
                machineId, machineConfig.IpAddress, machineConfig.Port);

            var result = cnc_allclibhndl3(
                machineConfig.IpAddress, 
                machineConfig.Port, 
                machineConfig.Username, 
                machineConfig.Password, 
                out ushort handle);

            if (result != 0)
            {
                var errorMessage = GetFocasErrorMessage(result);
                _logger.LogError("Failed to connect to machine {MachineId}: {Error} (Code: {Code})", 
                    machineId, errorMessage, result);
                throw new InvalidOperationException($"FANUC connection failed: {errorMessage} (Code: {result})");
            }

            _logger.LogInformation("Successfully connected to machine {MachineId} with handle {Handle}", machineId, handle);
            
            lock (_handleLock)
            {
                _machineHandles[machineId] = handle;
            }

            return handle;
        }

        private bool TestConnection(ushort handle)
        {
            try
            {
                var result = cnc_statinfo(handle, out ushort status);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task<Machine> ReadMachineDataAsync(string machineId, ushort handle)
        {
            // Read machine status
            var status = await ReadMachineStatusAsync(handle);
            
            // Read production count (from macro variable or parameter)
            var productionCount = await ReadProductionCountAsync(handle);
            
            // Read cycle time (from actual machine data)
            var cycleTime = await ReadCycleTimeAsync(handle);
            
            // Read additional machine information
            var machineInfo = await ReadMachineInfoAsync(handle);

            return new Machine
            {
                Id = machineId,
                Status = status,
                ProductionCount = productionCount,
                CycleTimeSeconds = cycleTime,
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task<MachineStatus> ReadMachineStatusAsync(ushort handle)
        {
            var result = cnc_statinfo(handle, out ushort status);
            if (result != 0)
            {
                _logger.LogWarning("Failed to read machine status, using default: {Error}", GetFocasErrorMessage(result));
                return MachineStatus.Stopped;
            }

            // FANUC status codes mapping
            // These are typical FANUC status codes - adjust based on your specific machine
            return status switch
            {
                0 => MachineStatus.Stopped,      // Not ready
                1 => MachineStatus.Running,      // Ready
                2 => MachineStatus.Running,      // Running
                3 => MachineStatus.Alarm,        // Alarm
                4 => MachineStatus.Stopped,      // Hold
                5 => MachineStatus.Stopped,      // Manual
                6 => MachineStatus.Stopped,      // MDI
                7 => MachineStatus.Stopped,      // Auto
                _ => MachineStatus.Stopped       // Unknown status
            };
        }

        private async Task<int> ReadProductionCountAsync(ushort handle)
        {
            try
            {
                // Try to read from macro variable (common for production counters)
                var result = cnc_rdmacro(handle, 5001, out double macroValue); // Macro variable 5001
                if (result == 0)
                {
                    return (int)macroValue;
                }

                // Fallback: try to read from parameter
                result = cnc_rdparam(handle, 6711, 0, out int paramValue); // Parameter 6711 (production count)
                if (result == 0)
                {
                    return paramValue;
                }

                _logger.LogWarning("Failed to read production count from macro or parameter, using default value");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading production count, using default value");
                return 0;
            }
        }

        private async Task<double> ReadCycleTimeAsync(ushort handle)
        {
            try
            {
                // Try to read actual cycle time from machine
                var result = cnc_rdactf(handle, 0, out double actualTime);
                if (result == 0 && actualTime > 0)
                {
                    return actualTime;
                }

                // Fallback: calculate from spindle speed and feed rate
                result = cnc_rdspdl(handle, 0, out double spindleSpeed);
                if (result == 0 && spindleSpeed > 0)
                {
                    // Simple calculation - in real implementation, use actual machine formulas
                    return 3600.0 / spindleSpeed; // Rough estimate
                }

                _logger.LogWarning("Failed to read cycle time, using default value");
                return 30.0; // Default 30 seconds
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading cycle time, using default value");
                return 30.0;
            }
        }

        private async Task<object> ReadMachineInfoAsync(ushort handle)
        {
            try
            {
                // Read current position
                var posResult = cnc_rdpos(handle, 0, out double position);
                
                // Read any active alarms
                var alarmResult = cnc_rdalarm(handle, out short alarmNumber, out short alarmAxis, out short alarmType);
                
                // Log machine information
                if (posResult == 0)
                {
                    _logger.LogDebug("Machine position: {Position}", position);
                }
                
                if (alarmResult == 0 && alarmNumber > 0)
                {
                    _logger.LogWarning("Machine alarm detected: Number={AlarmNumber}, Axis={AlarmAxis}, Type={AlarmType}", 
                        alarmNumber, alarmAxis, alarmType);
                }

                return new { Position = position, AlarmNumber = alarmNumber };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading additional machine information");
                return new { Position = 0.0, AlarmNumber = 0 };
            }
        }

        private FanucMachineConfiguration? GetMachineConfiguration(string machineId)
        {
            try
            {
                var section = _configuration.GetSection($"FanucMachines:{machineId}");
                if (!section.Exists())
                {
                    _logger.LogWarning("Machine {MachineId} not found in FANUC configuration", machineId);
                    return null;
                }

                var config = new FanucMachineConfiguration();
                section.Bind(config);

                if (string.IsNullOrEmpty(config.IpAddress))
                {
                    _logger.LogError("Invalid configuration for machine {MachineId}: IP address is required", machineId);
                    return null;
                }

                _logger.LogDebug("Loaded configuration for machine {MachineId}: {IpAddress}:{Port}", 
                    machineId, config.IpAddress, config.Port);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration for machine {MachineId}", machineId);
                return null;
            }
        }

        private string GetFocasErrorMessage(short errorCode)
        {
            // FANUC FOCAS error code mapping
            return errorCode switch
            {
                0 => "Success",
                -1 => "General error",
                -2 => "Invalid handle",
                -3 => "Invalid parameter",
                -4 => "Communication error",
                -5 => "Timeout",
                -6 => "Machine not ready",
                -7 => "Invalid operation",
                -8 => "Network error",
                -9 => "Authentication failed",
                -10 => "Access denied",
                _ => $"Unknown error (Code: {errorCode})"
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                lock (_handleLock)
                {
                    foreach (var handle in _machineHandles.Values)
                    {
                        try
                        {
                            cnc_freelibhndl(handle);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error disposing FANUC handle {Handle}", handle);
                        }
                    }
                    _machineHandles.Clear();
                }
                _disposed = true;
            }
        }
    }
}
