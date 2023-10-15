using Microsoft.AspNetCore.Identity;
using PocketStorage.Domain.Contracts;
using PocketStorage.Domain.Enums;

namespace PocketStorage.Domain.Application.Models;

public class Role : IdentityRole, IChangeTrackingEntity
{
    public Permission Permissions { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public User? UserCreatedBy { get; set; }
    public User? UserUpdatedBy { get; set; }
}
