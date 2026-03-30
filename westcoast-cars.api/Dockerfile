# ====================================================================================
# STAGE 1: Build Environment
# - Uses the .NET SDK image which contains all tools needed to build the application.
# - Restores NuGet packages and publishes the final application artifacts.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file and all project files
COPY ["westcoast-cars.sln", "."]

# API projects
COPY ["westcoast-cars.api/westcoast-cars.api.csproj", "westcoast-cars.api/"]
COPY ["westcoast-cars.api.tests/westcoast-cars.api.tests.csproj", "westcoast-cars.api.tests/"]

# Auth projects
COPY ["westcoast-cars.auth/Api/WestcoastCars.Auth.Api.csproj", "westcoast-cars.auth/Api/"]
COPY ["westcoast-cars.auth/Application/WestcoastCars.Auth.Application.csproj", "westcoast-cars.auth/Application/"]
COPY ["westcoast-cars.auth/Contracts/WestcoastCars.Auth.Contracts.csproj", "westcoast-cars.auth/Contracts/"]
COPY ["westcoast-cars.auth/Domain/WestcoastCars.Auth.Domain.csproj", "westcoast-cars.auth/Domain/"]
COPY ["westcoast-cars.auth/Infrastructure/WestcoastCars.Auth.Infrastructure.csproj", "westcoast-cars.auth/Infrastructure/"]
COPY ["westcoast-cars.auth/WestcoastCars.Auth.Tests/WestcoastCars.Auth.Tests.csproj", "westcoast-cars.auth/WestcoastCars.Auth.Tests/"]

# Web project
COPY ["westcoast-cars.web/westcoast-cars.web.csproj", "westcoast-cars.web/"]

# Core projects
COPY ["WestcoastCars.Application/WestcoastCars.Application.csproj", "WestcoastCars.Application/"]
COPY ["WestcoastCars.Application.Tests/WestcoastCars.Application.Tests.csproj", "WestcoastCars.Application.Tests/"]
COPY ["WestcoastCars.Contracts/WestcoastCars.Contracts.csproj", "WestcoastCars.Contracts/"]
COPY ["WestcoastCars.Domain/WestcoastCars.Domain.csproj", "WestcoastCars.Domain/"]
COPY ["WestcoastCars.Infrastructure/WestcoastCars.Infrastructure.csproj", "WestcoastCars.Infrastructure/"]

# Restore dependencies for the entire solution
RUN dotnet restore "westcoast-cars.sln"

# Copy the rest of the source code into the container
COPY . .

# Set the working directory to the API project
WORKDIR "/src/westcoast-cars.api"
# Publish the application, creating the compiled output.
RUN dotnet publish "westcoast-cars.api.csproj" -c Release -o /app/publish

# ====================================================================================
# STAGE 2: Final Runtime Image
# - Uses the lightweight ASP.NET runtime image, which is optimized for running apps.
# - This image is smaller and more secure as it doesn't contain the build SDK.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the published build output from the 'build' stage
COPY --from=build /app/publish .

# Set the entrypoint for the container to run the application
ENTRYPOINT ["sh", "-c", "sleep 5 && dotnet westcoast-cars.api.dll"]
