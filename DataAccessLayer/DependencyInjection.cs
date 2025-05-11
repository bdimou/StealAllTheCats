using DataAccessLayer.Context;
using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Instantiate SQL Server connection string template  
        string SqlConnectionString = configuration.GetConnectionString("SqlConnectionString")!;

        /*
        // Replace the placeholders in the connection string with environment variables  
        SqlConnectionString = SqlConnectionString
                .Replace("{Host}", Environment.GetEnvironmentVariable("SQLSERVER_HOST"))
                .Replace("{Port}", Environment.GetEnvironmentVariable("SQLSERVER_PORT"))
                .Replace("{Database}", Environment.GetEnvironmentVariable("SQLSERVER_DB"))
                .Replace("{Username}", Environment.GetEnvironmentVariable("SQLSERVER_USER"))
                .Replace("{Password}", Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD"));
        */
        // Add DbContext to the IoC container  
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(SqlConnectionString);
        });

        // Add the repositories to the IoC container  
        services.AddScoped<ICatsRepository, CatsRepository>();

        // Add the unit of work to the IoC container  
        return services;
    }
}
            