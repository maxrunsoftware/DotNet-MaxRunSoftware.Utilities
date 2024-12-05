using Microsoft.Extensions.DependencyInjection;

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// https://stackoverflow.com/a/75483576
/// </summary>
/// <param name="lifetime">Specifies the lifetime of the service</param>
/// <param name="interfaceType">Specific interface to register this service to</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped, Type? interfaceType = null) : Attribute
{
    public ServiceLifetime Lifetime { get; set; } = lifetime;
    public Type? InterfaceType { get; set; } = interfaceType;
    
    public ServiceDescriptor ToServiceDescriptor(Type type) => new(InterfaceType ?? type, type, Lifetime);
}

/// <summary>
/// https://stackoverflow.com/a/75483576
/// </summary>
/// <typeparam name="TInterface">The interface to register this service with</typeparam>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServiceAttribute<TInterface> : ServiceAttribute where TInterface : class
{
    /// <summary>
    /// https://stackoverflow.com/a/75483576
    /// </summary>
    /// <param name="lifetime">Specifies the lifetime of the service</param>
    public ServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped) : base(lifetime: lifetime, interfaceType: typeof(TInterface)) { }
}
