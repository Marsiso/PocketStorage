using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using PocketStorage.Domain.Constants;

namespace PocketStorage.ResourceServer.Extensions;

public static class OpenIdConnectExtensions
{
    public static AuthenticationBuilder AddOidc(this AuthenticationBuilder services, IConfiguration configuration) =>
        services.AddOpenIdConnect(options =>
        {
            string authority = configuration["OpenIdConnect:Authority"];
            string clientId = configuration["OpenIdConnect:ClientId"];
            string clientSecret = configuration["OpenIdConnect:ClientSecret"];

            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = authority;
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.RequireHttpsMetadata = true;
            options.ResponseType = OpenIddictConstants.ResponseTypes.Code;
            options.UsePkce = true;
            options.Scope.Add(OpenIddictConstants.Scopes.Profile);
            options.Scope.Add(OpenIddictConstants.Scopes.Roles);
            options.Scope.Add(OpenIddictScopeDefaults.Name);
            options.Scope.Add(OpenIddictScopeDefaults.Email);
            options.Scope.Add(OpenIddictScopeDefaults.PhoneNumber);
            options.Scope.Add(OpenIddictScopeDefaults.Locale);
            options.Scope.Add(OpenIddictScopeDefaults.Zoneinfo);
            options.Scope.Add(OpenIddictScopeDefaults.UpdatedAt);
            options.Scope.Add(OpenIddictConstants.Scopes.OfflineAccess);
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.RefreshInterval = TimeSpan.FromMinutes(3);
            options.AutomaticRefreshInterval = TimeSpan.FromMinutes(10);
            options.AccessDeniedPath = "/";
            options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = OpenIddictConstants.Claims.Name };
        });
}
