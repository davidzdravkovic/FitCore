using System.Text;
using FitCore.Api.Data;
using FitCore.Api.Features.PlatformAuth;
using FitCore.Api.Infrastructure.App;
using FitCore.Api.Infrastructure.Auth;
using FitCore.Api.Infrastructure.Aws;
using FitCore.Api.Infrastructure.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FitCore.Api.Composition;

public static class RegisterServices
{
    public static IServiceCollection AddFitCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<AppOptions>(configuration.GetSection(AppOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddOpenApi();
        services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        services.AddFitCoreAuthentication();
        services.AddAuthorization();
        services.AddFitCoreCors(configuration);
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<PlatformAuthService>();
        services.AddSingleton<JwtTokenIssuer>();
        services.AddScoped<IEmailSender, SesEmailSender>();

        return services;
    }

    private static IServiceCollection AddFitCoreAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

        return services;
    }

    private sealed class JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
        : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            if (name is not null && name != JwtBearerDefaults.AuthenticationScheme)
                return;

            var jwt = jwtOptions.Value;
            if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
                throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            };
        }
    }

    private static IServiceCollection AddFitCoreCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DevCors", policy =>
                policy.SetIsOriginAllowed(static origin =>
                    {
                        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                            return false;
                        return uri.IsLoopback;
                    })
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            options.AddPolicy("ProdCors", policy =>
            {
                var origins = configuration.GetSection("Allowed_Cors").Get<string[]>() ?? [];
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
