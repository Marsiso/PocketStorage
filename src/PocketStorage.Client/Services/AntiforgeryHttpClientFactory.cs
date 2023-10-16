using Microsoft.JSInterop;
using PocketStorage.Client.Services.Abstractions;
using PocketStorage.Domain.Contracts;

namespace PocketStorage.Client.Services;

public sealed class AntiforgeryHttpClientFactory : IAntiforgeryHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJSRuntime _jSRuntime;

    public AntiforgeryHttpClientFactory(IHttpClientFactory httpClientFactory, IJSRuntime jSRuntime)
    {
        _httpClientFactory = httpClientFactory;
        _jSRuntime = jSRuntime;
    }

    public async Task<HttpClient> CreateClientAsync(string clientName = AuthorizationDefaults.AuthorizedClientName)
    {
        string? token = await _jSRuntime.InvokeAsync<string>("getAntiForgeryToken");

        HttpClient client = _httpClientFactory.CreateClient(clientName);

        client.DefaultRequestHeaders.Add(AntiforgeryDefaults.HeaderName, token);

        return client;
    }
}
