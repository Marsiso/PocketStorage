using PocketStorage.Domain.Enums;
using PocketStorage.Domain.Models;

namespace PocketStorage.Domain.Contracts;

public interface IRequestResult
{
    public RequestStatus Status { get; set; }
    public RequestError? Error { get; set; }
}
