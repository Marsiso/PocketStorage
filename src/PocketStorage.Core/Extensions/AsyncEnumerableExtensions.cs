namespace PocketStorage.Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        if (source is null)
        {
            string errorMessage = $"[{nameof(AsyncEnumerableExtensions)}] Null reference exception. Parameter: '{nameof(source)}' Value: '{null}'.";

            throw new ArgumentNullException(nameof(source), errorMessage);
        }

        return ExecuteAsync();

        async Task<List<T>> ExecuteAsync()
        {
            List<T> list = new();

            await foreach (T element in source)
            {
                list.Add(element);
            }

            return list;
        }
    }
}
