using PocketStorage.Domain.Contracts;

namespace PocketStorage.Domain.Models;

public class EntityBase : IEntityBase
{
    public bool IsActive { get; set; }
}
