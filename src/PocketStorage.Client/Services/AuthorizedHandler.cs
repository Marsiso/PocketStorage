using System.Net;
using Microsoft.AspNetCore.Components.Authorization;

namespace PocketStorage.Client.Services;

public sealed class AuthorizedHandler : DelegatingHandler
{
    private readonly HostAuthenticationStateProvider _authenticationStateProvider;

    public AuthorizedHandler(HostAuthenticationStateProvider authenticationStateProvider) => _authenticationStateProvider = authenticationStateProvider;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AuthenticationState authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        HttpResponseMessage responseMessage;
        if (authState.User.Identity is { IsAuthenticated: false })
        {
            // If user is not authenticated, immediately set response status to 401 Unauthorized.
            responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
        else
        {
            responseMessage = await base.SendAsync(request, cancellationToken);
        }

        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
        {
            // If server returned 401 Unauthorized, redirect to login page.
            _authenticationStateProvider.SignIn();
        }

        return responseMessage;
    }
}
