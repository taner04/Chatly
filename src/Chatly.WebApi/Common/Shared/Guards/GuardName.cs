using System.Text;

namespace Chatly.WebApi.Common.Shared.Guards;

public static class GuardName
{
    public static string Clean(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return "Value";
        }

        var s = expression.Trim().Replace("!", string.Empty);

        // If it's something like "GetPrice()" -> "GetPrice"
        if (s.EndsWith("()"))
        {
            s = s[..^2];
        }

        // Special-case: if expression ends with ".Value", prefer the segment before it
        // Examples:
        // "product.Stock.Value" -> "Stock"
        // "dto.Price.Value" -> "Price"
        // But keep "Value" if that's actually the only identifier.
        var segments = s.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length >= 2 && string.Equals(segments[^1], "Value", StringComparison.Ordinal))
        {
            s = segments[^2];
        }
        else
        {
            // take last part after '.'
            var lastDot = s.LastIndexOf('.');
            if (lastDot >= 0 && lastDot < s.Length - 1)
            {
                s = s[(lastDot + 1)..];
            }
        }

        // remove indexers: items[0] -> items
        var bracket = s.IndexOf('[');
        if (bracket > 0)
        {
            s = s[..bracket];
        }

        // keep only identifier chars
        s = ExtractIdentifier(s);

        return string.IsNullOrWhiteSpace(s) ? "Value" : ToPascalCase(s);
    }

    private static string ExtractIdentifier(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                sb.Append(ch);
            }
        }

        return sb.ToString();
    }

    private static string ToPascalCase(string input)
    {
        return input.Length switch
        {
            0 => input,
            1 => input.ToUpperInvariant(),
            _ => char.ToUpperInvariant(input[0]) + input[1..]
        };
    }
}