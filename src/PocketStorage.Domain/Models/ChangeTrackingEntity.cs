using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Contracts;

namespace PocketStorage.Domain.Models;

public class ChangeTrackingEntity : EntityBase, IChangeTrackingEntity
{
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }

    public User? UserCreatedBy { get; set; }
    public User? UserUpdatedBy { get; set; }
}
