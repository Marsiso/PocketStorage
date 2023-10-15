using PocketStorage.Domain.Application.Models;

namespace PocketStorage.Domain.Contracts;

public interface IChangeTrackingEntity : IEntityBase
{
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }

    public User? UserCreatedBy { get; set; }
    public User? UserUpdatedBy { get; set; }
}
