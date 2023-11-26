namespace PocketStorage.Integration;

public class PocketStorageClient(HttpClient httpClient) : IDisposable
{
    private bool _disposed;

    void IDisposable.Dispose()
    {
        _disposed = true;

        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task<TResponse> CallAsync<TResponse>(Func<NSwagClient, Task<TResponse>> client) where TResponse : class
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(httpClient), $"Service: `{nameof(PocketStorageClient)}` Action: `{nameof(CallAsync)}` Property: `{nameof(httpClient)}`.");
        }

        return client(new NSwagClient(httpClient));
    }
}
