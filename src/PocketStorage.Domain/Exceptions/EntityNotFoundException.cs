namespace PocketStorage.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string? entityIdentifier, string? entityName)
    {
        EntityIdentifier = entityIdentifier;
        EntityName = entityName;
    }

    public string? EntityIdentifier { get; }
    public string? EntityName { get; }
}
