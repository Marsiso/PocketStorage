namespace PocketStorage.Domain.Options;

public class ApplicationSettings
{
    public const string SectionName = "Application";

    public required AspNetSettings AspNet { get; set; }
    public required PostgresqlSettings Postgresql { get; set; }
    public required OpenIdConnectSettings OpenIdConnect { get; set; }
}

public class AspNetSettings
{
    public required AspNetIdentitySettings Identity { get; set; }
}

public class AspNetIdentitySettings
{
    public required List<AspNetIdentityUserSettings> Users { get; set; }
}

public class AspNetIdentityUserSettings
{
    public required string GivenName { get; set; }
    public string? MiddleName { get; set; }
    public required string FamilyName { get; set; }
    public required string Email { get; set; }
    public required bool EmailVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public required bool PhoneNumberVerified { get; set; }
    public required string Locale { get; set; }
    public required string Password { get; set; }
    public required List<string> Roles { get; set; }
}

public class PostgresqlSettings
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Database { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool Pooling { get; set; }
}

public class OpenIdConnectSettings
{
    public required OpenIdConnectServerSettings Server { get; set; }
    public required List<OpenIdConnectClientSettings> Clients { get; set; }
}

public class OpenIdConnectServerSettings
{
    public required string Authority { get; set; }
    public required List<string> SupportedScopes { get; set; }
    public required List<string> SupportedClaims { get; set; }
}

public class OpenIdConnectClientSettings
{
    public required string Id { get; set; }
    public required string Secret { get; set; }
    public required string DisplayName { get; set; }
    public required List<string> Scopes { get; set; }
    public required List<string> Endpoints { get; set; }
    public required List<string> GrantTypes { get; set; }
    public required List<string> ResponseTypes { get; set; }
    public required List<string> Requirements { get; set; }
    public required List<string> RedirectUris { get; set; }
    public required List<string> PostLogoutRedirectUris { get; set; }
}
