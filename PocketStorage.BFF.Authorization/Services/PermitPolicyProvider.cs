using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PocketStorage.BFF.Authorization.Enums;
using PocketStorage.BFF.Authorization.Helpers;
using PocketStorage.BFF.Authorization.Models;
using static System.String;

namespace PocketStorage.BFF.Authorization.Services;

public class PermitPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    private readonly AuthorizationOptions _options = options.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);
        if (IsNullOrWhiteSpace(policyName) || !PolicyNameHelpers.IsValidPolicyName(policyName))
        {
            return policy;
        }

        Permission permissions = PolicyNameHelpers.GetPermissionsFrom(policyName);

        policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermitRequirement(permissions))
            .Build();

        _options.AddPolicy(policyName, policy);
        return policy;
    }
}
