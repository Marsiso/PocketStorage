using Microsoft.AspNetCore.Authorization;
using PocketStorage.BFF.Authorization.Enums;

namespace PocketStorage.BFF.Authorization.Models;

public class PermitRequirement(Permission permission) : IAuthorizationRequirement
{
    public Permission Permission { get; } = permission;
}
