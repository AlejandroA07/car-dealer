using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;
using System.Linq;

namespace WestcoastCars.Auth.Infrastructure.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        // Traverse up to find the repository root (where the .git directory is)
        while (directory != null && !directory.GetDirectories(".git").Any())
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new InvalidOperationException("Could not find the repository root directory. The .git directory was not found.");
        }

        var envFilePath = Path.Combine(directory.FullName, ".env");

        if (!File.Exists(envFilePath))
        {
            throw new InvalidOperationException($".env file not found at {envFilePath}");
        }

        var passwordLine = File.ReadLines(envFilePath).FirstOrDefault(line => line.Trim().StartsWith("MYSQL_PASSWORD="));
        if (string.IsNullOrEmpty(passwordLine))
        {
            throw new InvalidOperationException("MYSQL_PASSWORD not found in .env file.");
        }

        var password = passwordLine.Split('=').Last().Trim();

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        var connectionString = $"Server=localhost;Port=3307;Database=westcoast_auth;Uid=root;Pwd={password}";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new AuthDbContext(optionsBuilder.Options);
    }
}