using System.Net.Mime;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.ResourceServer.Controllers.Base;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PocketStorage.ResourceServer.Controllers;

public class UserController : ApiControllerBase<UserController>
{
    public UserController(ILogger<UserController> logger) : base(logger)
    {
    }

    [HttpGet("~/api/user")]
    [AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Ok(UserInfo.Anonymous);
        }

        UserInfo userInfo = new() { IsAuthenticated = true };

        if (User?.Identity is ClaimsIdentity claimsIdentity)
        {
            userInfo.NameClaimType = claimsIdentity.NameClaimType;
            userInfo.RoleClaimType = claimsIdentity.RoleClaimType;
        }
        else
        {
            userInfo.NameClaimType = ClaimTypes.Name;
            userInfo.RoleClaimType = ClaimTypes.Role;
        }

        if (User?.Claims?.Any() ?? false)
        {
            IList<ClaimValue> claims = new List<ClaimValue>();
            foreach (Claim claim in User.Claims)
            {
                if (claim.Type.Equals(userInfo.NameClaimType, StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new ClaimValue(userInfo.NameClaimType, claim.Value));
                }
                else if (claim.Type.Equals(userInfo.RoleClaimType, StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new ClaimValue(userInfo.RoleClaimType, claim.Value));
                }
                else if (claim.Type.Equals(Claims.GivenName, StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new ClaimValue(Claims.GivenName, claim.Value));
                }
                else if (claim.Type.Equals(Claims.FamilyName, StringComparison.OrdinalIgnoreCase))
                {
                    claims.Add(new ClaimValue(Claims.FamilyName, claim.Value));
                }
            }

            userInfo.Claims = claims;
        }

        return Ok(userInfo);
    }
}
