using Microsoft.OpenApi.Models;
using Shared.DependencyInjection;
using SRSS.Project.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var environment = builder.Environment.EnvironmentName;
var connectionString = config.GetConnectionString("SRSS_Project_DB");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGenForAuthentication();

builder.Services.AddInfrastructureServices(config);

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Application starting in {Environment} environment", environment);
logger.LogInformation("📦 PostgreSQL connection string: {Connection}", connectionString);
logger.LogInformation("🔗 Redis connection: {Redis}", config.GetConnectionString("Redis"));


app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        var publicBaseUrl = config["Swagger:PublicBaseUrl"];
        swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = publicBaseUrl, Description = "Via API Gateway" },
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}", Description = "Direct API Access (Use this for testing)" }
                    };
    });
});
app.UseSwaggerUI();

app.UseInfrastructurePolicy();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
