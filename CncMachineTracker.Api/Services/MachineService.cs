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

			// Generate a machine ID - sometimes use existing ones, sometimes create new ones
			string machineId;
			if (_machines.Count > 0 && rnd.Next(0, 100) < 60) // 60% chance to update existing machine
			{
				// Pick a random existing machine to update
				machineId = _machines[rnd.Next(_machines.Count)].Id;
			}
			else
			{
				// Create a new machine ID
				machineId = $"M{rnd.Next(100, 999)}";
			}

			// Check if machine with this ID already exists
			var existingMachine = _machines.FirstOrDefault(m => m.Id.Equals(machineId, StringComparison.OrdinalIgnoreCase));

			if (existingMachine != null)
			{
				// Update existing machine with new random data
				existingMachine.Status = statuses[rnd.Next(statuses.Length)];
				existingMachine.ProductionCount = rnd.Next(0, 1000);
				existingMachine.CycleTime = Math.Round(rnd.NextDouble() * 60, 1);
				existingMachine.Timestamp = DateTime.Now;
				
				return existingMachine;
			}
			else
			{
				// Create new machine
				var newMachine = new Machine
				{
					Id = machineId,
					Status = statuses[rnd.Next(statuses.Length)],
					ProductionCount = rnd.Next(0, 1000),
					CycleTime = Math.Round(rnd.NextDouble() * 60, 1),
					Timestamp = DateTime.Now
				};

				_machines.Add(newMachine);
				return newMachine;
			}
		}
	}
}
