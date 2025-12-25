using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =================== 🔐 AUTHENTICATION SETUP ===================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["JwtSettings:validIssuer"],   // ✅ SRSSAPI
        ValidAudience = builder.Configuration["JwtSettings:validAudience"], // ✅ SRSSClient

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["JwtSettings:secretKey"]!
            )
        )
    };
});

builder.Services
    .AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddBearerToken();


// =================== ✅ ROLE-BASED AUTHORIZATION ===================
builder.Services.AddAuthorizationBuilder()
                        .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
                        .AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));

//builder.Services.AddAuthorization();

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
    options.SwaggerEndpoint("/iam/swagger/v1/swagger.json", "IAM Service v1");
    options.RoutePrefix = string.Empty;
});

app.UseRouting();

app.UseCors();           // ✅ CORS TRƯỚC AUTH
app.UseRateLimiter();   // ✅ Rate limit sau routing

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("login", (bool firstApi = false, bool secondApi = false) => //for testing
    Results.SignIn(
        new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim("sub", Guid.NewGuid().ToString()),
                    //new Claim("first-api-access", firstApi.ToString()),
                    //new Claim("second-api-access", secondApi.ToString())
                ],
                BearerTokenDefaults.AuthenticationScheme)),
        authenticationScheme: BearerTokenDefaults.AuthenticationScheme));

app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value!.ToLower();

        // ✅ IAM → public
        if (path.StartsWith("/iam"))
        {
            await next();
            return;
        }

        // ✅ Mặc định: phải login
        var authResult = await context.AuthenticateAsync();
        if (!authResult.Succeeded)
        {
            context.Response.StatusCode = 401;
            return;
        }

        // ✅ Project → AdminOnly
        //if (path.StartsWith("/project") && !context.User.IsInRole("Admin"))
        //{
        //    context.Response.StatusCode = 403;
        //    return;
        //}

        await next();
    });
});


app.Run();
