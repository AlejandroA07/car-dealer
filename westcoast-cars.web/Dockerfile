# ====================================================================================
# STAGE 1: Build Environment
# - Uses the .NET SDK image which contains all tools needed to build the application.
# - Restores NuGet packages and publishes the final application artifacts.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project and solution files first to leverage Docker layer caching.
# Docker will only re-run 'dotnet restore' if these files change.
# Note: We use the API's solution file as it contains all project references.
COPY westcoast-cars.api/westcoast-cars.api.sln westcoast-cars.api/
COPY westcoast-cars.api/westcoast-cars.api.csproj westcoast-cars.api/
COPY westcoast-cars.web/westcoast-cars.web.csproj westcoast-cars.web/
COPY WestcoastCars.Application/WestcoastCars.Application.csproj WestcoastCars.Application/
COPY WestcoastCars.Contracts/WestcoastCars.Contracts.csproj WestcoastCars.Contracts/
COPY WestcoastCars.Domain/WestcoastCars.Domain.csproj WestcoastCars.Domain/
COPY WestcoastCars.Infrastructure/WestcoastCars.Infrastructure.csproj WestcoastCars.Infrastructure/

# Restore dependencies for the entire solution
RUN dotnet restore "westcoast-cars.api/westcoast-cars.api.sln"

# Copy the rest of the source code into the container
COPY . .

# Set the working directory to the Web project
WORKDIR "/src/westcoast-cars.web"
# Publish the application, creating the compiled output. --no-restore is used because we already restored.
RUN dotnet publish "westcoast-cars.web.csproj" -c Release -o /app/publish --no-restore

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
ENTRYPOINT ["dotnet", "westcoast-cars.web.dll"]
