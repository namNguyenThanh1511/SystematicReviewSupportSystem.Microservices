using IAM.API.Extensions;
using IAM.API.Middleware;
using IAM.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Models;
using StackExchange.Redis;


namespace IAM.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers().AddXmlSerializerFormatters();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.ConfigureSwaggerForAuthentication();
            builder.Services.ConfigureJWT(builder.Configuration);
            builder.Services.ConfigureGlobalException();

            // Configure logging (Console + Debug)
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var environment = builder.Environment.EnvironmentName;
            builder.Logging.AddConsole();

            // Database connection - CHANGED TO MYSQL
            var connectionString = builder.Configuration.GetConnectionString("IAMDb");
            builder.Services.AddDbContext<IAMDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Redis connection
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
                return ConnectionMultiplexer.Connect(redisConnection);
            });

            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors.Select(e => new ApiError { Message = e.ErrorMessage }))
                        .ToList();

                    var response = new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "D? li?u không h?p l?",
                        Errors = errors
                    };

                    return new BadRequestObjectResult(response);
                };
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.SetIsOriginAllowed(origin => true)  
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });




            var app = builder.Build();

            // ?? LOG MÔI TRU?NG VÀ CONNECTION INFO
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("?? Application starting in {Environment} environment", environment);
            logger.LogInformation("?? MySQL connection string: {Connection}", connectionString);
            logger.LogInformation("?? Redis connection: {Redis}", builder.Configuration.GetConnectionString("Redis"));

            // Apply pending migrations automatically
            if (app.Environment.IsEnvironment("Development") || app.Environment.IsEnvironment("Production"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IAMDbContext>();

                    try
                    {
                        if (db.Database.GetPendingMigrations().Any())
                        {
                            logger.LogInformation("?? Applying pending migrations...");
                            db.Database.Migrate();
                            logger.LogInformation("? Database migrations applied successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("?? Migration error: {Error}", ex.Message);
                    }
                }
            }

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    // Hỗ trợ cả Direct API access và Gateway access
                    // Direct API làm mặc định (first in list) để test
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}", Description = "Direct API Access (Use this for testing)" },
                        new OpenApiServer { Url = "http://localhost:5103/iam", Description = "Via API Gateway (Production)" }
                    };
                });
            });

            app.UseSwaggerUI();

            // ✅ CORRECT MIDDLEWARE ORDER FOR CORS
            app.UseHttpsRedirection();
            app.UseRouting();                                     // 1. Routing first
            app.UseCors();                                        // 2. CORS after routing
            app.UseMiddleware<JwtBlacklistMiddleware>();          // 3. Custom middleware after CORS
            app.UseAuthentication();                              // 4. Authentication
            app.UseAuthorization();                               // 5. Authorization
            app.UseExceptionHandler();                            // 6. Exception handler

            app.MapControllers();

            app.Run();
        }
    }
}
