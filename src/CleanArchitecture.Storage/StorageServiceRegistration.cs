using CleanArchitecture.Application;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;

namespace CleanArchitecture.Storage
{
    public static class StorageServiceRegistration
    {

        public static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureClients(cfg =>
            {
                cfg.AddBlobServiceClient(configuration.GetSection("Blobs")).WithCredential(new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    SharedTokenCacheTenantId = "3d4d17ea-1ae4-4705-947e-51369c5a5f79"
                }));
            });

            services.Configure<BlobClientSettings>(configuration.GetSection("Blobs"));

            services.AddScoped<IBlobRepository, BlobRepository>();

            return services;
        }
    }
}
