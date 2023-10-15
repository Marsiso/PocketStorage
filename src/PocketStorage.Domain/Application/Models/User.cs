﻿using Microsoft.AspNetCore.Identity;
using PocketStorage.Domain.Constants;
using PocketStorage.Domain.Contracts;

namespace PocketStorage.Domain.Application.Models;

public class User : IdentityUser, IChangeTrackingEntity
{
    public string Culture { get; set; } = CultureDefaults.Default;
    public string? ProfilePhoto { get; set; }
    public bool IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public User? UserCreatedBy { get; set; }
    public User? UserUpdatedBy { get; set; }
}