using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Chatly.Shared.Extensions;

public static class OptionExtensions
{
    extension(IConfiguration configuration)
    {
        public T GetOption<T>(string? sectionName = null) where T : class
        {
            sectionName ??= typeof(T).Name;

            var option = configuration.GetSection(sectionName).Get<T>()
                         ?? throw new InvalidOperationException($"Configuration section '{sectionName}' is missing.");

            Validate(option, sectionName);
            return option;
        }
    }

    private static void Validate<T>(T option, string sectionName) where T : class
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(option, new ValidationContext(option), results, true))
        {
            return;
        }

        throw new OptionsValidationException(
            sectionName,
            typeof(T),
            results.Select(result =>
                $"{sectionName}.{string.Join(", ", result.MemberNames)}: {result.ErrorMessage}"));
    }
}
