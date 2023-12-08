using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Application;
using CleanArchitecture.Persistence;
using CleanArchitecture.Storage;
using CleanArchitecture.Services;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.WebJob
{
    internal class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddTimers();
            });

            builder.ConfigureLogging(b =>
             {
                 b.SetMinimumLevel(LogLevel.Information);
                 b.AddConsole();
             });

            builder.ConfigureServices((context, b) =>
            {
                b.AddScoped<IApplicationUserAccessor, ApplicationUserAccessor>();
                b.AddScoped<IUriHelper, UriHelper>();
                b.AddPersistenceServices(context.Configuration);
                b.AddApplicationServices();
                b.AddStorageServices(context.Configuration);
                b.AddInfrastructureServicesServices(context.Configuration);
            });
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
