namespace PocketStorage.Domain.Exceptions;

public class EntityNotFoundException(string? entityIdentifier, string? entityName) : Exception
{
    public string? EntityIdentifier { get; } = entityIdentifier;
    public string? EntityName { get; } = entityName;
}
