using CleanArchitecture.Application;
using CleanArchitecture.Persistence.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CleanArchitectureDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("CleanArchitectureConnectionsString")));

            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            services.AddScoped(typeof(IAsyncReadOnlyRepository<>), typeof(BaseReadOnlyRepository<>));

            services.AddScoped<IDocumentsRepository, DocumentsRepository>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

            return services;
        }

    }
}
