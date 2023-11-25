using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Helpers;
using PocketStorage.BFF.Authorization.Models;

namespace PocketStorage.BFF.Authorization.Services;

public class PermitHandler : AuthorizationHandler<PermitRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermitRequirement requirement)
    {
        Claim? permissionClaim = context.User.FindFirst(PermitConstants.Claims.Permissions);
        if (permissionClaim == null)
        {
            return Task.CompletedTask;
        }

        Permission permissions = PolicyNameHelpers.GetPermissionsFrom(permissionClaim.Value);
        if ((permissions & requirement.Permission) != 0)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
