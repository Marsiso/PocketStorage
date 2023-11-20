using PocketStorage.Domain.Models;

namespace PocketStorage.Domain.Files.Models;

public class File : ChangeTrackingEntity
{
    public int Id { get; set; }
    public int FolderId { get; set; }
    public string SafeName { get; set; } = string.Empty;
    public string UnsafeName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public Folder? Folder { get; set; }
}
