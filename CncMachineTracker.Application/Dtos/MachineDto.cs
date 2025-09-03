namespace CncMachineTracker.Application.Dtos
{
    public class MachineDto
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ProductionCount { get; set; }
        public double CycleTimeSeconds { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
