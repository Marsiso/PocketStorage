using System.Collections.Immutable;
using System.Net.Mime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using PocketStorage.Domain.Application.Models;
using PocketStorage.IdentityServer.Controllers.Common;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PocketStorage.IdentityServer.Controllers;

public class UserInfoController : WebControllerBase<UserInfoController>
{
    private readonly UserManager<User> _userManager;

    public UserInfoController(UserManager<User> userManager) => _userManager = userManager;

    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Userinfo()
    {
        User? user = await _userManager.FindByIdAsync(User.GetClaim(Claims.Subject) ?? string.Empty);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists"
                }));
        }

        Dictionary<string, object> claims = await GetClaimsAsync(user);

        // Note: the complete list of standard claims supported by the OpenID Connect specification
        // can be found here: "https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims".
        return Ok(claims);
    }

    private async Task<Dictionary<string, object>> GetClaimsAsync(User user)
    {
        Dictionary<string, object> claims = new(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [Claims.Subject] = await _userManager.GetUserIdAsync(user)
        };

        ImmutableArray<string> scopes = User.GetScopes();
        foreach (string scope in scopes)
        {
            switch (scope)
            {
                case Scopes.Email:
                    claims[Claims.Email] = await _userManager.GetEmailAsync(user) ?? string.Empty;
                    claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
                    break;

                case Scopes.Phone:
                    claims[Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user) ?? string.Empty;
                    claims[Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
                    break;

                case Scopes.Roles:
                    claims[Claims.Role] = await _userManager.GetRolesAsync(user);
                    break;

                case Scopes.Profile:
                    claims[Claims.GivenName] = user.GivenName;
                    claims[Claims.FamilyName] = user.FamilyName;
                    break;

                default:
                    continue;
            }
        }

        return claims;
    }
}
