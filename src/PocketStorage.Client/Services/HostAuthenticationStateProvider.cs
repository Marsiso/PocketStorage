using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Models;

namespace PocketStorage.Client.Services;

public sealed class HostAuthenticationStateProvider(NavigationManager navigation, HttpClient client, ILogger<HostAuthenticationStateProvider> logger) : AuthenticationStateProvider
{
    private static readonly TimeSpan _cacheRefreshInterval = TimeSpan.FromSeconds(60);

    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());
    private DateTimeOffset _lastCheck = DateTimeOffset.FromUnixTimeSeconds(0);

    private async Task<ClaimsPrincipal> FetchUser()
    {
        ApiCallResponse<UserInfo>? response = null;

        try
        {
            logger.LogInformation("{BaseAddress}", client.BaseAddress?.ToString());
            response = await client.GetFromJsonAsync<ApiCallResponse<UserInfo>>("api/userinfo");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, $"[{nameof(HostAuthenticationStateProvider)}] Fetch user failure.");
        }

        if (response is not { Result.IsAuthenticated: true })
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        ClaimsIdentity identity = new(nameof(HostAuthenticationStateProvider), response.Value.Result.NameClaimType, response.Value.Result.RoleClaimType);
        if (response.Value.Result.Claims is { Count: > 0 })
        {
            identity.AddClaims(response.Value.Result.Claims.Select(claimValue => new Claim(claimValue.Type, claimValue.Value)));
        }

        return new ClaimsPrincipal(identity);
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = false)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
        if (useCache && dateTimeOffset < _lastCheck + _cacheRefreshInterval)
        {
            logger.LogDebug($"[{nameof(HostAuthenticationStateProvider)}] Retrieving user from cache.");
            return _cachedUser;
        }

        logger.LogDebug($"[{nameof(HostAuthenticationStateProvider)}] Fetching user.");
        _cachedUser = await FetchUser();
        _lastCheck = dateTimeOffset;

        return _cachedUser;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() => new(await GetUser(true));

    public void SignIn(string? customReturnUrl = default)
    {
        string? returnUrl = customReturnUrl != null ? navigation.ToAbsoluteUri(customReturnUrl).ToString() : default;
        string encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? navigation.Uri);
        Uri signinUrl = navigation.ToAbsoluteUri($"{AuthorizationConstants.LogInPath}?returnUrl={encodedReturnUrl}");
        navigation.NavigateTo(signinUrl.ToString(), true);
    }
}
