using Chatly.WebApi.Common.Behaviours;
using FluentValidation;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class ApplicationServiceCollection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyApplicationServices()
        {
            services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.GenerateTypesAsInternal = true;
                options.PipelineBehaviors =
                [
                    typeof(LoggingBehaviour<,>),
                    typeof(UserProvisioningBehaviour<,>),
                    typeof(FluentValidationBehaviour<,>)
                ];
            });

            services.AddSignalR();
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            return services;
        }
    }
}