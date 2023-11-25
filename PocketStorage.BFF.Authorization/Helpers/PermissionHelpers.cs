using PocketStorage.BFF.Authorization.Enums;

namespace PocketStorage.BFF.Authorization.Helpers;

public static class PermissionHelpers
{
    public static List<Permission> GetPermissionValues() => Enum.GetValues(typeof(Permission)).OfType<Permission>().ToList();
}
