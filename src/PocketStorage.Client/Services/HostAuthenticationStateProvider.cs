using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.Integration;

namespace PocketStorage.Client.Services;

public sealed class HostAuthenticationStateProvider(NavigationManager navigation, PocketStorageClient client, ILogger<HostAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private static readonly TimeSpan _cacheRefreshInterval = TimeSpan.FromSeconds(60);

    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());
    private DateTimeOffset _lastCheck = DateTimeOffset.FromUnixTimeSeconds(0);

    private async Task<ClaimsPrincipal> FetchUser()
    {
        ApiCallResponseWrapper<ApiCallResponseOfUserInfo>? response = null;

        try
        {
            response = await client.CallAsync(client => client.ApiUserinfoAsync());
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, $"Service: `{nameof(HostAuthenticationStateProvider)}` Message: `Fetch user failure.`.");
        }

        if (response is not { Result: { Status: RequestStatus.Success, Result.IsAuthenticated: true } })
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        ClaimsIdentity identity = new(nameof(HostAuthenticationStateProvider), response.Result.Result.NameClaimType, response.Result.Result.RoleClaimType);
        if (response.Result.Result is { Claims.Count: > 0 })
        {
            identity.AddClaims(response.Result.Result.Claims.Select(claimValue => new Claim(claimValue.Type, claimValue.Value)));
        }

        return new ClaimsPrincipal(identity);
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = false)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
        if (useCache && dateTimeOffset < _lastCheck + _cacheRefreshInterval)
        {
            logger.LogDebug($"Service: `{nameof(HostAuthenticationStateProvider)}` Message: `Retrieving user from cache.`.");
            return _cachedUser;
        }

        logger.LogDebug($"Service: `{nameof(HostAuthenticationStateProvider)}` Message: `Fetching user.`");

        _cachedUser = await FetchUser();
        _lastCheck = dateTimeOffset;

        return _cachedUser;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() => new(await GetUser(true));

    public void SignIn(string? customReturnUrl = null)
    {
        string? returnUrl = customReturnUrl != null ? navigation.ToAbsoluteUri(customReturnUrl).ToString() : null;
        string encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? navigation.Uri);

        Uri signInUrl = navigation.ToAbsoluteUri($"{AuthorizationConstants.SignInRoute}?returnUrl={encodedReturnUrl}");

        navigation.NavigateTo(signInUrl.ToString(), true);
    }
}
