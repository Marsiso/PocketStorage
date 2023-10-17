using PocketStorage.Domain.Models;

namespace PocketStorage.Domain.FileSystem.Models;

public class Folder : ChangeTrackingEntity
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSize { get; set; }
    public int TotalCount { get; set; }
    public Folder? Parent { get; set; }
    public ICollection<Folder>? Children { get; set; }
    public ICollection<File>? Files { get; set; }
}