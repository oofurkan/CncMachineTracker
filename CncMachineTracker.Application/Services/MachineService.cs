using CncMachineTracker.Application.Ports;
using CncMachineTracker.Domain.Entities;
using CncMachineTracker.Domain.Enums;

namespace CncMachineTracker.Application.Services
{
    public class MachineService
    {
        private readonly IMachineRepository _repository;
        private readonly IFanucClient? _fanucClient;
        private readonly Random _random = new();
        private readonly Dictionary<string, MachineBaseline> _machineBaselines = new();

        public MachineService(IMachineRepository repository, IFanucClient? fanucClient = null)
        {
            _repository = repository;
            _fanucClient = fanucClient;
        }

        public async Task<IReadOnlyList<Machine>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Machine?> GetLatestAsync(string id)
        {
            return await _repository.GetLatestAsync(id);
        }

        public async Task<IReadOnlyList<MachineSample>> GetHistoryAsync(string id, TimeSpan window)
        {
            return await _repository.GetHistoryAsync(id, window);
        }

        public async Task<Machine> SimulateAsync(string id)
        {
            // Ensure machine exists with baseline
            await EnsureMachineBaselineAsync(id);

            var baseline = _machineBaselines[id];
            var currentMachine = await _repository.GetLatestAsync(id) ?? baseline.InitialMachine;

            // Simulate realistic state transitions
            var newStatus = SimulateStatusTransition(currentMachine.Status);
            var newProductionCount = SimulateProductionCount(currentMachine.ProductionCount, newStatus);
            var newCycleTime = SimulateCycleTime(baseline.BaseCycleTimeSeconds, newStatus);

            var updatedMachine = new Machine
            {
                Id = id,
                Status = newStatus,
                ProductionCount = newProductionCount,
                CycleTimeSeconds = newCycleTime,
                Timestamp = DateTime.UtcNow
            };

            var sample = new MachineSample
            {
                MachineId = id,
                Status = newStatus,
                ProductionCount = newProductionCount,
                CycleTimeSeconds = newCycleTime,
                Timestamp = DateTime.UtcNow
            };

            await _repository.UpsertAsync(updatedMachine, sample);
            return updatedMachine;
        }

        public async Task<Machine> RefreshFromFanucAsync(string id)
        {
            if (_fanucClient == null)
            {
                throw new InvalidOperationException("FANUC client is not configured. Set UseFanuc=true in configuration.");
            }

            var machine = await _fanucClient.ReadCurrentAsync(id);
            var sample = new MachineSample
            {
                MachineId = id,
                Status = machine.Status,
                ProductionCount = machine.ProductionCount,
                CycleTimeSeconds = machine.CycleTimeSeconds,
                Timestamp = machine.Timestamp
            };

            await _repository.UpsertAsync(machine, sample);
            return machine;
        }

        private async Task EnsureMachineBaselineAsync(string id)
        {
            if (!_machineBaselines.ContainsKey(id))
            {
                var initialMachine = await _repository.GetLatestAsync(id);
                if (initialMachine == null)
                {
                    // Create new machine with realistic baseline
                    var baseCycleTime = _random.Next(25, 41); // 25-40 seconds
                    var initialStatus = (MachineStatus)_random.Next(0, 3);
                    var initialProductionCount = _random.Next(0, 100);

                    initialMachine = new Machine
                    {
                        Id = id,
                        Status = initialStatus,
                        ProductionCount = initialProductionCount,
                        CycleTimeSeconds = baseCycleTime,
                        Timestamp = DateTime.UtcNow
                    };

                    await _repository.EnsureExistsAsync(id, initialMachine);
                }

                _machineBaselines[id] = new MachineBaseline
                {
                    BaseCycleTimeSeconds = initialMachine.CycleTimeSeconds,
                    InitialMachine = initialMachine
                };
            }
        }

        private MachineStatus SimulateStatusTransition(MachineStatus currentStatus)
        {
            var rand = _random.NextDouble();

            // Markov chain: prefer staying in same state
            switch (currentStatus)
            {
                case MachineStatus.Running:
                    if (rand < 0.80) return MachineStatus.Running;      // 80% stay running
                    if (rand < 0.95) return MachineStatus.Stopped;     // 15% go to stopped
                    return MachineStatus.Alarm;                         // 5% go to alarm

                case MachineStatus.Stopped:
                    if (rand < 0.80) return MachineStatus.Stopped;     // 80% stay stopped
                    if (rand < 0.95) return MachineStatus.Running;     // 15% go to running
                    return MachineStatus.Alarm;                         // 5% go to alarm

                case MachineStatus.Alarm:
                    if (rand < 0.70) return MachineStatus.Alarm;       // 70% stay in alarm
                    if (rand < 0.85) return MachineStatus.Stopped;     // 15% go to stopped
                    return MachineStatus.Running;                       // 15% go to running

                default:
                    return MachineStatus.Stopped;
            }
        }

        private int SimulateProductionCount(int currentCount, MachineStatus newStatus)
        {
            switch (newStatus)
            {
                case MachineStatus.Running:
                    return currentCount + _random.Next(1, 6); // +1 to +5 parts
                case MachineStatus.Stopped:
                    // Small chance to finish a part
                    return currentCount + (_random.NextDouble() < 0.1 ? 1 : 0);
                case MachineStatus.Alarm:
                    return currentCount; // No production during alarm
                default:
                    return currentCount;
            }
        }

        private double SimulateCycleTime(double baseCycleTime, MachineStatus newStatus)
        {
            if (newStatus != MachineStatus.Running)
                return 0; // No cycle time when not running

            // Normal distribution around base with ±10% noise, clamped to reasonable range
            var noise = (_random.NextDouble() - 0.5) * 0.2; // ±10%
            var newCycleTime = baseCycleTime * (1 + noise);
            
            return Math.Max(10, Math.Min(120, Math.Round(newCycleTime, 1))); // Clamp to 10-120 seconds
        }

        private class MachineBaseline
        {
            public double BaseCycleTimeSeconds { get; set; }
            public Machine InitialMachine { get; set; } = null!;
        }
    }
}
