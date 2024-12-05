// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace MaxRunSoftware.Utilities.Common;

public class ReflectionScanner
{
    private static readonly IEqualityComparer<Assembly> ASSEMBLY_COMPARER = AssemblyComparer.Default;
    private Assembly Assembly { get; }

    private HashSet<Type> TypesInThisAssembly { get; } = new();
    private HashSet<Type> TypesInOtherAssemblies { get; } = new();
    private HashSet<Type> TypesScanned { get; } = new();

    private HashSet<Assembly> SystemAssemblies { get; } = new(ASSEMBLY_COMPARER);
    private HashSet<Assembly> NonSystemAssemblies { get; } = new(ASSEMBLY_COMPARER);


    public BindingFlags Flags { get; set; } = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private ReflectionScanner(Assembly assembly) => Assembly = assembly;


    public static ReflectionScannerResult Scan(Assembly assembly)
    {
        var scanner = new ReflectionScanner(assembly);

        var typesToScan = GetTypes(assembly).ToList();

        while (typesToScan.Count > 0)
        {
            var typesScanned = new HashSet<Type>(scanner.TypesInThisAssembly);

            scanner.Scan(typesToScan);

            typesToScan.Clear();
            if (typesScanned.Count < scanner.TypesInThisAssembly.Count)
            {
                typesToScan = scanner.TypesInThisAssembly.Except(typesScanned).ToList();
            }
        }

        return new(scanner.Assembly, scanner.TypesInThisAssembly, scanner.TypesInOtherAssemblies);
    }

    private static Type[] GetTypes(Assembly assembly)
    {
        /*
        // https://stackoverflow.com/a/7889272
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.WhereNotNull().Where(o => o.TypeInitializer != null);
        }
        */
        return assembly.GetTypes();
    }
    
    private void Add(Type[] types)
    {
        foreach (var type in types)
        {
            Add(type);
        }
    }
    
    private void Add(Type? type)
    {
        if (type == null) return;
        if (ASSEMBLY_COMPARER.Equals(type.Assembly, Assembly)) TypesInThisAssembly.Add(type);
        else TypesInOtherAssemblies.Add(type);
    }

    private void Scan(IEnumerable<Type> typesToScan)
    {
        foreach (var type in typesToScan)
        {
            Scan(type);
        }
    }

    private void Scan(Type? type)
    {
        if (type == null) return;
        if (!TypesScanned.Add(type)) return;
        
        Add(type);
        var shouldSkip = SystemAssemblies.Contains(type.Assembly);
        if (!shouldSkip)
        {
            if (!NonSystemAssemblies.Contains(type.Assembly))
            {
                var isSystemAssembly = type.Assembly.IsSystemAssembly();
                if (isSystemAssembly)
                {
                    SystemAssemblies.Add(type.Assembly);
                    shouldSkip = true;
                }
                else
                {
                    NonSystemAssemblies.Add(type.Assembly);
                }
            }
        }

        if (shouldSkip) return;

        ScanType(type);
        Scan(type.BaseTypes());
    }

    private void ScanType(Type type)
    {
        ScanMemberInfo(type);
        Add(type.GenericTypeArguments);
        Add(type.GetInterfaces());

        var flags = Flags;
        ScanConstructorInfo(type.GetConstructors(flags));
        ScanMemberInfo(type.GetMembers(flags));
        ScanMethodInfo(type.GetMethods(flags));
        ScanPropertyInfo(type.GetProperties(flags));
        ScanFieldInfo(type.GetFields(flags));
        ScanEventInfo(type.GetEvents(flags));
        ScanMemberInfo(type.GetDefaultMembers());
        Add(type.GetElementType());
        Add(type.GetNestedTypes());

        if (type.IsEnum) Add(type.GetEnumUnderlyingType());
        if (type.IsGenericParameter) Add(type.GetGenericParameterConstraints());
        if (type.IsGenericType) Add(type.GetGenericTypeDefinition());
    }


    private void ScanConstructorInfo(params ConstructorInfo?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            ScanMethodBase(info);
        }
    }

    private void ScanEventInfo(params EventInfo?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            ScanMemberInfo(info);
            ScanMethodInfo(info.AddMethod);
            ScanMethodInfo(info.GetAddMethod());

            ScanMethodInfo(info.RaiseMethod);
            ScanMethodInfo(info.GetRaiseMethod());

            ScanMethodInfo(info.RemoveMethod);
            ScanMethodInfo(info.GetRemoveMethod());

            ScanMethodInfo(info.GetOtherMethods());

            Add(info.EventHandlerType);
        }
    }

    private void ScanFieldInfo(params FieldInfo?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            ScanMemberInfo(info);
            foreach (var t in info.GetOptionalCustomModifiers()) Add(t);
            foreach (var t in info.GetRequiredCustomModifiers()) Add(t);
            Add(info.FieldType);
        }
    }

    private void ScanMethodInfo(params MethodInfo?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            //if (info.IsAnonymous()) return;
            //if (info.IsInner()) return;
            ScanMethodBase(info);
            ScanParameterInfo(info.ReturnParameter);
            ScanCustomAttributeData(info.GetCustomAttributesData());
            Add(info.ReturnType);
        }
    }

    private void ScanPropertyInfo(params PropertyInfo?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            ScanMemberInfo(info);
            ScanParameterInfo(info.GetIndexParameters());
            ScanMethodInfo(info.GetMethod);
            ScanMethodInfo(info.SetMethod);
            Add(info.PropertyType);
        }
    }

    private void ScanMemberInfo(params MemberInfo?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            ScanCustomAttributes(info);
            ScanCustomAttributeData(info.GetCustomAttributesData());
        }
    }

    private void ScanMethodBase(params MethodBase?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            if (!info.IsConstructor && !StringComparer.Ordinal.Equals(info.Name, ".cctor"))
            {
                var reflectedType = info.ReflectedType;

                if (reflectedType != null && reflectedType.IsRealType())
                {
                    try
                    {
                        Add(info.GetGenericArguments());
                    }
                    catch (Exception e)
                    {
                        throw new TargetInvocationException($"Error {info}.GetGenericArguments()  {nameof(info.Name)}={info.Name}  {nameof(info.ReflectedType)}={info.ReflectedType?.FullNameFormatted()}", e);
                    }
                }
            }

            ScanParameterInfo(info.GetParameters());

            ScanMemberInfo(info);
        }
    }

    private void ScanParameterInfo(params ParameterInfo?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            Add(info.ParameterType);
            foreach (var attr in info.GetCustomAttributes(true)) Add(attr.GetType());
            ScanCustomAttributes(info);
            ScanCustomAttributeData(info.GetCustomAttributesData());
        }
    }

    private void ScanCustomAttributes(ICustomAttributeProvider? customAttributeProvider)
    {
        if (customAttributeProvider == null) return;
        foreach (var attr in customAttributeProvider.GetCustomAttributes(true).WhereNotNull()) Add(attr.GetType());
    }

    private void ScanCustomAttributeData(IEnumerable<CustomAttributeData?>? info)
    {
        foreach (var attr in info.OrEmpty()) ScanCustomAttributeData(attr);
    }

    private void ScanCustomAttributeData(params CustomAttributeData?[]? infos)
    {
        foreach (var info in infos.OrEmpty().WhereNotNull())
        {
            Add(info.AttributeType);
            ScanConstructorInfo(info.Constructor);
            foreach (var a in info.ConstructorArguments) Add(a.ArgumentType);
            foreach (var a in info.NamedArguments.OrEmpty()) Add(a.TypedValue.ArgumentType);
        }
    }
}
