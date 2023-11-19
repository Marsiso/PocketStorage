using PocketStorage.Domain.Enums;

namespace PocketStorage.Domain.Models;

public record struct ApiCallError(RequestStatus Status, string? UserFriendlyMessage, Exception? Error);
