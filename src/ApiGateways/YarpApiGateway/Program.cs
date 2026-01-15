using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Shared.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// =================== 🔐 AUTHENTICATION SETUP ===================
builder.Services.AddJwtAuthenticationScheme(configuration);

// =================== 🌐 YARP & SWAGGER ===================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});




// =================== ⏱ RATE LIMITER ===================
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 5;
    });
});

var app = builder.Build();

// =================== 🧭 MIDDLEWARE PIPELINE ===================
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        // Sửa host Swagger để là Gateway URL
        var serverUrl = $"{httpReq.Scheme}://{httpReq.Host.Value}";
        swaggerDoc.Servers = new List<OpenApiServer> { new() { Url = serverUrl } };
    });
});

app.UseSwaggerUI(options =>
{
    // Load swagger services from configuration
    var swaggerServices = configuration.GetSection("SwaggerServices").Get<List<SwaggerServiceConfig>>();
    
    if (swaggerServices != null && swaggerServices.Any())
    {
        foreach (var service in swaggerServices)
        {
            options.SwaggerEndpoint(service.Endpoint, $"{service.Name} {service.Version}");
        }
    }
    
    options.RoutePrefix = string.Empty; // Swagger UI được mount tại ROOT /
});


app.UseRouting();

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();


app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value!.ToLower();

        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next();
            return;
        }

        if (path.Contains("/swagger"))
        {
            await next();
            return;
        }

        // ✅ IAM → public
        if (path.StartsWith("/iam"))
        {
            await next();
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = 401;
            return;
        }

        await next();
    });
});


app.Run();


// Configuration model for Swagger services
public class SwaggerServiceConfig
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}
