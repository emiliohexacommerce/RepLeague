using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Infrastructure.Persistence;
using RepLeague.Infrastructure.Services;

namespace RepLeague.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHashingService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IWebPushService, WebPushService>();

        return services;
    }
}
