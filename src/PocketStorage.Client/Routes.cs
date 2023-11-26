namespace PocketStorage.Client;

public static class Routes
{
    public const string Homepage = "/";
    public const string Privacy = "/privacy";

    public static class User
    {
        public const string Base = "/user";

        public const string Claims = Base + "/claims";
    }
}
