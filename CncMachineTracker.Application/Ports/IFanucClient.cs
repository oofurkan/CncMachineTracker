using CncMachineTracker.Domain.Entities;

namespace CncMachineTracker.Application.Ports
{
    public interface IFanucClient
    {
        Task<Machine> ReadCurrentAsync(string id);
    }
}
