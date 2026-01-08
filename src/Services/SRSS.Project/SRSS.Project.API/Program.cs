using Microsoft.OpenApi.Models;
using Shared.DependencyInjection;
using SRSS.Project.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGenForAuthentication();

builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();



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
