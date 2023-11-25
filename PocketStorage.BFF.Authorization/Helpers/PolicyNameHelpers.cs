using PocketStorage.BFF.Authorization.Enums;
using static System.String;

namespace PocketStorage.BFF.Authorization.Helpers;

public static class PolicyNameHelpers
{
    public const string Prefix = "";

    public static bool IsValidPolicyName(string? policyName) => !IsNullOrWhiteSpace(policyName) && policyName.StartsWith(Prefix);

    public static string GetPolicyNameFor(Permission permissions) => $"{Prefix}{(int)permissions}";

    public static Permission GetPermissionsFrom(string? policyName)
    {
        if (IsNullOrWhiteSpace(policyName))
        {
            return Permission.None;
        }

        if (int.TryParse(policyName[Prefix.Length..], out int permissionsValue))
        {
            return (Permission)permissionsValue;
        }

        return Permission.None;
    }
}
