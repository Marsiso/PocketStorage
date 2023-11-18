using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using PocketStorage.Domain.Constants;
using PocketStorage.Domain.Options;

namespace PocketStorage.Application.Extensions;

public static class OpenIdConnectOptionsExtensions
{
    public static OpenIdConnectOptions Configure(this OpenIdConnectOptions options, ApplicationSettings settings)
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = settings.OpenIdConnect.Server.Authority;
        options.ClientId = settings.OpenIdConnect.Clients.Single(client => client.Id == "pocket_storage_resource_server").Id;
        options.ClientSecret = settings.OpenIdConnect.Clients.Single(client => client.Id == "pocket_storage_resource_server").Secret;
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

        return options;
    }
}
