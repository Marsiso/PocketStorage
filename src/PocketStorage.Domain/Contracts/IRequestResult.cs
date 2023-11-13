using PocketStorage.Domain.Models;

namespace PocketStorage.Domain.Contracts;

public interface IRequestResult
{
    public ApiCallError? Error { get; set; }
}
