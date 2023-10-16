using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace PocketStorage.ResourceServer.Extensions;

public static class MvcExtensions
{
    public static void Configure(this MvcOptions options)
    {
        AuthorizationPolicyBuilder policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();

        AuthorizationPolicy policy = policyBuilder.Build();

        options.Filters.Add(new AuthorizeFilter(policy));
    }
}
