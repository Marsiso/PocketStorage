namespace PocketStorage.Domain.Enums;

public enum RequestStatus
{
    Success = 200,
    EntityCreated = 201,
    EntityNotFound = 404,
    Fail = 400,
    Unauthorized = 401,
    Cancelled = 499,
    Error = 500
}
