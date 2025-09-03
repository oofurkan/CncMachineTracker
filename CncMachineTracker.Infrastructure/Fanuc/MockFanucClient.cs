using CncMachineTracker.Application.Ports;
using CncMachineTracker.Domain.Entities;
using CncMachineTracker.Domain.Enums;

namespace CncMachineTracker.Infrastructure.Fanuc
{
    public class MockFanucClient : IFanucClient
    {
        private readonly Random _random = new();

        public Task<Machine> ReadCurrentAsync(string id)
        {
            // Simulate realistic FANUC data but mark it as "from device"
            var status = (MachineStatus)_random.Next(0, 3);
            var productionCount = _random.Next(100, 1000);
            var cycleTime = status == MachineStatus.Running ? _random.Next(25, 41) : 0;

            var machine = new Machine
            {
                Id = id,
                Status = status,
                ProductionCount = productionCount,
                CycleTimeSeconds = cycleTime,
                Timestamp = DateTime.UtcNow
            };

            return Task.FromResult(machine);
        }
    }
}
