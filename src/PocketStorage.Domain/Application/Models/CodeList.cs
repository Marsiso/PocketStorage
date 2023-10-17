using PocketStorage.Domain.Models;

namespace PocketStorage.Domain.Application.Models;

public class CodeList : ChangeTrackingEntity
{
    public int CodeListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<CodeListItem>? CodeListItems { get; set; }
}