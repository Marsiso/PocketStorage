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

    [HttpGet("~/api/userinfo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Ok(UserInfo.Anonymous);
        }

        UserInfo userinfo = new() { IsAuthenticated = true };
        if (User.Identity is ClaimsIdentity identity)
        {
            userinfo.NameClaimType = identity.NameClaimType;
            userinfo.RoleClaimType = identity.RoleClaimType;
        }
        else
        {
            userinfo.NameClaimType = ClaimTypes.Name;
            userinfo.RoleClaimType = ClaimTypes.Role;
        }

        if (!User.Claims.Any())
        {
            return Ok(userinfo);
        }

        userinfo.Claims = new List<ClaimValue>();
        foreach (Claim claim in User.Claims)
        {
            if (claim.Type == Claims.UpdatedAt)
            {
                continue;
            }

            userinfo.Claims.Add(new ClaimValue(claim.Type, claim.Value));
        }

        return Ok(userinfo);
    }
}
