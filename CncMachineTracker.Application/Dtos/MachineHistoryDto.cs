namespace CncMachineTracker.Application.Dtos
{
    public class MachineHistoryDto
    {
        public string Id { get; set; } = string.Empty;
        public List<MachineDto> Samples { get; set; } = new();
    }
}
