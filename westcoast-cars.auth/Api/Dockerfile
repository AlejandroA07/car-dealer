FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

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

# Copy the rest of the source code
COPY . .

# Set the working directory to the Api project and publish it
WORKDIR "/src/westcoast-cars.auth/Api"
RUN dotnet publish "WestcoastCars.Auth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WestcoastCars.Auth.Api.dll"]
