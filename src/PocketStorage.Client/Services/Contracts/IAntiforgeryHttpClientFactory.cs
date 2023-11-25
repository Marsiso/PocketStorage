using PocketStorage.BFF.Authorization.Constants;

namespace PocketStorage.Client.Services.Contracts;

public interface IAntiforgeryHttpClientFactory
{
    public Task<HttpClient> CreateClientAsync(string clientName = AuthorizationConstants.AuthorizedClientName);
}
