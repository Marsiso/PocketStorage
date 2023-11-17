﻿using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PocketStorage.Domain.Application.DataTransferObjects;
using PocketStorage.Domain.Constants;

namespace PocketStorage.Client.Services;

public sealed class HostAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly TimeSpan _cacheRefreshInterval = TimeSpan.FromSeconds(60);

    private readonly HttpClient _client;
    private readonly ILogger<HostAuthenticationStateProvider> _logger;
    private readonly NavigationManager _navigation;

    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());
    private DateTimeOffset _lastCheck = DateTimeOffset.FromUnixTimeSeconds(0);

    public HostAuthenticationStateProvider(NavigationManager navigation, HttpClient client, ILogger<HostAuthenticationStateProvider> logger)
    {
        _navigation = navigation;
        _client = client;
        _logger = logger;
    }

    private async Task<ClaimsPrincipal> FetchUser()
    {
        UserInfo? userinfo = default;

        try
        {
            _logger.LogInformation("{BaseAddress}", _client.BaseAddress?.ToString());
            userinfo = await _client.GetFromJsonAsync<UserInfo>("api/userinfo");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, $"[{nameof(HostAuthenticationStateProvider)}] Fetch user failure.");
        }

        if (userinfo is not { IsAuthenticated: true })
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        ClaimsIdentity identity = new(nameof(HostAuthenticationStateProvider), userinfo.NameClaimType, userinfo.RoleClaimType);
        if (userinfo.Claims != null)
        {
            identity.AddClaims(userinfo.Claims.Select(claimValue => new Claim(claimValue.Type, claimValue.Value)));
        }

        return new ClaimsPrincipal(identity);
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = false)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
        if (useCache && dateTimeOffset < _lastCheck + _cacheRefreshInterval)
        {
            _logger.LogDebug($"[{nameof(HostAuthenticationStateProvider)}] Retrieving user from cache.");
            return _cachedUser;
        }

        _logger.LogDebug($"[{nameof(HostAuthenticationStateProvider)}] Fetching user.");
        _cachedUser = await FetchUser();
        _lastCheck = dateTimeOffset;

        return _cachedUser;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() => new(await GetUser(true));

    public void SignIn(string? customReturnUrl = default)
    {
        string? returnUrl = customReturnUrl != null ? _navigation.ToAbsoluteUri(customReturnUrl).ToString() : default;
        string encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? _navigation.Uri);
        Uri signinUrl = _navigation.ToAbsoluteUri($"{AuthorizationDefaults.LogInPath}?returnUrl={encodedReturnUrl}");
        _navigation.NavigateTo(signinUrl.ToString(), true);
    }
}
