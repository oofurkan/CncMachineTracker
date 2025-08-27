using CncMachineTracker.Api.Models;

namespace CncMachineTracker.Api.Services
{
	public interface IMachineService
	{
		List<Machine> GetAll();
		Machine? GetById(string id);
		Machine Simulate(); // dummy veri üret
	}
}
