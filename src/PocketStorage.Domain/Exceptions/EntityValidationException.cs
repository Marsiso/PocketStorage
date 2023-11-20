namespace PocketStorage.Domain.Exceptions;

public class EntityValidationException(string? name, string? identifier, string? message, Dictionary<string, string[]> errors) : Exception(message)
{
    public string? Name { get; } = name;
    public string? Identifier { get; } = identifier;
    public Dictionary<string, string[]> Errors { get; } = errors;
}
