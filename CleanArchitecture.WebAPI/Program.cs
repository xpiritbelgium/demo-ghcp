using CleanArchitecture.WebAPI.Middleware;
using CleanArchitecture.Application;
using CleanArchitecture.Persistence;
using CleanArchitecture.Storage;
using CleanArchitecture.Services;
using CleanArchitecture.Application.Extensions;

namespace CleanArchitecture.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IApplicationUserAccessor, ApplicationUserAccessor>();
            builder.Services.AddScoped<IUriHelper, UriHelper>();
            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddStorageServices(builder.Configuration);
            builder.Services.AddInfrastructureServicesServices(builder.Configuration);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCustomExceptionHandler();

            app.MapControllers();

            app.Run();
        }
    }
}