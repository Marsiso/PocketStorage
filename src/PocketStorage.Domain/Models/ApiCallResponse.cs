using PocketStorage.Domain.Enums;

namespace PocketStorage.Domain.Models;

public record struct ApiCallResponse<TResult>(RequestStatus Status, TResult? Result, ApiCallError? Error);
