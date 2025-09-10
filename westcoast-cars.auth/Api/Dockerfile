FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the project files first, preserving the directory structure
COPY ["westcoast-cars.auth/WestcoastCars.Auth.sln", "westcoast-cars.auth/"]
COPY ["westcoast-cars.auth/Api/WestcoastCars.Auth.Api.csproj", "westcoast-cars.auth/Api/"]
COPY ["westcoast-cars.auth/Application/WestcoastCars.Auth.Application.csproj", "westcoast-cars.auth/Application/"]
COPY ["westcoast-cars.auth/Domain/WestcoastCars.Auth.Domain.csproj", "westcoast-cars.auth/Domain/"]
COPY ["westcoast-cars.auth/Infrastructure/WestcoastCars.Auth.Infrastructure.csproj", "westcoast-cars.auth/Infrastructure/"]
COPY ["westcoast-cars.auth/Contracts/WestcoastCars.Auth.Contracts.csproj", "westcoast-cars.auth/Contracts/"]

# Restore dependencies for the entire solution
RUN dotnet restore "westcoast-cars.auth/WestcoastCars.Auth.sln"

# Copy the rest of the source code for the auth microservice
COPY ["westcoast-cars.auth/", "westcoast-cars.auth/"]

# Set the working directory to the Api project and publish it
WORKDIR "/src/westcoast-cars.auth/Api"
RUN dotnet publish "WestcoastCars.Auth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WestcoastCars.Auth.Api.dll"]