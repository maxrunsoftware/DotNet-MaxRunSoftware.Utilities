using Microsoft.Extensions.DependencyInjection;

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// Service Options
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class OptionsAttribute(string configSectionPath) : Attribute
{
    public string ConfigSectionPath { get; set; } = configSectionPath;
    
    public static OptionsAttribute? GetAttribute(Type type)
    {
        var attrs = type.GetCustomAttributes(false);
        foreach (var a in attrs)
        {
            if (a.GetType() == typeof(OptionsAttribute)) return (OptionsAttribute)a;
        }
        
        foreach (var a in attrs)
        {
            if (a is OptionsAttribute o) return o;
        }
        
        return null;
    }
}


public static class OptionsAttributeExtensions
{
    private static MethodSlim FindMethod(
        Type type,
        string name,
        Func<IEnumerable<MethodSlim>, IEnumerable<MethodSlim>> query
    )
    {
        //var t = typeof(OptionsServiceCollectionExtensions);
        //var n = nameof(OptionsServiceCollectionExtensions.AddOptions);
        var n = "AddOptions";
        
        var ms = query(type.GetMethodSlims(BindingFlags.Public | BindingFlags.Static).Where(o => o.Name.EqualsOrdinal(name))).ToList();
        
        if (ms.Count < 1)
            throw new ArgumentException(string.Format("Type {0} did not contain any methods named {1} that met criteria",
                type.FullNameFormatted(),
                n
            ), nameof(type));
        
        if (ms.Count > 1)
            throw new ArgumentException(string.Format("Type {0} contained {1} methods named {2} that met criteria: {3}",
                type.FullNameFormatted(),
                ms.Count,
                n,
                ms.OrderBy(o => o.NameFull, StringComparer.OrdinalIgnoreCase)
                    .Select((o, i) => $"[{i}]{o.NameFull}")
                    .ToStringDelimited("  ")
            ), nameof(type));
        
        return ms[0];
    }
    
    #region OptionsServiceCollectionExtensions.AddOptions
    
    public static readonly string OptionsServiceCollectionExtensions_Type_NameFull = /* typeof(OptionsServiceCollectionExtensions).FullName */ "Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions";
    public static readonly string OptionsServiceCollectionExtensions_Type_Name = OptionsServiceCollectionExtensions_Type_NameFull.Split('.').Last();

    private static MethodSlim? OptionsServiceCollectionExtensions_AddOptions;
    
    private static MethodSlim OptionsServiceCollectionExtensions_AddOptions_Get(Type typeofOptionsServiceCollectionExtensions) =>
        OptionsServiceCollectionExtensions_AddOptions ??= FindMethod(typeofOptionsServiceCollectionExtensions, "AddOptions", enumerable => enumerable
            .Where(o => o.GenericArguments.Length == 1)
            .Where(o => o.Parameters.Length == 1)
            .Where(o => o.Parameters[0].Parameter.Type.Type == typeof(IServiceCollection))
        );
    
    private static object AddOptions(Type typeofOptionsServiceCollectionExtensions, IServiceCollection services, Type optionsType)
    {
        var m = OptionsServiceCollectionExtensions_AddOptions_Get(typeofOptionsServiceCollectionExtensions);
        var mc = m.GetMethodCaller([optionsType,]);
        var optionsBuilder = mc.Invoke(null, [services,]).CheckNotNull();
        return optionsBuilder;
    }
    
    #endregion OptionsServiceCollectionExtensions.AddOptions
    
    #region OptionsBuilderConfigurationExtensions.BindConfiguration

    public static readonly string OptionsBuilderConfigurationExtensions_Type_NameFull = /* typeof(OptionsBuilderConfigurationExtensions).FullName */ "Microsoft.Extensions.DependencyInjection.OptionsBuilderConfigurationExtensions";
    public static readonly string OptionsBuilderConfigurationExtensions_Type_Name = OptionsBuilderConfigurationExtensions_Type_NameFull.Split('.').Last();

    private static MethodSlim? OptionsBuilderConfigurationExtensions_BindConfiguration;
    
    private static MethodSlim OptionsBuilderConfigurationExtensions_BindConfiguration_Get(Type typeofOptionsBuilderConfigurationExtensions) =>
        OptionsBuilderConfigurationExtensions_BindConfiguration ??= FindMethod(typeofOptionsBuilderConfigurationExtensions, "BindConfiguration", enumerable => enumerable
            .Where(o => o.GenericArguments.Length == 1)
            .Where(o => o.Parameters.Length == 3)
            .Where(o => o.Parameters[0].Parameter.Type.Type == o.ReturnType?.Type)
            .Where(o => o.Parameters[1].Parameter.Type.Type == typeof(string))
        );
    
    private static void BindConfiguration(Type typeofOptionsBuilderConfigurationExtensions, Type optionsType, object optionsBuilder, OptionsAttribute optionsAttribute)
    {
        var m = OptionsBuilderConfigurationExtensions_BindConfiguration_Get(typeofOptionsBuilderConfigurationExtensions);
        var mc = m.GetMethodCaller([optionsType,]); 
        mc.Invoke(null, [optionsBuilder, optionsAttribute.ConfigSectionPath, null, ]).CheckNotNull();
    }
    
    #endregion OptionsBuilderConfigurationExtensions.BindConfiguration
    
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services,
        Type typeof_OptionsServiceCollectionExtensions,
        Type typeof_OptionsBuilderConfigurationExtensions,
        Type optionsType,
        OptionsAttribute optionsAttribute
    )
    {
        var optionsBuilder = AddOptions(typeof_OptionsServiceCollectionExtensions, services, optionsType);
        BindConfiguration(typeof_OptionsBuilderConfigurationExtensions, optionsType, optionsBuilder, optionsAttribute);
        return services;
    }
    
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services,
        Type typeof_OptionsServiceCollectionExtensions,
        Type typeof_OptionsBuilderConfigurationExtensions,
        Type optionsType
    ) => AddOptionsAndBind(
        services,
        typeof_OptionsServiceCollectionExtensions,
        typeof_OptionsBuilderConfigurationExtensions,
        optionsType,
        OptionsAttribute.GetAttribute(optionsType).CheckNotNull()
    );
    
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services,
        Type typeof_OptionsServiceCollectionExtensions,
        Type typeof_OptionsBuilderConfigurationExtensions,
        IEnumerable<(Type, OptionsAttribute)> options,
        Func<Type, OptionsAttribute, bool>? predicate = null
    )
    {
        foreach (var (optionsType, optionsAttribute) in options)
        {
            if (predicate == null || predicate(optionsType, optionsAttribute))
            {
                services = AddOptionsAndBind(
                    services,
                    typeof_OptionsServiceCollectionExtensions,
                    typeof_OptionsBuilderConfigurationExtensions,
                    optionsType,
                    optionsAttribute
                );
            }
        }
        
        return services;
    }
    
    public static IServiceCollection AddOptionsAndBind(
        this IServiceCollection services,
        Type typeof_OptionsServiceCollectionExtensions,
        Type typeof_OptionsBuilderConfigurationExtensions,
        Assembly assembly,
        Func<Type, OptionsAttribute, bool>? predicate = null
    ) => AddOptionsAndBind(
        services,
        typeof_OptionsServiceCollectionExtensions,
        typeof_OptionsBuilderConfigurationExtensions,
        assembly.GetTypesWithAttribute<OptionsAttribute>(inherited: false),
        predicate: predicate
    );
}
