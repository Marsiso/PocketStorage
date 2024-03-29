﻿using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using PocketStorage.Application.Extensions;
using PocketStorage.Application.Filters;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.Core.Extensions;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Constants;
using PocketStorage.IdentityServer.Controllers.Common;
using PocketStorage.IdentityServer.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PocketStorage.IdentityServer.Controllers;

public class AuthorizationController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        ISender sender,
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    : WebControllerBase<AuthorizationController>
{
    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.
        if (claim.Subject == null)
        {
            yield break;
        }

        switch (claim.Type)
        {
            case Claims.Name:
            case Claims.Username:
            case Claims.GivenName:
            case Claims.MiddleName:
            case Claims.FamilyName:
                yield return Destinations.AccessToken;
                if (claim.Subject.HasScope(OpenIddictScopeDefaults.Name))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Email:
            case Claims.EmailVerified:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictScopeDefaults.Email))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.PhoneNumber:
            case Claims.PhoneNumberVerified:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictScopeDefaults.PhoneNumber))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Roles))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Locale:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictScopeDefaults.Locale))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.Zoneinfo:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictScopeDefaults.Zoneinfo))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case Claims.UpdatedAt:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictScopeDefaults.UpdatedAt))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            case PermitConstants.Claims.Permissions:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(PermitConstants.Scopes.Permissions))
                {
                    yield return Destinations.IdentityToken;
                }

                yield break;

            // Never include the security stamp in the access and identity tokens, as it is a
            // secret value.
            case "AspNet.Identity.SecurityStamp":
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }

    [Authorize]
    [FormValueRequired("submit.Accept")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(CancellationToken cancellationToken)
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the profile of the logged in user.
        User user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        object application = await applicationManager.FindByClientIdAsync(request.ClientId ?? "", cancellationToken) ?? throw new InvalidOperationException("Details concerning the calling client application.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        List<object> authorizations = await authorizationManager.FindAsync(
            await userManager.GetUserIdAsync(user),
            await applicationManager.GetIdAsync(application, cancellationToken) ?? "",
            Statuses.Valid,
            AuthorizationTypes.Permanent,
            request.GetScopes(), cancellationToken).ToListAsync();

        // Note: the same check is already made in the other action but is repeated hero to ensure a
        // malicious user cant abuse this POST-only endpoint and force it to return a valid response
        // without the external authorization.
        if (!authorizations.Any() && await applicationManager.HasConsentTypeAsync(application, ConsentTypes.External, cancellationToken))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application."
                }));
        }

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        ClaimsIdentity identity = new(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        // Add the claims that will be persisted in the tokens.
        identity = await identity.SetClaims(user.Email, sender, cancellationToken);

        // Note: in this sample, the granted scopes match the requested scope but you may want to
        // allow the user to uncheck specific scopes, for that, simply restrict the list of scopes
        // before calling SetScopes.
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes(), cancellationToken).ToListAsync());

        // Automatically create a permanent authorization to avoid requiring explicit consent for
        // future authorization or token requests containing the same scopes.
        object? authorization = authorizations.LastOrDefault();
        authorization ??= await authorizationManager.CreateAsync(
            identity,
            await userManager.GetUserIdAsync(user),
            await applicationManager.GetIdAsync(application, cancellationToken) ?? string.Empty,
            AuthorizationTypes.Permanent,
            identity.GetScopes(), cancellationToken);

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization, cancellationToken));
        identity.SetDestinations(GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        AuthenticateResult result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        // Try to retrieve the user principal stored in the authentication cookie and redirect the
        // user agent to the login page (or to an external provider) in the following cases:
        //
        // - If the user principal cant be extracted or the cookie is too old
        // - If prompt=login was specified by the client application
        // - If a max_age parameter was provided and the authentication cookie is not considered
        // "fresh" enough.
        if (result.Succeeded == false || request.HasPrompt(Prompts.Login) ||
            (request.MaxAge != null && result.Properties?.IssuedUtc != null && DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            // If the client application requested prompt less authentication, return an error
            // indicating that the user is not logged in.
            if (request.HasPrompt(Prompts.None))
            {
                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in" }));
            }

            // To avoid endless login -> authorization redirects, the prompt=login flag is removed
            // from the authorization request payload before redirecting the user.
            string prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

            List<KeyValuePair<string, StringValues>> parameters = Request.HasFormContentType
                ? Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList()
                : Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

            parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

            return Challenge(authenticationSchemes: IdentityConstants.ApplicationScheme, properties: new AuthenticationProperties { RedirectUri = Request.Path + QueryString.Create(parameters) });
        }

        // Retrieve the profile of the logged in user.
        User user = await userManager.GetUserAsync(result.Principal) ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        object application = await applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        List<object> authorizations = await authorizationManager.FindAsync(
            await userManager.GetUserIdAsync(user),
            await applicationManager.GetClientIdAsync(application, cancellationToken) ?? string.Empty,
            Statuses.Valid, AuthorizationTypes.Permanent,
            request.GetScopes(), cancellationToken).ToListAsync();

        switch (await applicationManager.GetConsentTypeAsync(application, cancellationToken))
        {
            // If the consent is external (e. g when authorizations are granted by a system
            // administrator), immediately return an error if no authorization can be found in the database.
            case ConsentTypes.External when !authorizations.Any():
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application."
                    }));

            // If the consent is implicit or if an authorization was found return an authorization
            // response without displaying the consent form.
            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Any():
            case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                // Create the claims-base identity that will be used by OpenIddict to generate tokens.
                ClaimsIdentity identity = new(
                    TokenValidationParameters.DefaultAuthenticationType,
                    Claims.Name,
                    Claims.Role);

                // Add the claims that will be persisted in the tokens.
                identity = await identity.SetClaims(user.Email, sender, cancellationToken);

                // Note: in this sample, the granted scopes match the requested scope but you may
                // want to allow the user to uncheck specific scopes for that, simply restrict the
                // list of scopes before calling SetScopes.
                identity.SetScopes(request.GetScopes());
                identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes(), cancellationToken).ToListAsync());

                // Automatically create a permanent authorization to avoid requiring explicit
                // consent for future authorization or token requests containing the same scopes.
                object? authorization = authorizations.LastOrDefault();
                authorization ??= await authorizationManager.CreateAsync(
                    identity,
                    await userManager.GetUserIdAsync(user),
                    await applicationManager.GetIdAsync(application) ?? "",
                    AuthorizationTypes.Permanent,
                    identity.GetScopes(), cancellationToken);

                identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization, cancellationToken));
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // At this point, no authorization was found in the database and an error must be
            // returned if the client application specified prompt=none in the authorization request.
            case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
            case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required." }));

            // In every other case, render the consent form
            default:
                return View(new AuthorizeViewModel { ApplicationName = await applicationManager.GetLocalizedDisplayNameAsync(application, cancellationToken), Host = HttpContext.Request.Host.Value, Scopes = request.Scope });
        }
    }

    [Authorize]
    [FormValueRequired("submit.Deny")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
    {
        OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            User? user = await userManager.FindByIdAsync(result.Principal?.GetClaim(Claims.Subject) ?? string.Empty);
            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid." }));
            }

            // Ensure the user is still allowed to sign in.
            if (!await signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?> { [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in." }));
            }

            ClaimsIdentity identity = new(result.Principal?.Claims,
                TokenValidationParameters.DefaultAuthenticationType,
                Claims.Name,
                Claims.Role);

            // Override the user claims present in the principal in case they changed since the
            // authorization code/refresh token was issued.
            identity = await identity.SetClaims(user.Email, sender, cancellationToken);
            identity.SetDestinations(GetDestinations);

            // Returning a SignInResult will ask openIddict to issue the appropriate access/identity tokens.
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View();

    [ActionName(nameof(Logout))]
    [HttpPost("~/connect/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutPost()
    {
        // Ask ASP.NET Core Identity to delete the local and external cookies created when the user
        // agent is redirected from the external identity provider after a successful authentication
        // flow (e. g Google or Facebook).
        await signInManager.SignOutAsync();

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent to the
        // post_logout_redirect_uri specified by the client application or to the RedirectUri
        // specified in the authentication properties if none was set.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }
}
