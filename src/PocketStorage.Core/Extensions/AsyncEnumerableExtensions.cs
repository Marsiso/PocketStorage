namespace PocketStorage.Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static Task<List<TItem>> ToListAsync<TItem>(this IAsyncEnumerable<TItem> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), $"[{nameof(AsyncEnumerableExtensions)}] Null reference exception. Parameter: '{nameof(source)}' Value: '{null}'.");
        }

        return ExecuteAsync();

        async Task<List<TItem>> ExecuteAsync()
        {
            List<TItem> list = new();

            await foreach (TItem element in source)
            {
                list.Add(element);
            }

            return list;
        }
    }
}
