using CleanArchitecture.Application;
using CleanArchitecture.Persistence;
using CleanArchitecture.Storage;
using CleanArchitecture.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.IdentityModel.Tokens.Jwt;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.UI.Web.BackgroundTask;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.UI.Web
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

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Add services to the container.
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

            builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // The claim in the Jwt token where App roles are available.
                options.TokenValidationParameters.RoleClaimType = "roles";
                // Mapping the roles claim to the role property
                options.ClaimActions.MapJsonKey(options.TokenValidationParameters.RoleClaimType, options.TokenValidationParameters.RoleClaimType);
            });

            builder.Services.AddAuthorization();

            // The following line enables Application Insights telemetry collection.
            builder.Services.AddApplicationInsightsTelemetry();

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();

            builder.Services.AddHostedService<PublishDocumentsBackgroundTask>();

            var app = builder.Build();

            app.Services.CreateScope().ServiceProvider.GetRequiredService<CleanArchitectureDbContext>().Database.Migrate();

            app.UseExceptionHandler("/Home/Error");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            //app.UseCustomExceptionHandler();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}