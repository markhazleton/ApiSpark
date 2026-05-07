using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ApiSpark.Api.Infrastructure.Auth;

public static class AuthorizationSetup
{
    public static IServiceCollection AddApiSparkAuth(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = 401;
                        ctx.Response.Headers.WWWAuthenticate = "Bearer";
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            });

            options.AddPolicy("Publisher", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin", "Publisher");
            });

            options.AddPolicy("ServiceOrAdmin", policy =>
            {
                policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole("Admin") ||
                    ctx.User.HasClaim("scope", "apispark.publish"));
            });
        });

        return services;
    }
}
