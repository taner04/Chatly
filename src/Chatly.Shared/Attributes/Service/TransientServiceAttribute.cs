using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Shared.Attributes.Service;

public sealed class TransientServiceAttribute(Type? serviceType = null, bool asSelf = false)
    : ServiceAttribute(ServiceLifetime.Transient, serviceType, asSelf);