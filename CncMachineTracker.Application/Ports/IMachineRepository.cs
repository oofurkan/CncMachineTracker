using CncMachineTracker.Domain.Entities;

namespace CncMachineTracker.Application.Ports
{
    public interface IMachineRepository
    {
        Task<IReadOnlyList<Machine>> GetAllAsync();
        Task<Machine?> GetLatestAsync(string id);
        Task<IReadOnlyList<MachineSample>> GetHistoryAsync(string id, TimeSpan window);
        Task UpsertAsync(Machine latest, MachineSample sample);
        Task EnsureExistsAsync(string id, Machine initial);
    }
}
