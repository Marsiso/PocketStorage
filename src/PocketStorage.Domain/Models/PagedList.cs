namespace PocketStorage.Domain.Models;

public class PagedList<TItem>(List<TItem> items, int totalCount, int pageNumber, int pageSize)
{
    public int CurrentPage { get; init; } = pageNumber;
    public int TotalPages { get; init; } = (int)Math.Ceiling(totalCount / (double)pageSize);
    public int PageSize { get; init; } = pageSize;
    public int TotalCount { get; init; } = totalCount;
    public List<TItem> Items { get; init; } = items;

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
