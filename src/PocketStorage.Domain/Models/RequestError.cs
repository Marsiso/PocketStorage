using System.Text.Json.Serialization;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Domain.Models;

public class RequestError
{
    public RequestError()
    {
    }

    public RequestError(RequestStatus status, string? userFriendlyMessage, Exception? exception)
    {
        HttpStatus = (int)status;
        UserFriendlyMessage = userFriendlyMessage;
        Exception = exception;
    }

    public RequestError(int httpStatus, string? userFriendlyMessage, Exception? exception)
    {
        HttpStatus = httpStatus;
        UserFriendlyMessage = userFriendlyMessage;
        Exception = exception;
    }

    public int HttpStatus { get; set; } = (int)RequestStatus.Fail;
    public string? UserFriendlyMessage { get; set; }
    [JsonIgnore] public Exception? Exception { get; set; }
}
