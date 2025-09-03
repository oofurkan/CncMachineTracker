namespace CncMachineTracker.Infrastructure.Fanuc
{
    /// <summary>
    /// Configuration model for FANUC machine connections
    /// </summary>
    public class FanucMachineConfiguration
    {
        public string IpAddress { get; set; } = string.Empty;
        public ushort Port { get; set; } = 8193;
        public string Username { get; set; } = "FANUC";
        public string Password { get; set; } = "FANUC";
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Configuration section for all FANUC machines
    /// </summary>
    public class FanucMachinesConfiguration
    {
        public Dictionary<string, FanucMachineConfiguration> Machines { get; set; } = new();
    }
}
