using PocketStorage.BFF.Authorization.Enums;

namespace PocketStorage.Core.Application.Models;

public class RoleResource
{
    public string Name { get; set; } = string.Empty;
    public Permission Permissions { get; set; }
}
