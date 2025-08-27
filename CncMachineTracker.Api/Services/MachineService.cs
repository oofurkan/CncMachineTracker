using CncMachineTracker.Api.Models;

namespace CncMachineTracker.Api.Services
{
	public class MachineService : IMachineService
	{
		private readonly List<Machine> _machines = new();

		public MachineService()
		{
			_machines.AddRange(new List<Machine>
			{
				new Machine { Id = "M001", Status = "Çalışıyor", ProductionCount = 100, CycleTime = 30, Timestamp = DateTime.Now },
				new Machine { Id = "M002", Status = "Duruşta",   ProductionCount = 50,  CycleTime = 45, Timestamp = DateTime.Now },
				new Machine { Id = "M003", Status = "Alarm",     ProductionCount = 0,   CycleTime = 0,  Timestamp = DateTime.Now }
			});
		}

		public List<Machine> GetAll() => _machines;

		public Machine? GetById(string id) =>
			_machines.FirstOrDefault(m => m.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

		public Machine Simulate()
		{
			var rnd = new Random();
			var statuses = new[] { "Çalışıyor", "Duruşta", "Alarm" };

			var machine = new Machine
			{
				Id = $"M{rnd.Next(100, 999)}",
				Status = statuses[rnd.Next(statuses.Length)],
				ProductionCount = rnd.Next(0, 1000),
				CycleTime = Math.Round(rnd.NextDouble() * 60, 1),
				Timestamp = DateTime.Now
			};

			_machines.Add(machine);
			return machine;
		}
	}
}
