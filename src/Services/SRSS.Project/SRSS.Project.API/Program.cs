using Microsoft.OpenApi.Models;
using Shared.DependencyInjection;
using SRSS.Project.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var environment = builder.Environment.EnvironmentName;
var connectionString = config.GetConnectionString("SRSS_IAM_DB");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGenForAuthentication();

builder.Services.AddInfrastructureServices(config);

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 Application starting in {Environment} environment", environment);
logger.LogInformation("📦 SQL Server connection string: {Connection}", connectionString);
logger.LogInformation("🔗 Redis connection: {Redis}", config.GetConnectionString("Redis"));


app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        // Hỗ trợ cả Direct API access và Gateway access
        // Direct API làm mặc định (first in list) để test
        swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}", Description = "Direct API Access (Use this for testing)" },
                        new OpenApiServer { Url = "/project", Description = "Via API Gateway (Production)" }
                    };
    });
});
app.UseSwaggerUI();

app.UseInfrastructurePolicy();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
