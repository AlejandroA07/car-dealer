# ====================================================================================
# STAGE 1: Build Environment
# - Uses the .NET SDK image which contains all tools needed to build the application.
# - Restores NuGet packages and publishes the final application artifacts.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copy the solution file and all project files
COPY ["westcoast-cars.sln", "."]

# API projects
COPY ["WestcoastCars.Api/WestcoastCars.Api.csproj", "WestcoastCars.Api/"]
COPY ["WestcoastCars.Api.Tests/WestcoastCars.Api.Tests.csproj", "WestcoastCars.Api.Tests/"]

# Web project
COPY ["WestcoastCars.Web/WestcoastCars.Web.csproj", "WestcoastCars.Web/"]

# Core projects
COPY ["WestcoastCars.Application/WestcoastCars.Application.csproj", "WestcoastCars.Application/"]
COPY ["WestcoastCars.Application.Tests/WestcoastCars.Application.Tests.csproj", "WestcoastCars.Application.Tests/"]
COPY ["WestcoastCars.Contracts/WestcoastCars.Contracts.csproj", "WestcoastCars.Contracts/"]
COPY ["WestcoastCars.Domain/WestcoastCars.Domain.csproj", "WestcoastCars.Domain/"]
COPY ["WestcoastCars.Infrastructure/WestcoastCars.Infrastructure.csproj", "WestcoastCars.Infrastructure/"]

# Restore only what this service needs (avoids requiring test projects during container builds)
RUN dotnet restore "WestcoastCars.Web/WestcoastCars.Web.csproj"

# Copy the rest of the source code into the container
COPY . .

# Set the working directory to the Web project
WORKDIR "/src/WestcoastCars.Web"
# Publish the application, creating the compiled output.
RUN dotnet publish "WestcoastCars.Web.csproj" -c Release -o /app/publish

# ====================================================================================
# STAGE 2: Final Runtime Image
# - Uses the lightweight ASP.NET runtime image, which is optimized for running apps.
# - This image is smaller and more secure as it doesn't contain the build SDK.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble
WORKDIR /app

# Copy the published build output from the 'build' stage
COPY --from=build /app/publish .

# Create and set permissions for the data protection keys folder
RUN mkdir /app/keys
RUN chown app /app/keys

# Set the entrypoint for the container to run the application
ENTRYPOINT ["dotnet", "WestcoastCars.Web.dll"]
