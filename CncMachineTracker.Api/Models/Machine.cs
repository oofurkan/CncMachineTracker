namespace CncMachineTracker.Api.Models
{
	public class Machine
	{
		public string Id { get; set; } = string.Empty;
		public string Status { get; set; } = "Çalışıyor"; // Çalışıyor, Duruşta, Alarm
		public int ProductionCount { get; set; }
		public double CycleTime { get; set; } // saniye
		public DateTime Timestamp { get; set; } = DateTime.Now;
	}
}
