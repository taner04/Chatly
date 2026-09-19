namespace Chatly.Contracts.Common.Policies;

public static class UsernamePolicy
{
    public const int MaxLength = 32;
    public const string Pattern = "^[a-zA-Z0-9_]+$";
}