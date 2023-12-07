namespace PocketStorage.Domain.Models;

public class QueryStringBase
{
    private const int MinPageSize = 10;
    private const int MaxPageSize = 50;
    private const int MinPageNumber = 1;

    private int _pageNumber = MinPageNumber;
    private int _pageSize = MinPageSize;

    public int PageSize
    {
        get => _pageSize;
        set =>
            _pageSize = value switch
            {
                >= 1 and < MaxPageSize => value,
                < 1 => MinPageSize,
                _ => MaxPageSize
            };
    }

    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            if (value > 0)
            {
                _pageNumber = value;
            }
            else
            {
                _pageNumber = 1;
            }
        }
    }

    public int RecordsOffset() => PageSize * (PageNumber - 1);
    public int RecordsToReturn() => PageSize;
}
