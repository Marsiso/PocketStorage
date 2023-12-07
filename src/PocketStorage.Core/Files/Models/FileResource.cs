namespace PocketStorage.Core.Files.Models;

public class FileResource
{
    public int Id { get; set; }
    public string SafeName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }

    public FolderResource? Folder { get; set; }
}
