using Microsoft.AspNetCore.Authorization;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Helpers;

namespace PocketStorage.BFF.Authorization.Filters;

public class PermitAttribute : AuthorizeAttribute
{
    public PermitAttribute()
    {
    }

    public PermitAttribute(string policy) : base(policy)
    {
    }

    public PermitAttribute(Permission permissions) => Permissions = permissions;

    public Permission Permissions
    {
        get => !string.IsNullOrWhiteSpace(Policy) ? PolicyNameHelpers.GetPermissionsFrom(Policy) : Permission.None;
        set => Policy = value != Permission.None ? PolicyNameHelpers.GetPolicyNameFor(value) : string.Empty;
    }
}
