using Microsoft.Extensions.DependencyInjection;

namespace Chatly.Shared.Attributes.Service;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public abstract class ServiceAttribute(ServiceLifetime lifetime, Type? serviceType = null, bool asSelf = false)
    : Attribute
{
    public ServiceLifetime Lifetime { get; } = lifetime;
    public Type? ServiceType { get; } = serviceType;
    public bool AsSelf { get; } = asSelf;
}
