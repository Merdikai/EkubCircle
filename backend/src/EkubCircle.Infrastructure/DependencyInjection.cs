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
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ekubcircle.db";
        services.AddDbContext<EkubDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IEkubDbContext>(provider => provider.GetRequiredService<EkubDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
