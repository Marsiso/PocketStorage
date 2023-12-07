namespace PocketStorage.Core.Application.Models;

public class CodeListResource
{
    public int CodeListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<CodeListItemResource>? CodeListItems { get; set; }
}
