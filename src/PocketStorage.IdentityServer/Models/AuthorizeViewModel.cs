﻿using System.ComponentModel.DataAnnotations;

namespace PocketStorage.IdentityServer.Models;

public sealed class AuthorizeViewModel
{
    [Display(Name = "Application")] public string? ApplicationName { get; set; }

    [Display(Name = "Scope")] public string? Scope { get; set; }
}
