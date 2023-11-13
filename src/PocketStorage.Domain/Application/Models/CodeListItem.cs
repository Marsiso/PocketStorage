using PocketStorage.Domain.Files.Models;
using PocketStorage.Domain.Models;

namespace PocketStorage.Domain.Application.Models;

public class CodeListItem : ChangeTrackingEntity
{
    public int CodeListItemId { get; set; }
    public int CodeListId { get; set; }
    public string Value { get; set; } = string.Empty;
    public ICollection<Folder>? Folders { get; set; }
}
