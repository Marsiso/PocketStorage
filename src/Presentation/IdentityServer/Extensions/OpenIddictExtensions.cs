﻿using Domain.Data;
using OpenIddict.Abstractions;

namespace IdentityServer.Extensions;

public static class OpenIddictExtensions
{
    #region Public Methods

    public static IServiceCollection AddApplicationOpenIddict(this IServiceCollection services)
    {
        services
            .AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
            .AddServer(options => options.Configure())
            // Register the OpenIddict validation components
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance
                options.UseLocalServer();

                // Register the ASP.NET Core host
                options.UseAspNetCore();
            });

        return services;
    }

    public static OpenIddictServerBuilder Configure(this OpenIddictServerBuilder builder)
    {
        // Enable the authorization, logout, token and user info endpoints
        builder.SetAuthorizationEndpointUris("/connect/authorize");
        builder.SetLogoutEndpointUris("/connect/logout");
        builder.SetTokenEndpointUris("/connect/token");
        builder.SetUserinfoEndpointUris("/connect/userInfo");
        builder.SetIntrospectionEndpointUris("/connect/introspect");
        builder.SetVerificationEndpointUris("/connect/verify");

        // Mark the "email", "profile" and "roles" scopes as supported scopes
        builder.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles);

        // Enable the client credentials flow
        builder.AllowClientCredentialsFlow();
        builder.AllowAuthorizationCodeFlow();
        builder.AllowRefreshTokenFlow();
        builder.RequireProofKeyForCodeExchange();

        // Register the signing and encryption credentials
        builder.AddDevelopmentEncryptionCertificate();
        builder.AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core options
        builder.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough()
               .EnableStatusCodePagesIntegration();

        return builder;
    }

    #endregion Public Methods
}