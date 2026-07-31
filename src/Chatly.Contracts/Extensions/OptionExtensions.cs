using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Contracts.Extensions;

public static class OptionExtensions
{
    extension(IConfiguration  configuration) 
    {
        public T GetOption<T>(string? sectionName = null!) where T : class
        {
            sectionName ??= typeof(T).Name;

            var options = configuration.GetSection(sectionName).Get<T>();
            ArgumentNullException.ThrowIfNull(sectionName);

            return options!;
        }
    }
    
    extension(IServiceCollection services) 
    {
        public IServiceCollection AddOption<T>(IConfiguration configuration, string? sectionName = null!) where T : class
        {
            sectionName ??= typeof(T).Name;

            services.AddOptions<T>()
                    .BindConfiguration(sectionName)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
            
            return services;
        }
    }
}