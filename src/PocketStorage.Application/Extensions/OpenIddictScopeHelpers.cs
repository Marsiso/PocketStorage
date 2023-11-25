using CommunityToolkit.Diagnostics;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.Domain.Constants;
using static OpenIddict.Abstractions.OpenIddictConstants.Claims;

namespace PocketStorage.Application.Extensions;

public static class OpenIddictScopeHelpers
{
    public static Dictionary<string, string[]> GetIncludedClaimNamesFrom(string scopes, string delimiter)
    {
        Guard.IsNotNull(scopes);
        Guard.IsNotNull(delimiter);
        Dictionary<string, string[]> scopesWithClaims = new();

        foreach (string scope in scopes.Split(delimiter))
        {
            scopesWithClaims[scope] = GetIncludedClaimNamesFrom(scope);
        }

        return scopesWithClaims;
    }

    public static string[] GetIncludedClaimNamesFrom(string scope)
    {
        Guard.IsNotNull(scope);
        return scope switch
        {
            OpenIddictScopeDefaults.Name => new[] { Name, Username, GivenName, MiddleName, FamilyName },
            OpenIddictScopeDefaults.Email => new[] { Email, EmailVerified },
            OpenIddictScopeDefaults.PhoneNumber => new[] { PhoneNumber, PhoneNumberVerified },
            OpenIddictScopeDefaults.Locale => new[] { Locale },
            OpenIddictScopeDefaults.Zoneinfo => new[] { Zoneinfo },
            OpenIddictScopeDefaults.UpdatedAt => new[] { UpdatedAt },
            PermitConstants.Scopes.Permissions => new[] { PermitConstants.Claims.Permissions },
            _ => Array.Empty<string>()
        };
    }
}
