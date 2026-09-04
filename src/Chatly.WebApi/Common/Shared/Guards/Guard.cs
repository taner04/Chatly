using System.Net.Mail;
using System.Runtime.CompilerServices;
using Chatly.WebApi.Common.Shared.Guards.Exceptions;

namespace Chatly.WebApi.Common.Shared.Guards;

public static class Guard
{
    public static class Against
    {
        private static string Owner<TOwner>()
        {
            return typeof(TOwner).Name;
        }

        private static string Prop(string? paramName)
        {
            return GuardName.Clean(paramName);
        }

        // ---- Strings ----

        public static void NullOrEmpty<TOwner>(
            string? value,
            [CallerArgumentExpression(nameof(value))]
            string? paramName = null)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return;
            }

            var prop = Prop(paramName);
            GuardException.Throw(
                Owner<TOwner>(),
                prop,
                "InvalidEmpty",
                $"{prop} cannot be null or empty.");
        }

        public static void InvalidEmail<TOwner>(
            string value,
            [CallerArgumentExpression(nameof(value))]
            string? paramName = null)
        {
            if (MailAddress.TryCreate(value, out _))
            {
                return;
            }

            var prop = Prop(paramName);
            GuardException.Throw(
                Owner<TOwner>(),
                prop,
                "InvalidEmail",
                $"{prop} must be a valid email address.");
        }

    }
}
