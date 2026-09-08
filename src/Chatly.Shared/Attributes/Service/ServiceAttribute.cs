using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Shared.Attributes.Service;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public abstract class ServiceAttribute(ServiceLifetime lifetime, Type? serviceType = null, bool asSelf = false)
    : Attribute
{
    internal ServiceLifetime Lifetime { get; } = lifetime;
    internal Type? ServiceType { get; } = serviceType;
    internal bool AsSelf { get; } = asSelf;
}
