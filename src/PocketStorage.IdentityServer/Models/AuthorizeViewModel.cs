using System.ComponentModel.DataAnnotations;

namespace PocketStorage.IdentityServer.Models;

public class AuthorizeViewModel
{
    [Display(Name = "Application")] public string? ApplicationName { get; set; }

    [Display(Name = "Host")] public string? Host { get; set; }

    [Display(Name = "Scopes")] public string? Scopes { get; set; }
}
