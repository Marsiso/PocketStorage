namespace PocketStorage.Domain.Contracts;

public static class AntiforgeryDefaults
{
    public const string CookieName = "__Host-X-XSRF-TOKEN";
    public const string HeaderName = "X-XSRF-TOKEN";
}
