namespace PocketStorage.Domain.Enums;

public enum RequestStatus
{
    Success = 200,
    EntityCreated = 201,
    EntityNotFound = 404,
    Fail = 400,
    ValidationFailure = 400,
    Cancelled = 499,
    Error = 500
}
