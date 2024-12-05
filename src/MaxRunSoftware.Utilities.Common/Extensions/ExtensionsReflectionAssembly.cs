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

using System.Diagnostics;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsReflectionAssembly
{
    public static string? GetFileVersion(this Assembly assembly) => FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;

    public static string? GetVersion(this Assembly assembly) => assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;
    
    public static bool IsSystemAssembly(this Assembly assembly)
    {
        assembly.CheckNotNull(nameof(assembly));

        var namePrefixes = new[]
        {
            "CommonLanguageRuntimeLibrary",
            "System.",
            "System,",
            "mscorlib,",
            "netstandard,",
            "Microsoft.CSharp",
            "Microsoft.VisualStudio",
        };

        foreach (var name in AssemblyNames(assembly))
        {
            var s = name?.ToString().TrimOrNull();
            if (s != null && s.StartsWithAny(StringComparison.OrdinalIgnoreCase, namePrefixes)) return true;
        }

        return false;

        static IEnumerable<object?> AssemblyNames(Assembly a)
        {
            foreach (var module in a.Modules) yield return module.ScopeName;
            yield return a.FullName;
            yield return a;
            var an = a.GetName();
            yield return an;
            yield return an.FullName;
            yield return an.Name;
        }
    }
    
    public static ((AssemblyName, Assembly)[], (AssemblyName, Exception)[]) GetReferencedAssembliesAndExceptions(this Assembly assembly)
    {
        var list = new List<(AssemblyName, Assembly)>();
        var listExceptions = new List<(AssemblyName, Exception)>();
        var referencedAssemblyNames = assembly.GetReferencedAssemblies();
        foreach (var referencedAssemblyName in referencedAssemblyNames.WhereNotNull())
        {
            // TODO: avoid Assembly.Load and perhaps switch to something else that doesn't full load, perhaps https://learn.microsoft.com/en-us/dotnet/standard/assembly/inspect-contents-using-metadataloadcontext
            var referencedAssembly = referencedAssemblyName.GetAssembly(out var e);
            if (e == null)
            {
                list.Add((referencedAssemblyName, referencedAssembly!));
            }
            else
            {
                listExceptions.Add((referencedAssemblyName, e));
            }
            
        }

        return (list.ToArray(), listExceptions.ToArray());
    }

    public static Assembly[] GetAssembliesVisible(bool recursive = false)
    {
        var assemblies = new List<Assembly?>();

        try { assemblies.Add(Assembly.GetEntryAssembly()); }
        catch (Exception) { /* ignore */ }

        try { assemblies.Add(Assembly.GetCallingAssembly()); }
        catch (Exception) { /* ignore */ }

        try { assemblies.Add(Assembly.GetExecutingAssembly()); }
        catch (Exception) { /* ignore */ }

        try { assemblies.Add(MethodBase.GetCurrentMethod()!.DeclaringType?.Assembly); }
        catch (Exception) { /* ignore */ }

        try { assemblies.Add(MethodBase.GetCurrentMethod()!.DeclaringType?.Assembly); }
        catch (Exception) { /* ignore */ }

        try { assemblies.Add(typeof(Constant).Assembly); }
        catch (Exception) { /* ignore */ }
        
        try { assemblies.Add(typeof(ExtensionsReflectionAssembly).Assembly); }
        catch (Exception) { /* ignore */ }
        
        try
        {
            var stackTrace = new StackTrace(); // get call stack
            var stackFrames = stackTrace.GetFrames(); // get method calls (frames)
            foreach (var stackFrame in stackFrames)
            {
                try { assemblies.Add(stackFrame.GetMethod()?.GetType().Assembly); }
                catch (Exception) { /* ignore */ }
            }
        }
        catch (Exception) { /* ignore */ }


        var d = new Dictionary<AssemblyName, Assembly>(AssemblyNameComparer.Default);
        foreach (var assembly in assemblies.WhereNotNull())
        {
            d.TryAdd(assembly.GetName(), assembly);
        }

        if (recursive)
        {
            var alreadyGotReferencedAssemblies = new HashSet<AssemblyName>(AssemblyNameComparer.Default);
            var queue = new Queue<Assembly>(d.Values);

            while (queue.TryDequeue(out var asm))
            {
                var asmName = asm.GetName();
                if (!alreadyGotReferencedAssemblies.Add(asmName)) continue;
                
                foreach (var referencedAssemblyName in asm.GetReferencedAssemblies())
                {
                    var referencedAssembly = referencedAssemblyName.GetAssembly(out var e);
                    if (referencedAssembly != null)
                    {
                        d.TryAdd(referencedAssemblyName, referencedAssembly);
                    }
                    
                    if (!alreadyGotReferencedAssemblies.Contains(referencedAssemblyName))
                    {
                        if (referencedAssembly != null)
                        {
                            queue.Enqueue(referencedAssembly);
                        }
                    }
                }
            }
        }

        return d.Values.ToArray();
    }

    public static bool IsEqual(this Assembly? x, Assembly? y) => AssemblyComparer.Default.Equals(x, y);
    
    public static int Compare(this Assembly? x, Assembly? y) => AssemblyComparer.Default.Compare(x, y);
}
