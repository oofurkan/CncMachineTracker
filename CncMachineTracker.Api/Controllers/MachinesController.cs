using Microsoft.AspNetCore.Mvc;
using CncMachineTracker.Api.Services;
using CncMachineTracker.Api.Models;

namespace CncMachineTracker.Api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MachinesController : ControllerBase
	{
		private readonly IMachineService _machineService;

		public MachinesController(IMachineService machineService)
		{
			_machineService = machineService;
		}

		[HttpGet]
		public ActionResult<List<Machine>> GetAll()
		{
			return Ok(_machineService.GetAll());
		}

		[HttpGet("{id}")]
		public ActionResult<Machine> GetById(string id)
		{
			var machine = _machineService.GetById(id);
			if (machine == null) return NotFound();
			return Ok(machine);
		}

		[HttpPost("simulate")]
		public ActionResult<Machine> Simulate()
		{
			var machine = _machineService.Simulate();
			return Ok(machine);
		}
	}
}
