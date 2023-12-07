namespace PocketStorage.Domain.Options;

public class Settings
{
    public const string SectionName = "Application";

    public required IdentitySettings Identity { get; set; }
    public required DatabaseSettings Database { get; set; }
    public required OpenIdConnectSettings OpenIdConnect { get; set; }
}

public class IdentitySettings
{
    public required List<AspNetIdentityUserSettings> Users { get; set; }
}

public class AspNetIdentityUserSettings
{
    public required string GivenName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public required string FamilyName { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public required bool EmailVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public required bool PhoneNumberVerified { get; set; }
    public required string Locale { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public required List<string> Roles { get; set; } = new();
}

public class DatabaseSettings
{
    public required string Host { get; set; } = string.Empty;
    public required int Port { get; set; }
    public required string Database { get; set; } = string.Empty;
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public required bool Pooling { get; set; }
}

public class OpenIdConnectSettings
{
    public required OpenIdConnectServerSettings Server { get; set; }
    public required List<OpenIdConnectClientSettings> Clients { get; set; }
}

public class OpenIdConnectServerSettings
{
    public required string Authority { get; set; } = string.Empty;

    public required List<string> SupportedScopes { get; set; } = new();
    public required List<string> SupportedClaims { get; set; } = new();
}

public class OpenIdConnectClientSettings
{
    public required string ClientId { get; set; } = string.Empty;
    public required string ClientSecret { get; set; } = string.Empty;
    public required string DisplayName { get; set; } = string.Empty;
    public required List<string> Scopes { get; set; } = new();
    public required List<string> Endpoints { get; set; } = new();
    public required List<string> GrantTypes { get; set; } = new();
    public required List<string> ResponseTypes { get; set; } = new();
    public required List<string> Requirements { get; set; } = new();
    public required List<string> RedirectUris { get; set; } = new();
    public required List<string> PostLogoutRedirectUris { get; set; } = new();
}
