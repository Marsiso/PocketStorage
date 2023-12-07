namespace PocketStorage.Core.Files.Models;

public class FolderResource
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSize { get; set; }
    public int TotalCount { get; set; }

    public FolderResource? Parent { get; set; }
    public List<FileResource>? Files { get; set; }
    public List<FolderResource>? Children { get; set; }
}
