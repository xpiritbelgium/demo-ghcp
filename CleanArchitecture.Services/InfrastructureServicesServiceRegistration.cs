using CleanArchitecture.Application;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;

namespace CleanArchitecture.Services
{
    public static class InfrastructureServicesServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServicesServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureClients(cfg =>
            {
                cfg.AddEmailClient(configuration.GetSection("Email")).WithCredential(new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    SharedTokenCacheTenantId = "3d4d17ea-1ae4-4705-947e-51369c5a5f79"
                }));
            });

            services.Configure<EmailClientSettings>(configuration.GetSection("Email"));

            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
