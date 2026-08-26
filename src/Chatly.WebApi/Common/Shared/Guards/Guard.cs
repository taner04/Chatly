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

        public static void LengthBetween<TOwner>(
            string? value,
            int min,
            int max,
            [CallerArgumentExpression(nameof(value))]
            string? paramName = null)
        {
            if (value is null)
            {
                return;
            }

            var len = value.Length;
            if (len >= min && len <= max)
            {
                return;
            }

            var prop = Prop(paramName);
            GuardException.Throw(
                Owner<TOwner>(),
                prop,
                "InvalidLength",
                $"{prop} length must be between {min} and {max}. Actual: {len}.");
        }

        // ---- Numbers ----

        public static void NegativeOrZero<TOwner>(
            decimal value,
            [CallerArgumentExpression(nameof(value))]
            string? paramName = null)
        {
            if (value > 0)
            {
                return;
            }

            var prop = Prop(paramName);
            GuardException.Throw(
                Owner<TOwner>(),
                prop,
                "InvalidPositive",
                $"{prop} must be > 0. Actual: {value}.");
        }

        public static void Negative<TOwner>(
            int value,
            [CallerArgumentExpression(nameof(value))]
            string? paramName = null)
        {
            if (value >= 0)
            {
                return;
            }

            var prop = Prop(paramName);
            GuardException.Throw(
                Owner<TOwner>(),
                prop,
                "InvalidNonNegative",
                $"{prop} must be >= 0. Actual: {value}.");
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

        // ---- Uri ----

        public static void ValidAbsoluteHttpUrl<TOwner>(
            string value,
            [CallerArgumentExpression(nameof(value))]
            string? paramName = null)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                var prop = Prop(paramName);

                GuardException.Throw(
                    Owner<TOwner>(),
                    prop,
                    "InvalidAbsoluteUri",
                    $"{prop} must be a valid absolute http/https URI.");
            }
        }

        // ---- List ----
        public static void EmptyCollection<TOwner>(
            IEnumerable<TOwner>? value,
            [CallerArgumentExpression(nameof(value))]
            string? paramName = null)
        {
            if (value is not null && value.Any())
            {
                return;
            }

            var prop = Prop(paramName);
            GuardException.Throw(
                Owner<TOwner>(),
                prop,
                "InvalidEmptyCollection",
                $"{prop} collection cannot be null or empty.");
        }
    }
}