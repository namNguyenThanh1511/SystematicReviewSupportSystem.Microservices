# Quick Reference: Microservice Solutions

## Solution Files Location

```
src/
??? ApiGateways/YarpGateway.sln       ? API Gateway
??? Services/
?   ??? SRSS.IAM/IAM.sln              ? IAM Service  
?   ??? SRSS.Project/Project.sln      ? Project Service
??? SRSS.Microservices.sln            (legacy - all projects)
```

## Quick Commands

### Build All Services
```bash
# API Gateway
cd src/ApiGateways && dotnet build YarpGateway.sln

# IAM Service
cd src/Services/SRSS.IAM && dotnet build IAM.sln

# Project Service
cd src/Services/SRSS.Project && dotnet build Project.sln
```

### Run Services
```bash
# API Gateway
cd src/ApiGateways/YarpApiGateway && dotnet run

# IAM Service
cd src/Services/SRSS.IAM/SRSS.IAM.API && dotnet run

# Project Service
cd src/Services/SRSS.Project/SRSS.Project.API && dotnet run
```

## Architecture Compliance ?

? Each service = 1 solution
? Gateway has its own solution
? Services are independent
? Shared library referenced by all

For detailed documentation, see: [MICROSERVICE_STRUCTURE.md](MICROSERVICE_STRUCTURE.md)
