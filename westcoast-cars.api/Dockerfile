# ====================================================================================
# STAGE 1: Build Environment
# - Uses the .NET SDK image which contains all tools needed to build the application.
# - Restores NuGet packages and publishes the final application artifacts.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project and solution files first to leverage Docker layer caching.
# Docker will only re-run 'dotnet restore' if these files change.
COPY westcoast-cars.api/westcoast-cars.api.sln westcoast-cars.api/
COPY westcoast-cars.api/westcoast-cars.api.csproj westcoast-cars.api/
COPY westcoast-cars.web/westcoast-cars.web.csproj westcoast-cars.web/
COPY WestcoastCars.Application/WestcoastCars.Application.csproj WestcoastCars.Application/
COPY WestcoastCars.Contracts/WestcoastCars.Contracts.csproj WestcoastCars.Contracts/
COPY WestcoastCars.Domain/WestcoastCars.Domain.csproj WestcoastCars.Domain/
COPY WestcoastCars.Infrastructure/WestcoastCars.Infrastructure.csproj WestcoastCars.Infrastructure/
COPY westcoast-cars.api.tests/westcoast-cars.api.tests.csproj westcoast-cars.api.tests/

# Restore dependencies for the entire solution
RUN dotnet restore "westcoast-cars.api/westcoast-cars.api.sln"

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
