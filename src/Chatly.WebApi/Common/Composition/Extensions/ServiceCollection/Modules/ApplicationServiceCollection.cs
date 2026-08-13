using Chatly.WebApi.Common.Behaviours;
using Chatly.WebApi.Common.Infrastructure;
using Chatly.WebApi.Features.Users.Services;
using FluentValidation;

namespace Chatly.WebApi.Common.Composition.Extensions.ServiceCollection.Modules;

internal static class ApplicationServiceCollection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddChatlyApplicationServices()
        {
            services.AddScoped<CurrentUserService>();
            services.AddScoped<UserService>();
            services.AddScoped<ProfilePictureService>();
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

            services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            return services;
        }
    }
}
