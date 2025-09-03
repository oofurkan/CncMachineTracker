using CncMachineTracker.Domain.Enums;

namespace CncMachineTracker.Domain.Entities
{
    public class MachineSample
    {
        public string MachineId { get; set; } = string.Empty;
        public MachineStatus Status { get; set; }
        public int ProductionCount { get; set; }
        public double CycleTimeSeconds { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
