using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Helpers;

namespace PocketStorage.BFF.Authorization.Components;

public class PermitView : AuthorizeView
{
    [Parameter]
    public Permission Permissions
    {
        get => string.IsNullOrWhiteSpace(Policy) ? Permission.None : PolicyNameHelpers.GetPermissionsFrom(Policy);
        set => Policy = PolicyNameHelpers.GetPolicyNameFor(value);
    }
}
