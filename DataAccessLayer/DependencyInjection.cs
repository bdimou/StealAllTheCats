using DataAccessLayer.Context;
using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using dotenv.net;

namespace DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Move up one directory from /API to solution root and load .env
        var solutionPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
        var envPath = Path.Combine(solutionPath, ".env");

        try 
        {
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { envPath }));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning loading .env file: {ex.Message}");
        }
        
        var connectionString = configuration.GetConnectionString("SqlConnectionString");
        
        // Get environment variables with fallbacks
        var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "1433";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "CatsDb";
        var user = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "YourStrong@Passw0rd";

        if (!string.IsNullOrEmpty(connectionString))
        {
            // Replace template variables in the existing connection string
            connectionString = connectionString
                .Replace("${DB_HOST}", host)
                .Replace("${DB_PORT}", port)
                .Replace("${DB_NAME}", database)
                .Replace("${DB_USER}", user)
                .Replace("${DB_PASSWORD}", password);
        }
        else
        {
            // Build connection string from scratch
            connectionString = $"Server={host},{port};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";
        }

        // Add DbContext to the IoC container
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Add the repositories to the IoC container
        services.AddScoped<ICatsRepository, CatsRepository>();

        return services;
    }
}
            