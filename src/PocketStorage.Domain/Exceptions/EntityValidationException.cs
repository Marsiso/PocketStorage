namespace PocketStorage.Domain.Exceptions;

public class EntityValidationException : Exception
{
    public EntityValidationException(string? name, string? identifier, string? message, Dictionary<string, string[]> errors) : base(message)
    {
        Name = name;
        Identifier = identifier;
        Errors = errors;
    }

    public string? Name { get; }
    public string? Identifier { get; }
    public Dictionary<string, string[]> Errors { get; }
}
