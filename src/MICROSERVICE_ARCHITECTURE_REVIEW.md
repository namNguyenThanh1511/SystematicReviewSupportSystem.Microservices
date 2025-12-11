# ??? MICROSERVICE ARCHITECTURE REVIEW

## ?? Executive Summary

**Architecture Status:** ?? **PARTIAL MICROSERVICE** (55% compliance)

H? th?ng ?ang th?c hi?n ki?n trúc microservice nh?ng còn nhi?u ?i?m ch?a chu?n. Có nh?ng y?u t? t?t nh?ng c?ng có nh?ng vi ph?m nguyên t?c microservice quan tr?ng.

---

## ?? Các Tiêu Chí ?ánh Giá Microservice

### 1?? **SERVICE INDEPENDENCE** (??c l?p c?a services)

#### ? **YÊU C?U:** M?i service có database riêng bi?t
**Hi?n tr?ng:**
```
? IAM Service   ? Database: SRSS.IAM (riêng)
? Course Service ? (Gi? ??nh riêng)
? Submission Service ? (Gi? ??nh riêng)
```

**?ánh giá:** ? T?t - M?i service có database riêng

#### ? **V?N ??:** Shared JWT Secret Key
```csharp
// Gateway appsettings.json
"secretKey": "DhftOS5uphK3vmCJQrexST1RsyjZBjXWRgJMFPU4"

// IAM appsettings.json
"secretKey": "DhftOS5uphK3vmCJQrexST1RsyjZBjXWRgJMFPU4"  // ? SAME!
```

**V?n ??:**
- ?? N?u 1 service b? xâm ph?m ? Toàn b? h? th?ng ? nguy hi?m
- ? Vi ph?m nguyên t?c service isolation

**Recommendation:**
```json
{
  "JwtSettings": {
    "ServiceSecret": "iam-service-specific-secret-key",
    "SharedAudience": "SRSSAPI"
  }
}
```

---

### 2?? **API GATEWAY PATTERN**

#### ? **YÊU C?U:** Dùng API Gateway ?? route requests
**Hi?n tr?ng:**
```
? Dùng YARP (Yet Another Reverse Proxy)
? File: ApiGateways\YarpApiGateway\appsettings.json
? Gateway route t?i services
```

#### ?? **ISSUE 1: Hardcoded Service URLs**
```json
"Clusters": {
  "iam-cluster": {
    "Destinations": {
      "dest": { "Address": "http://localhost:8081/" }  // ? Hard-coded localhost
    }
  }
}
```

**V?n ??:**
- ? Không scalable - Ch? 1 instance
- ? Không thích h?p cho production/Kubernetes
- ? Khi scale ? Ph?i update config

**Fix:**
```json
"iam-cluster": {
  "Destinations": {
    "dest1": { "Address": "http://iam-service:8080/" },      // ? DNS name
    "dest2": { "Address": "http://iam-service-replica:8080/" }
  },
  "LoadBalancingPolicy": "RoundRobin"  // ? Load balancing
}
```

#### ?? **ISSUE 2: Thi?u Load Balancing Configuration**
```json
// Hi?n t?i: Ch? có 1 destination, không có load balancing
"api1-cluster": {
  "Destinations": {
    "destination1": {
      "Address": "http://localhost:8080"
    }
  }
  // ? Không có LoadBalancingPolicy
}
```

**Fix:**
```json
"api1-cluster": {
  "Destinations": {
    "dest1": { "Address": "http://localhost:8080" },
    "dest2": { "Address": "http://localhost:8081" }
  },
  "LoadBalancingPolicy": "RoundRobin"  // ? Add this
}
```

---

### 3?? **AUTHENTICATION & AUTHORIZATION**

#### ? **YÊU C?U:** Centralized Authentication
**Hi?n tr?ng:**
```
? IAM Service c?p JWT tokens
? Gateway validate token
? M?i service validate token
```

#### ? **ISSUE: JWT Secret Key chia s?**
```
Gateway appsettings.json
  ?? validIssuer: "SRSSAPI"
  ?? validAudience: "SRSSClient"
  ?? secretKey: "DhftOS5uphK3vmCJQrexST1RsyjZBjXWRgJMFPU4"

IAM appsettings.json
  ?? validIssuer: "SRSSAPI"
  ?? validAudience: "SRSSClient"
  ?? secretKey: "DhftOS5uphK3vmCJQrexST1RsyjZBjXWRgJMFPU4"  // ? Same!
```

**V?n ??:**
- ? C?n chia s? ?? validate token
- ? Nh?ng hard-coded ? B?o m?t y?u
- ?? Khi rotate key ? Ph?i update nhi?u ch?

---

### 4?? **SERVICE-TO-SERVICE COMMUNICATION**

#### ? **V?N ??:** Không có pattern ?? g?i service khác

**Hi?n tr?ng:**
```
? Không có HttpClient registered
? Không có service-to-service calls
? Không có circuit breaker pattern
? Không có retry policy
? Không có resilience strategy
```

**Scenario v?n ??:**
```
Course Service c?n l?y User info t? IAM:
  CourseService.CreateCourse(userId) 
    ? g?i IAM.GetUser(userId)
    ? N?u IAM fail ? Course fail (cascading failure)
    ? Không có retry/circuit breaker
```

**Recommendation:**
```csharp
// Program.cs (Course Service)
builder.Services.AddHttpClient<IIamServiceClient, IamServiceClient>()
    .ConfigureHttpClient(client => 
    {
        client.BaseAddress = new Uri(configuration["Services:Iam:Url"]);
        client.Timeout = TimeSpan.FromSeconds(5);
    })
    .AddPolicyHandler(GetRetryPolicy())           // ? Retry
    .AddPolicyHandler(GetCircuitBreakerPolicy()); // ? Circuit breaker

// Retry Policy: 3 l?n, exponential backoff
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3, 
            sleepDurationProvider: retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );
}

// Circuit Breaker: M? sau 5 failures, khóa 30s
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync<HttpResponseMessage>(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)
        );
}
```

---

### 5?? **DATA ISOLATION & CONSISTENCY**

#### ? **YÊU C?U:** M?i service có database riêng
**Hi?n tr?ng:**
```
? IAM ? SRSS.IAM database
? M?i service t? qu?n lý data
```

#### ? **V?N ??:** Synchronous Database Transactions

**Scenario:**
```
User Register Flow:
1. User registers ? IAM Service
2. Course Service needs to initialize user data
3. Cách hi?n t?i: G?i HTTP call (sync)
4. N?u HTTP fail ? Partial data (inconsistent)
5. Không có distributed transaction
```

**V?n ??:**
- ? Không có event-driven architecture
- ? Không có message queue (RabbitMQ, Kafka)
- ? Không có Saga pattern (distributed transaction)
- ? Data inconsistency

**Recommendation - Event-Driven Approach:**
```csharp
// Event class
public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
}

// IAM Service - Publish event
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
    // ... existing code ...
    
    // Publish event
    await _eventBus.PublishAsync(new UserRegisteredEvent 
    { 
        UserId = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        Role = user.Role.ToString()
    });
    
    return CreateLoginResponse(user, accessToken);
}

// Course Service - Subscribe
services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredEventConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost");
        cfg.ConfigureEndpoints(context);
    });
});

public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var evt = context.Message;
        // Initialize user in Course Service
        await _courseService.InitializeUserAsync(evt.UserId);
    }
}
```

---

### 6?? **LOGGING & OBSERVABILITY**

#### ?? **V?N ??:** Thi?u centralized logging

**Hi?n tr?ng:**
```csharp
// SRSS.IAM.API\Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();  // ? Console logging
builder.Logging.AddDebug();    // ? Debug logging
```

**V?n ??:**
- ?? Ch? log ra console/debug
- ? Không có centralized logging (ELK, Datadog)
- ? Không có correlation ID (tracing cross-services)
- ? Không có structured logging (JSON format)

**Example:**
```
Request A vào Gateway ? Route t?i IAM Service ? Route t?i Course Service
Làm sao trace request A across 3 services? ? C?n Correlation ID

Mà hi?n t?i không có ? Debugging nightmare!
```

**Recommendation:**
```csharp
// Add Serilog with Correlation ID
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddSerilog(new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.Seq("http://localhost:5341")  // ? Centralized logging
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "IAM")
        .Enrich.WithProperty("Version", "1.0")
        .CreateLogger());
});

// Middleware: Add Correlation ID
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers
        .FirstOrDefault(h => h.Key == "X-Correlation-ID").Value
        .FirstOrDefault() ?? Guid.NewGuid().ToString();
    
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers.Add("X-Correlation-ID", correlationId);
    
    // Propagate to downstream services
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});
```

---

### 7?? **HEALTH CHECKS & READINESS**

#### ? **V?N ??:** Không có health check endpoints

**Hi?n tr?ng:**
```
? Không có /health endpoint
? Không có liveness probe
? Không có readiness probe
? Không thích h?p cho Kubernetes
```

**V?n ??:**
- ? Kubernetes không bi?t service alive hay không
- ? Load balancer không bi?t instance nào healthy
- ? Downtime không ???c detect

**Recommendation:**
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    // Database health check
    .AddCheck("Database", () =>
    {
        using var conn = new SqlConnection(connectionString);
        try
        {
            conn.Open();
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Database connection failed");
        }
    }, tags: new[] { "ready" })
    // Redis health check
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis"), 
        tags: new[] { "ready" }
    );

var app = builder.Build();

// Liveness probe: Service is running
app.MapHealthChecks("/health/live");

// Readiness probe: Service is ready to serve
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});
```

**Dockerfile update:**
```dockerfile
# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1
```

---

### 8?? **CONTAINERIZATION & ORCHESTRATION**

#### ? **YÊU C?U:** Docker support
**Hi?n tr?ng:**
```
? Có Dockerfile cho t?ng service
? Multi-stage build
? Proper base image (.NET 8)
```

#### ? **V?N ??:** Thi?u Docker Compose & Kubernetes

**Hi?n tr?ng:**
```
? docker-compose.yml tr?ng (empty)
? Không có Kubernetes manifests
? Không có environment-specific configs
```

**docker-compose.yml (Recommendation):**
```yaml
version: '3.8'

services:
  # Database
  mssql:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: "12345"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "12345", "-Q", "SELECT 1"]
      interval: 10s
      timeout: 3s
      retries: 10

  # Redis
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 3s
      retries: 5

  # IAM Service
  iam-service:
    build:
      context: .
      dockerfile: Services/SRSS.IAM/SRSS.IAM.API/Dockerfile
    ports:
      - "8081:8080"
    environment:
      ConnectionStrings__SRSS_IAM_DB: "Server=mssql;Database=SRSS.IAM;User Id=sa;Password=12345"
      ConnectionStrings__Redis: "redis:6379"
      JwtSettings__validIssuer: "SRSSAPI"
      JwtSettings__validAudience: "SRSSClient"
      JwtSettings__secretKey: "dev-key-only-for-local"
    depends_on:
      mssql:
        condition: service_healthy
      redis:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
      interval: 30s
      timeout: 10s
      retries: 3

  # API Gateway
  yarp-gateway:
    build:
      context: .
      dockerfile: ApiGateways/YarpApiGateway/Dockerfile
    ports:
      - "5103:8080"
    environment:
      ReverseProxy__Clusters__iam-cluster__Destinations__dest__Address: "http://iam-service:8080"
    depends_on:
      iam-service:
        condition: service_healthy

volumes:
  mssql_data:
```

**Kubernetes Manifest (iam-service-deployment.yaml):**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: iam-config
namespace: default
data:
  ConnectionStrings__SRSS_IAM_DB: "Server=mssql-service;Database=SRSS.IAM;User Id=sa;Password=prod-pwd"
  ConnectionStrings__Redis: "redis-service:6379"
  JwtSettings__validIssuer: "SRSSAPI"
  JwtSettings__validAudience: "SRSSClient"

---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: iam-service
  namespace: default
spec:
  replicas: 3  # ? Scale to 3 instances
  selector:
    matchLabels:
      app: iam-service
  template:
    metadata:
      labels:
        app: iam-service
    spec:
      containers:
      - name: iam-service
        image: iam-service:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 8080
        envFrom:
        - configMapRef:
            name: iam-config
        livenessProbe:  # ? Service is alive
          httpGet:
            path: /health/live
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:  # ? Service ready to serve
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 20
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 2
        resources:  # ? Resource limits
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"

---
apiVersion: v1
kind: Service
metadata:
  name: iam-service
  namespace: default
spec:
  selector:
    app: iam-service
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: ClusterIP  # ? Internal service discovery
```

---

### 9?? **CONFIGURATION MANAGEMENT**

#### ?? **V?N ??:** Hard-coded Secrets

**Hi?n tr?ng:**
```json
{
  "JwtSettings": {
    "secretKey": "DhftOS5uphK3vmCJQrexST1RsyjZBjXWRgJMFPU4"  // ? Hard-coded!
  },
  "ConnectionStrings": {
    "SRSS_IAM_DB": "Server=.;uid=sa;pwd=12345"  // ? Hard-coded!
  }
}
```

**V?n ??:**
- ?? Secrets trong source code
- ?? Credentials visible ? Security breach
- ? Khó qu?n lý across environments
- ? Không thích h?p cho production

**Recommendation:**

**Local Development:**
```json
// appsettings.Development.json
{
  "JwtSettings": {
    "secretKey": "dev-key-only-for-local-testing"
  },
  "ConnectionStrings": {
    "SRSS_IAM_DB": "Server=localhost;Database=SRSS.IAM;User Id=sa;Password=dev-password"
  }
}
```

**Program.cs:**
```csharp
var environment = builder.Environment.EnvironmentName;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()  // ? Support environment variables
    .AddUserSecrets<Program>(optional: true);  // ? Local secrets

// Production: Load from Azure Key Vault / AWS Secrets Manager
if (app.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    if (!string.IsNullOrEmpty(keyVaultUrl))
    {
        var credential = new DefaultAzureCredential();
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUrl),
            credential
        );
    }
}
```

**Environment Variables (for Docker/K8s):**
```bash
# docker-compose.yml or Kubernetes
environment:
  JwtSettings__secretKey: "production-secret-from-vault"
  ConnectionStrings__SRSS_IAM_DB: "Server=mssql;Database=SRSS.IAM;..."
```

---

### ?? **API VERSIONING**

#### ? **V?N ??:** Không có API versioning strategy

**Hi?n tr?ng:**
```
? Không có /v1/, /v2/ prefixes
? Không có versioning header
? Không có deprecation policy
? Breaking changes s? là v?n ??
```

**Recommendation:**
```csharp
// Install: Asp.Versioning.Mvc.ApiExplorer
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),      // /api/v1/
        new HeaderApiVersionReader("X-API-Version")  // Header
    );
});

// Controller
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Version 1 implementation
    }
}

// New version with breaking changes
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthV2Controller : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestV2 request)
    {
        // Version 2 implementation
    }
}
```

---

## ?? MICROSERVICE COMPLIANCE SCORECARD

| Tiêu chí | Status | Score | Notes |
|----------|--------|-------|-------|
| Service Independence | ?? Partial | 70% | Database riêng ?, Secret key chia s? ? |
| API Gateway | ? Good | 75% | YARP setup ?, Hardcoded URLs ?, No LB ? |
| Authentication | ? Good | 80% | JWT ?, Secret shared ?, No service key ? |
| Service Communication | ? Missing | 0% | No HttpClient, No Polly, No circuit breaker |
| Data Isolation | ? Good | 85% | Separate DB ?, No event-driven ? |
| Logging | ?? Partial | 40% | Console ?, No centralized ?, No correlation ID ? |
| Health Checks | ? Missing | 0% | No endpoints, Not K8s ready |
| Containerization | ? Good | 90% | Docker ?, No compose ? |
| Config Management | ? Poor | 10% | Hard-coded secrets ?, No secret mgmt |
| API Versioning | ? Missing | 0% | No versioning strategy |
| **TOTAL** | **?? PARTIAL** | **55%** | **C?n improvements** |

---

## ?? CRITICAL ISSUES TO FIX

### ?? **P1 - SECURITY (FIX IMMEDIATELY)**

1. **Hard-coded Secrets in appsettings.json**
   - Location: `appsettings.json` - JWT secret, DB password
   - Impact: Data breach risk
   - Timeline: **This week**

2. **JWT Secret Sharing Between Services**
   - Location: Gateway + IAM use same secret
   - Impact: Service isolation broken
   - Solution: Per-service secret keys

3. **No Input Validation**
   - Missing email validation, password strength
   - Timeline: **This week**

---

### ?? **P2 - HIGH PRIORITY**

4. **No Service-to-Service Communication Pattern**
   - When Course needs to call IAM ? ???
   - Solution: HttpClient + Polly (Circuit Breaker + Retry)
   - Timeline: **Next 2 weeks**

5. **Hard-coded Service URLs** 
   - `http://localhost:8081` not scalable
   - Solution: Use DNS names + Service Discovery
   - Timeline: **Next 2 weeks**

6. **No Correlation ID**
   - Can't trace requests across services
   - Timeline: **Next week**

7. **No Health Checks**
   - K8s cannot manage services
   - Timeline: **Next week**

8. **No Event-Driven Architecture**
   - Data inconsistency in distributed transactions
   - Solution: MassTransit/NServiceBus
   - Timeline: **Next 2-3 weeks**

---

### ?? **P3 - MEDIUM PRIORITY**

9. **No Centralized Logging**
   - Solution: Serilog + Seq/ELK
   - Timeline: **Week 4**

10. **No Load Balancing Config**
    - Timeline: **Week 2**

11. **No API Versioning**
    - Timeline: **Week 3**

12. **No docker-compose**
    - Khó setup local dev
    - Timeline: **Week 2**

13. **No Kubernetes manifests**
    - Not production-ready
    - Timeline: **Week 4**

---

## ?? RECOMMENDED MIGRATION PLAN

### **PHASE 1: Security & Basics (Week 1-2)**
- [ ] Move secrets to environment variables / Key Vault
- [ ] Add input validation (FluentValidation)
- [ ] Add HTTPS enforcement
- [ ] Add correlation ID middleware
- [ ] Add health check endpoints
- [ ] Create docker-compose.yml

### **PHASE 2: Service Communication (Week 3-4)**
- [ ] Implement HttpClientFactory with Polly
- [ ] Add circuit breaker pattern
- [ ] Add retry policies
- [ ] Setup basic event bus structure

### **PHASE 3: Observability (Week 5-6)**
- [ ] Add Serilog with centralized logging
- [ ] Setup correlation ID propagation
- [ ] Add distributed tracing
- [ ] Setup monitoring dashboards

### **PHASE 4: Advanced Features (Week 7-8)**
- [ ] Implement API versioning
- [ ] Setup Kubernetes manifests
- [ ] Configure service discovery
- [ ] Setup CI/CD pipeline

### **PHASE 5: Production Ready (Week 9-10)**
- [ ] Implement event-driven architecture
- [ ] Setup message queue (RabbitMQ/Kafka)
- [ ] Load testing
- [ ] Security audit
- [ ] Production deployment

---

## ? WHAT'S WORKING WELL

1. ? **Database Separation** - Each service has own DB
2. ? **API Gateway Pattern** - YARP is good choice
3. ? **JWT Authentication** - Proper implementation
4. ? **Docker Support** - Multi-stage builds
5. ? **Dependency Injection** - Clean DI setup
6. ? **Exception Handling** - Global exception middleware
7. ? **JWT Blacklisting** - Redis implementation good
8. ? **Entity Framework** - Proper ORM usage
9. ? **Unit of Work Pattern** - Good repository pattern
10. ? **Configuration** - Structured config approach

---

## ?? REFERENCE LINKS

**Service Communication & Resilience:**
- Polly: https://github.com/App-vNext/Polly
- MassTransit: https://masstransit.io/
- NServiceBus: https://particular.net/nservicebus

**Logging & Monitoring:**
- Serilog: https://serilog.net/
- Seq: https://datalust.co/seq
- Application Insights: https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview

**Secrets Management:**
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault

**Orchestration:**
- Docker Compose: https://docs.docker.com/compose/
- Kubernetes: https://kubernetes.io/

**Distributed Tracing:**
- Jaeger: https://www.jaegertracing.io/
- Zipkin: https://zipkin.io/
- Application Insights

