using Microsoft.Extensions.Configuration;

namespace Chatly.Shared.Extensions;

public static class OptionExtensions
{
    extension(IConfiguration configuration)
    {
        public T GetOption<T>(string? sectionName = null!) where T : class
        {
            sectionName ??= typeof(T).Name;

            var options = configuration.GetSection(sectionName).Get<T>();
            ArgumentNullException.ThrowIfNull(sectionName);

            return options!;
        }
    }
}