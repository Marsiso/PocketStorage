using System.Net;

namespace PocketStorage.Domain.Models;

public class ApiCallError
{
    public ApiCallError()
    {
    }

    public ApiCallError(int statusCode, string? userFriendlyMessage, Exception? error)
    {
        StatusCode = statusCode;
        Error = error;
        UserFriendlyMessage = userFriendlyMessage;
    }

    public ApiCallError(int statusCode, Exception? error)
    {
        StatusCode = statusCode;
        Error = error;
    }

    public ApiCallError(HttpStatusCode code, string? userFriendlyMessage, Exception? error)
    {
        StatusCode = (int)code;
        Error = error;
        UserFriendlyMessage = userFriendlyMessage;
    }

    public ApiCallError(HttpStatusCode code, Exception? error)
    {
        StatusCode = (int)code;
        Error = error;
    }

    public ApiCallError(Exception? error) => Error = error;

    public int StatusCode { get; set; } = (int)HttpStatusCode.BadRequest;
    public string? UserFriendlyMessage { get; set; }
    public Exception? Error { get; set; }
}
