using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using CncMachineTracker.Application;
using CncMachineTracker.Infrastructure;

namespace CncMachineTracker.Tests
{
    public class TestHost
    {
        public static IHost CreateHost()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<TestStartup>();
                });

            return builder.Build();
        }
    }

    public class TestStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddApplication();
            services.AddCncInfrastructure(new TestConfiguration());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class TestConfiguration : IConfiguration
    {
        public string? this[string key] 
        { 
            get => key switch
            {
                "UseFanuc" => "false",
                "HistoryWindowMinutes" => "10",
                _ => null
            };
            set { }
        }

        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
        public IConfigurationSection GetSection(string key) => new TestConfigurationSection();
        public IChangeToken GetReloadToken() => new TestChangeToken();
    }

    public class TestConfigurationSection : IConfigurationSection
    {
        public string Key => string.Empty;
        public string Path => string.Empty;
        public string? Value { get; set; }
        
        public string? this[string key]
        {
            get => null;
            set { }
        }
        
        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
        public IConfigurationSection GetSection(string key) => new TestConfigurationSection();
        public IChangeToken GetReloadToken() => new TestChangeToken();
    }

    public class TestChangeToken : IChangeToken
    {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => new TestDisposable();
    }

    public class TestDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
