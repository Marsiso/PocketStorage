using System.Collections.Immutable;
using System.Globalization;
using System.Net.Mime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Constants;
using PocketStorage.IdentityServer.Controllers.Common;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PocketStorage.IdentityServer.Controllers;

public class UserInfoController(UserManager<User> userManager) : WebControllerBase<UserInfoController>
{
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Userinfo()
    {
        User? user = await userManager.FindByIdAsync(User.GetClaim(Claims.Subject) ?? string.Empty);
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists"
                }));
        }

        // Note: the complete list of standard claims supported by the OpenID Connect specification
        // can be found here: "https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims".
        return Ok(await GetClaimsAsync(user));
    }

    private async Task<Dictionary<string, object>> GetClaimsAsync(User user)
    {
        Dictionary<string, object> claims = new(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [Claims.Subject] = await userManager.GetUserIdAsync(user)
        };

        ImmutableArray<string> scopes = User.GetScopes();
        foreach (string scope in scopes)
        {
            switch (scope)
            {
                case OpenIddictScopeDefaults.Name:
                    claims[Claims.Name] = await userManager.GetUserNameAsync(user);
                    claims[Claims.Username] = await userManager.GetUserNameAsync(user);
                    claims[Claims.GivenName] = user.GivenName;
                    claims[Claims.MiddleName] = string.Empty;
                    claims[Claims.FamilyName] = user.FamilyName;
                    break;

                case OpenIddictScopeDefaults.Email:
                    claims[Claims.Email] = await userManager.GetEmailAsync(user) ?? string.Empty;
                    claims[Claims.EmailVerified] = await userManager.IsEmailConfirmedAsync(user);
                    break;

                case OpenIddictScopeDefaults.PhoneNumber:
                    claims[Claims.PhoneNumber] = await userManager.GetPhoneNumberAsync(user);
                    claims[Claims.PhoneNumberVerified] = await userManager.IsPhoneNumberConfirmedAsync(user);
                    break;

                case Scopes.Roles:
                    claims[Claims.Role] = await userManager.GetRolesAsync(user);
                    break;

                case OpenIddictScopeDefaults.Locale:
                    claims[Claims.Locale] = user.Culture;
                    break;

                case OpenIddictScopeDefaults.Zoneinfo:
                    claims[Claims.Zoneinfo] = TimeZoneInfo.Local.DisplayName;
                    break;

                case OpenIddictScopeDefaults.UpdatedAt:
                    claims[Claims.UpdatedAt] = user.DateUpdated.ToString(new CultureInfo(user.Culture));
                    break;

                default:
                    continue;
            }
        }

        return claims;
    }
}
