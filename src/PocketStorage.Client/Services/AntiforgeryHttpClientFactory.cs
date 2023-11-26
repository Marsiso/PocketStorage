using Microsoft.JSInterop;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.Client.Services.Contracts;

namespace PocketStorage.Client.Services;

public sealed class AntiforgeryHttpClientFactory(IHttpClientFactory httpClientFactory, IJSRuntime javascriptRuntime) : IAntiforgeryHttpClientFactory
{
    public async Task<HttpClient> CreateClientAsync(string clientName = AuthorizationConstants.AuthorizedClientName)
    {
        string? token = await javascriptRuntime.InvokeAsync<string>("getAntiForgeryToken");

        HttpClient client = httpClientFactory.CreateClient(clientName);

        client.DefaultRequestHeaders.Add(AntiforgeryConstants.HeaderName, token);
        return client;
    }
}
