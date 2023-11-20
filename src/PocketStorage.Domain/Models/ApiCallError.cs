using System.Text.Json.Serialization;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Domain.Models;

public struct ApiCallError(RequestStatus status, string? userFriendlyMessage, Exception? error)
{
    public RequestStatus Status { get; set; } = status;
    public string? UserFriendlyMessage { get; set; } = userFriendlyMessage;

    [JsonIgnore] public Exception? Error { get; set; } = error;

    public readonly void Deconstruct(out RequestStatus status, out string? userFriendlyMessage, out Exception? error)
    {
        status = Status;
        userFriendlyMessage = UserFriendlyMessage;
        error = Error;
    }
}
