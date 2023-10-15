namespace PocketStorage.Domain.Enums;

[Flags]
public enum Permission
{
    None = 1,
    All = ~None
}
