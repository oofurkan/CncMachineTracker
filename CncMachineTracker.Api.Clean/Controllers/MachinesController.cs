using Microsoft.AspNetCore.Mvc;
using CncMachineTracker.Application.Services;
using CncMachineTracker.Application.Dtos;
using CncMachineTracker.Domain.Enums;

namespace CncMachineTracker.Api.Clean.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachinesController : ControllerBase
    {
        private readonly MachineService _machineService;
        private readonly IConfiguration _configuration;

        public MachinesController(MachineService machineService, IConfiguration configuration)
        {
            _machineService = machineService;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all machines current snapshots
        /// </summary>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<MachineDto>))]
        public async Task<ActionResult<List<MachineDto>>> GetAll()
        {
            var machines = await _machineService.GetAllAsync();
            var dtos = machines.Select(MapToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get latest snapshot for a specific machine
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(MachineDto))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MachineDto>> GetById(string id)
        {
            var machine = await _machineService.GetLatestAsync(id);
            if (machine == null) 
                return NotFound($"Machine with ID '{id}' not found");

            return Ok(MapToDto(machine));
        }

        /// <summary>
        /// Get machine history for the last N minutes
        /// </summary>
        [HttpGet("{id}/history")]
        [ProducesResponseType(200, Type = typeof(MachineHistoryDto))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MachineHistoryDto>> GetHistory(string id, [FromQuery] int? minutes)
        {
            var machine = await _machineService.GetLatestAsync(id);
            if (machine == null) 
                return NotFound($"Machine with ID '{id}' not found");

            var windowMinutes = minutes ?? _configuration.GetValue<int>("HistoryWindowMinutes", 10);
            var window = TimeSpan.FromMinutes(windowMinutes);
            var samples = await _machineService.GetHistoryAsync(id, window);

            var historyDto = new MachineHistoryDto
            {
                Id = id,
                Samples = samples.Select(MapToDto).ToList()
            };

            return Ok(historyDto);
        }

        /// <summary>
        /// Simulate machine state changes
        /// </summary>
        [HttpPost("{id}/simulate")]
        [ProducesResponseType(200, Type = typeof(MachineDto))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MachineDto>> Simulate(string id)
        {
            try
            {
                var machine = await _machineService.SimulateAsync(id);
                return Ok(MapToDto(machine));
            }
            catch (Exception ex)
            {
                return BadRequest($"Simulation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh machine data from FANUC controller
        /// </summary>
        [HttpPost("{id}/refresh")]
        [ProducesResponseType(200, Type = typeof(MachineDto))]
        [ProducesResponseType(404)]
        [ProducesResponseType(501)]
        public async Task<ActionResult<MachineDto>> Refresh(string id)
        {
            try
            {
                var machine = await _machineService.RefreshFromFanucAsync(id);
                return Ok(MapToDto(machine));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(501, new { 
                    error = "FANUC integration not enabled", 
                    message = ex.Message,
                    instructions = "Set UseFanuc=true in appsettings.json to enable real FANUC integration"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Refresh failed: {ex.Message}");
            }
        }

        private static MachineDto MapToDto(dynamic machine)
        {
            return new MachineDto
            {
                Id = machine.Id,
                Status = GetLocalizedStatus(machine.Status),
                ProductionCount = machine.ProductionCount,
                CycleTimeSeconds = machine.CycleTimeSeconds,
                Timestamp = machine.Timestamp
            };
        }

        private static string GetLocalizedStatus(MachineStatus status)
        {
            return status switch
            {
                MachineStatus.Running => "Çalışıyor",
                MachineStatus.Stopped => "Duruşta",
                MachineStatus.Alarm => "Alarm",
                _ => status.ToString()
            };
        }
    }
}
