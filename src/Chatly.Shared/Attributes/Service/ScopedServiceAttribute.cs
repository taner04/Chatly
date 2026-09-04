using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Shared.Attributes.Service;

public sealed class ScopedServiceAttribute(Type? serviceType = null, bool asSelf = false)
    : ServiceAttribute(ServiceLifetime.Scoped, serviceType, asSelf);
