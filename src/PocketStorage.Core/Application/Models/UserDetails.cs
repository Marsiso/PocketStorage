namespace PocketStorage.Core.Application.Models;

public class UserDetails
{
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneNumberConfirmed { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public string Culture { get; set; } = string.Empty;
    public string ProfilePhoto { get; set; } = string.Empty;
    public UserDetails? UserCreatedBy { get; set; }
    public UserDetails? UserUpdatedBy { get; set; }
}
