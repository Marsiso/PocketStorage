namespace PocketStorage.Core.Application.Models;

public class CodeListItemResource
{
    public int CodeListId { get; set; }
    public int CodeListItemId { get; set; }
    public string Value { get; set; } = string.Empty;
}
