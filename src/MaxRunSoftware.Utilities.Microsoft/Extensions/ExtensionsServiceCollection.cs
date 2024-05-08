using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsServiceCollection
{
    // private class Opts { public required string Name { get; set; } }
    
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services,
        Type optionsType,
        OptionsAttribute optionsAttribute)
    {
        //var b = OptionsServiceCollectionExtensions.AddOptions<Opts>(services);
        //OptionsBuilderConfigurationExtensions.BindConfiguration(b, optionsAttribute.ConfigSectionPath);
        
        var mcAddOptions = typeof(OptionsServiceCollectionExtensions).GetMethodSlims(BindingFlags.Public | BindingFlags.Static)
                .Where(o => o.Name.EqualsOrdinal(nameof(OptionsServiceCollectionExtensions.AddOptions)))
                .Where(o => o.GenericArguments.Length == 1)
                .Where(o => o.Parameters.Length == 1)
                .Where(o => o.Parameters[0].Parameter.Type.Type == typeof(IServiceCollection))
                .ToList()
                .First()
                .GetMethodCaller(genericTypeArguments: [optionsType])
            ;
        
        var mcBindConfiguration = typeof(OptionsBuilderConfigurationExtensions).GetMethodSlims(BindingFlags.Public | BindingFlags.Static)
                .Where(o => o.Name.EqualsOrdinal(nameof(OptionsBuilderConfigurationExtensions.BindConfiguration)))
                .Where(o => o.GenericArguments.Length == 0)
                .Where(o => o.Parameters.Length == 3)
                .Where(o => o.Parameters[1].Parameter.Type.Type == typeof(string))
                .ToList()
                .First()
                .GetMethodCaller()
            ;
        
        var builder = mcAddOptions.Invoke(null, [services,]);
        mcBindConfiguration.Invoke(null, [builder, optionsAttribute.ConfigSectionPath, null]);
        
        return services;
    }
    
            
            
    /// <summary>
    /// Gets an options builder that forwards Configure calls for the same <typeparamref name="TOptions" /> to the underlying service collection and binds to a configuration section.
    /// </summary>
    /// <typeparam name="TOptions">The options type to be configured.</typeparam>
    /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the services to.</param>
    /// <param name="configSectionPath">The name of the configuration section to bind from.</param>
    /// <param name="configureBinder">Optional. Used to configure the <see cref="T:Microsoft.Extensions.Configuration.BinderOptions" />.</param>
    /// <returns>The <see cref="T:Microsoft.Extensions.Options.OptionsBuilder`1" /> so that configure calls can be chained in it.</returns>
    [RequiresDynamicCode("Binding strongly typed objects to configuration values may require generating dynamic code at runtime.")]
    [RequiresUnreferencedCode("TOptions's dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static OptionsBuilder<TOptions> AddOptionsAndBind<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TOptions>(
        this IServiceCollection services, 
        string? configSectionPath = null,
        Action<BinderOptions>? configureBinder = null
        ) where TOptions : class
    {
        
        //services.AddOptions<DbConnectionProviderOptions>().BindConfiguration(DbConnectionProviderOptions.SECTION);
        
        return services.AddOptions<TOptions>(Microsoft.Extensions.Options.Options.DefaultName);
    }
}
