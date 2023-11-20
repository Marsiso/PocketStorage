﻿namespace PocketStorage.Domain.Enums;

[Flags]
public enum Permission
{
    None = 1,
    ViewPermissions = 2,
    EditPermission = 4,
    ViewRoles = 8,
    EditRoles = 16,
    ViewUsers = 32,
    EditUsers = 64,
    ViewFiles = 128,
    EditFiles = 256,
    All = ~None
}
