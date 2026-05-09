# ====================================================================================
# STAGE 1: Build Environment
# - Uses the .NET SDK image which contains all tools needed to build the application.
# - Restores NuGet packages and publishes the final application artifacts.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

# Copy the solution file and all project files
COPY ["westcoast-cars.sln", "."]
COPY ["global.json", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

# Web project
COPY ["WestcoastCars.Web/WestcoastCars.Web.csproj", "WestcoastCars.Web/"]

# Shared contract project
COPY ["WestcoastCars.Contracts/WestcoastCars.Contracts.csproj", "WestcoastCars.Contracts/"]

# Restore only what this service needs (avoids requiring test projects during container builds)
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "WestcoastCars.Web/WestcoastCars.Web.csproj"

# Copy the rest of the source code into the container
COPY . .

# Set the working directory to the Web project
WORKDIR "/src/WestcoastCars.Web"
# Publish the application, creating the compiled output.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish "WestcoastCars.Web.csproj" -c Release -o /app/publish --no-restore

# ====================================================================================
# STAGE 2: Final Runtime Image
# - Uses the lightweight ASP.NET runtime image, which is optimized for running apps.
# - This image is smaller and more secure as it doesn't contain the build SDK.
# ====================================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Copy the published build output from the 'build' stage
COPY --from=build /app/publish .

# Create and set permissions for the data protection keys folder
RUN mkdir /app/keys
RUN chown app /app/keys

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "WestcoastCars.Web.dll"]
