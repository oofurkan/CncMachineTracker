using CncMachineTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CncMachineTracker.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<MachineService>();
            return services;
        }
    }
}
