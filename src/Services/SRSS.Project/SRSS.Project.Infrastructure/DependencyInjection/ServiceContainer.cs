using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Cache;
using Shared.DependencyInjection;
using SRSS.Project.Infrastructure.ProjectRepo;
using SRSS.Project.Infrastructure.Repositories;

namespace SRSS.Project.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            // Infrastructure-specific service registrations go here
            //Add database connectivity
            services.AddDbContext<Data.ProjectDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("SRSS_Project_DB"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));
            //add redis before because jwt blacklist middleware depends on it
            services.AddRedisCacheWithHealthCheck(config);
            //add authentication scheme
            //Global exception handling middleware
            SharedServiceContainer.AddSharedServices(services, config, config["MySerilog:FileName"]!);
            //create dependecy injection (DI)
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProjectRepository, ProjectRepository>();

            return services;
        }

        public static IApplicationBuilder UseInfrastructurePolicy(this IApplicationBuilder app)
        {
            //Register middleware such as : 
            //Listen to only api gateway
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }
    }
}
