using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Helpers;

namespace PocketStorage.BFF.Authorization.Extensions;

public static class IAuthorizationServiceExtensions
{
    public static Task<AuthorizationResult> AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission permission) => service.AuthorizeAsync(user, user, PolicyNameHelpers.GetPolicyNameFor(permission));
}
