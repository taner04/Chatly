namespace Chatly.WebApi.Features.Users.Models;

public static class Security
{
    public static class Policies
    {
        public const string Admin = "admin-role";
        public const string User = "user-role";
    }

    public static class RateLimiting
    {
        public const string Global = "global-ratelimit";
    }
}