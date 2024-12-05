using Microsoft.Extensions.DependencyInjection;

namespace MaxRunSoftware.Utilities.Common;

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
    ) => AddServiceAttributeServices(
        services,
        assembly.GetTypesWithAttribute<ServiceAttribute>(),
        predicate: predicate
    );
}
