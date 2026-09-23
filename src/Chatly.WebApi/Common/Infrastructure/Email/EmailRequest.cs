namespace Chatly.WebApi.Common.Infrastructure.Email;

public sealed class EmailRequest(
    string subject,
    string template)
{
    private readonly Dictionary<string, string> _values = [];

    public string Subject { get; } = subject;
    public string Template { get; } = template;

    public IReadOnlyDictionary<string, string> Values => _values;

    public EmailRequest AddValue(string key, string value)
    {
        if (!_values.TryAdd(key, value))
        {
            throw new InvalidOperationException($"The key '{key}' already exists in the email values.");
        }

        return this;
    }
}