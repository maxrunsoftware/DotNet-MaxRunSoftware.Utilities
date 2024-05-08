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
    
    public static IEnumerable<(Type, ServiceAttribute)> GetTypesWithAttribute(Assembly assembly) => assembly.GetTypesWithAttribute<ServiceAttribute>(inherited: false);
    
    public static IEnumerable<(Type, ServiceAttribute)> GetTypesWithAttribute<TInAssembly>() => GetTypesWithAttribute(typeof(TInAssembly).Assembly);
    
    public static ServiceAttribute? GetAttribute(Type type)
    {
        var attrs = type.GetCustomAttributes(false);
        foreach (var a in attrs)
        {
            if (a.GetType() == typeof(ServiceAttribute)) return (ServiceAttribute)a;
        }
        
        foreach (var a in attrs)
        {
            if (a is ServiceAttribute o) return o;
        }
        
        return null;
    }
    
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


public static class ServiceAttributeExtensions
{
    public static IServiceCollection AddServiceAttributeServices(
        this IServiceCollection services,
        IEnumerable<(Type, ServiceAttribute)> serviceAttributeServices,
        Func<Type, ServiceAttribute, bool>? predicate = null
    )
    {
        foreach (var (serviceType, serviceAttribute) in serviceAttributeServices)
        {
            if (predicate == null || predicate(serviceType, serviceAttribute))
            {
                services.Add(serviceAttribute.ToServiceDescriptor(serviceType));
            }
        }
        
        return services;
    }
    
    public static IServiceCollection AddServiceAttributeServices(
        this IServiceCollection services,
        Assembly assembly,
        Func<Type, ServiceAttribute, bool>? predicate = null
    )
    {
        return AddServiceAttributeServices(
            services,
            ServiceAttribute.GetTypesWithAttribute(assembly),
            predicate: predicate
        );
    }
    
    public static IServiceCollection AddServiceAttributeServices<TInAssembly>(
        this IServiceCollection services,
        Func<Type, ServiceAttribute, bool>? predicate = null
    ) => AddServiceAttributeServices(
        services,
        ServiceAttribute.GetTypesWithAttribute<TInAssembly>(),
        predicate: predicate
    );
}
