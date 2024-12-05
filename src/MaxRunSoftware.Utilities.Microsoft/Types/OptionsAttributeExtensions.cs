using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace MaxRunSoftware.Utilities.Common;

public static class OptionsAttributeExtensions
{
    private static readonly MethodInfo method_AddOptionsAndBind_Internal = typeof(OptionsAttributeExtensions)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
        .First(o => o.Name.EqualsOrdinal(nameof(AddOptionsAndBind_Internal)));
    
    /// <summary>
    /// Gets an options builder that forwards Configure calls for the same <typeparamref name="TOptions" /> to the underlying service collection and binds to a configuration section.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the services to.</param>
    /// <param name="sectionName">The name of the configuration section to bind from.</param>
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.")]
    private static void AddOptionsAndBind_Internal<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        IServiceCollection services,
        string sectionName
    ) where TOptions : class => services.AddOptions<TOptions>().BindConfiguration(sectionName);
    
    /// <summary>
    /// Gets an options builder that forwards Configure calls for the same <paramref name="optionsType" /> to the underlying service collection and binds to a configuration section.
    /// </summary>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the services to.</param>
    /// <param name="optionsType">The options class</param>
    /// <param name="sectionName">The name of the configuration section to bind from.</param>
    /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that calls can be chained in it.</returns>
    public static IServiceCollection AddOptionsAndBind(this IServiceCollection services, Type optionsType, string sectionName)
    {
        method_AddOptionsAndBind_Internal
            .GetMethodCaller([optionsType])
            .Invoke(null, [services, sectionName])
            ;
        
        return services;
    }
    
    public static IServiceCollection AddOptionsAndBind(this IServiceCollection services, Type optionsType, OptionsAttribute optionsAttribute) => 
        services.AddOptionsAndBind(optionsType, optionsAttribute.ConfigSectionPath);
    
    public static IServiceCollection AddOptionsAndBind(this IServiceCollection services, Type optionsType) =>
        services.AddOptionsAndBind(optionsType, optionsType.GetAttribute<OptionsAttribute>().CheckNotNull());
     
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services, 
        IEnumerable<(Type, string)> options, 
        Func<Type, string, bool>? predicate = null
    )
    {
        foreach (var (optionsType, sectionName) in options)
        {
            if (predicate == null || predicate(optionsType, sectionName))
            {
                services = AddOptionsAndBind(services, optionsType, sectionName);
            }
        }
        
        return services;
    }
    
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services,
        IEnumerable<(Type, OptionsAttribute)> options,
        Func<Type, OptionsAttribute, bool>? predicate = null
    )
    {
        foreach (var (optionsType, optionsAttribute) in options)
        {
            if (predicate == null || predicate(optionsType, optionsAttribute))
            {
                services = AddOptionsAndBind(services, optionsType, optionsAttribute);
            }
        }
        
        return services;
    }
    
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services,
        Assembly assembly,
        Func<Type, OptionsAttribute, bool>? predicate = null
    ) => AddOptionsAndBind(
        services,
        assembly.GetTypesWithAttribute<OptionsAttribute>(inherited: false),
        predicate: predicate
    );
}
