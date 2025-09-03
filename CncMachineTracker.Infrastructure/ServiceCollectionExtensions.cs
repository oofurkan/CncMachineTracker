using CncMachineTracker.Application.Ports;
using CncMachineTracker.Infrastructure.Fanuc;
using CncMachineTracker.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CncMachineTracker.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCncInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register repository
            services.AddSingleton<IMachineRepository, InMemoryMachineRepository>();

            // Conditionally register FANUC client based on configuration
            var useFanuc = configuration.GetValue<bool>("UseFanuc", false);
            
            if (useFanuc)
            {
                services.AddScoped<IFanucClient, FocasFanucClient>();
            }
            else
            {
                services.AddScoped<IFanucClient, MockFanucClient>();
            }

            return services;
        }
    }
}
