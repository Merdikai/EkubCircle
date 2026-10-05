using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EkubCircle.Application.Common.Interfaces;
using EkubCircle.Infrastructure.Identity;
using EkubCircle.Infrastructure.Persistence.Context;

namespace EkubCircle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var postgresConn = configuration.GetConnectionString("PostgresConnection");
        var defaultConn = configuration.GetConnectionString("DefaultConnection");

        string connectionString;
        if (!string.IsNullOrWhiteSpace(postgresConn) && !postgresConn.Contains("your_password_here", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = postgresConn;
        }
        else if (!string.IsNullOrWhiteSpace(defaultConn))
        {
            connectionString = defaultConn;
        }
        else
        {
            connectionString = "Data Source=ekubcircle.db";
        }

        services.AddDbContext<EkubDbContext>(options =>
        {
            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Port=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IEkubDbContext>(provider => provider.GetRequiredService<EkubDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
