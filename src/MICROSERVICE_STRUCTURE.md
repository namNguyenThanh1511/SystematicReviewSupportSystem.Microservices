# Microservice Architecture Structure

This repository follows a microservice architecture pattern where each service is isolated with its own solution file.

## Structure Overview

```
/
??? ApiGateways/
?   ??? YarpGateway.sln          # API Gateway solution
?   ??? YarpApiGateway/          # YARP API Gateway project
?
??? Services/
?   ??? SRSS.IAM/
?   ?   ??? IAM.sln              # IAM Service solution
?   ?   ??? SRSS.IAM.API/
?   ?   ??? SRSS.IAM.Services/
?   ?   ??? SRSS.IAM.Repositories/
?   ?
?   ??? SRSS.Project/
?       ??? Project.sln          # Project Service solution
?       ??? SRSS.Project.API/
?       ??? SRSS.Project.Application/
?       ??? SRSS.Project.Domain/
?       ??? SRSS.Project.Infrastructure/
?
??? Shared/
    ??? Shared/                  # Shared libraries used across services
```

## Solution Files

### 1. YarpGateway.sln
Located in: `ApiGateways/YarpGateway.sln`

**Purpose**: API Gateway that routes requests to microservices

**Projects included**:
- YarpApiGateway
- Shared (referenced)

**Build command**:
```bash
cd ApiGateways
dotnet build YarpGateway.sln
```

### 2. IAM.sln
Located in: `Services/SRSS.IAM/IAM.sln`

**Purpose**: Identity and Access Management microservice

**Projects included**:
- SRSS.IAM.API
- SRSS.IAM.Services
- SRSS.IAM.Repositories
- Shared (referenced)

**Build command**:
```bash
cd Services/SRSS.IAM
dotnet build IAM.sln
```

### 3. Project.sln
Located in: `Services/SRSS.Project/Project.sln`

**Purpose**: Project management microservice

**Projects included**:
- SRSS.Project.API
- SRSS.Project.Application
- SRSS.Project.Domain
- SRSS.Project.Infrastructure
- Shared (referenced)

**Build command**:
```bash
cd Services/SRSS.Project
dotnet build Project.sln
```

## Benefits of This Structure

1. **Service Isolation**: Each microservice has its own solution file, allowing independent development and deployment
2. **Clear Boundaries**: Physical separation of services makes boundaries explicit
3. **Independent Building**: Services can be built and tested independently
4. **Easier CI/CD**: Each service can have its own CI/CD pipeline
5. **Shared Libraries**: Common functionality is shared through the Shared project

## Development Workflow

### Working on a Specific Service

1. Open the solution file for the service you want to work on:
   - `YarpGateway.sln` for API Gateway
   - `IAM.sln` for IAM service
   - `Project.sln` for Project service

2. Make your changes within that service

3. Build and test the specific service:
   ```bash
   dotnet build <solution-name>.sln
   dotnet test <solution-name>.sln
   ```

### Building All Services

To build all services at once, you can run:

```bash
# Build API Gateway
cd ApiGateways
dotnet build YarpGateway.sln

# Build IAM Service
cd ../Services/SRSS.IAM
dotnet build IAM.sln

# Build Project Service
cd ../SRSS.Project
dotnet build Project.sln
```

## Shared Project

The `Shared` project contains common functionality used across multiple services:
- JWT Authentication
- Common middlewares
- Shared utilities

Each service solution includes a reference to the Shared project, ensuring consistency across services while maintaining independence.

## Adding a New Service

To add a new microservice:

1. Create a new folder under `Services/` with your service name
2. Create a new solution file in that folder: `<ServiceName>.sln`
3. Add your service projects to the solution
4. Add a reference to the Shared project if needed
5. Update this documentation

## Docker & Deployment

Each service can be containerized and deployed independently. Make sure to:
- Create a Dockerfile for each service's API project
- Configure appropriate docker-compose files
- Set up proper service discovery and communication
