using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Shared.Attributes.Service;

public sealed class SingletonServiceAttribute(Type? serviceType = null, bool asSelf = false)
    : ServiceAttribute(ServiceLifetime.Singleton, serviceType, asSelf);
