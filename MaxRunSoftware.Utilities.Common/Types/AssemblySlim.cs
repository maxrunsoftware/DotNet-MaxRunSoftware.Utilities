// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
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
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace MaxRunSoftware.Utilities;

[PublicAPI]
public sealed class AssemblySlim : IEquatable<AssemblySlim>, IComparable<AssemblySlim>, IComparable
{
    public Assembly Assembly { get; }
    public string NameFull { get; }
    private readonly int getHashCode;

    public AssemblySlim(Assembly assembly)
    {
        Assembly = assembly.CheckNotNull(nameof(assembly));
        NameFull = NameFull_Build(assembly);
        getHashCode = StringComparer.Ordinal.GetHashCode(NameFull);
    }

    #region Override

    public override int GetHashCode() => getHashCode;

    public override bool Equals(object? obj) => Equals(obj as AssemblySlim);
    public bool Equals(AssemblySlim? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (getHashCode != other.getHashCode) return false;
        if (!StringComparer.Ordinal.Equals(NameFull, other.NameFull)) return false;

        return true;
    }

    public int CompareTo(object? obj) => CompareTo(obj as AssemblySlim);
    public int CompareTo(AssemblySlim? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        return Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(NameFull, other.NameFull);
    }

    public override string ToString() => NameFull;

    #endregion Override

    #region Static

    private static string NameFull_Build(Assembly assembly)
    {
        var fn = assembly.FullName.TrimOrNull();
        if (fn != null) return fn;

        var assemblyName = assembly.GetName();
        fn = assemblyName.FullName.TrimOrNull();
        if (fn != null) return fn;

        fn = assemblyName.Name.TrimOrNull();
        if (fn != null) return fn;

        fn = assemblyName.ToString().TrimOrNull();
        if (fn != null) return fn;

        fn = assembly.ToString().TrimOrNull();
        if (fn != null) return fn;

        throw new Exception($"Could not determine assembly name for assembly {assembly}");
    }

    public HashSet<AssemblySlim> GetReferencedAssemblies()
    {
        var hs = new HashSet<AssemblySlim>();
        var referencedAssemblyNames = Assembly.GetReferencedAssemblies();
        foreach (var referencedAssemblyName in referencedAssemblyNames.WhereNotNull())
        {
            try
            {
                // TODO: avoid Assembly.Load and perhaps switch to something else that doesn't full load, perhaps https://learn.microsoft.com/en-us/dotnet/standard/assembly/inspect-contents-using-metadataloadcontext
                var referencedAssembly = Assembly.Load(referencedAssemblyName);
                var referencedAssemblySlim = new AssemblySlim(referencedAssembly);
                hs.Add(referencedAssemblySlim);
            }
            catch (Exception e)
            {
                Constant.CreateLogger<AssemblySlim>().LogDebug(e, "Unable to load assembly: {ReferencedAssemblyName}", referencedAssemblyName);
            }
        }

        return hs;
    }

    public static HashSet<AssemblySlim> GetAssembliesVisible(bool recursive = false)
    {
        var assemblies = new List<Assembly?>();

        try { assemblies.Add(Assembly.GetEntryAssembly()); }
        catch (Exception) { }

        try { assemblies.Add(Assembly.GetCallingAssembly()); }
        catch (Exception) { }

        try { assemblies.Add(Assembly.GetExecutingAssembly()); }
        catch (Exception) { }

        try { assemblies.Add(MethodBase.GetCurrentMethod()!.DeclaringType?.Assembly); }
        catch (Exception) { }

        try { assemblies.Add(MethodBase.GetCurrentMethod()!.DeclaringType?.Assembly); }
        catch (Exception) { }

        try { assemblies.Add(typeof(AssemblySlim).Assembly); }
        catch (Exception) { }

        try
        {
            var stackTrace = new StackTrace(); // get call stack
            var stackFrames = stackTrace.GetFrames(); // get method calls (frames)
            foreach (var stackFrame in stackFrames)
            {
                try { assemblies.Add(stackFrame.GetMethod()?.GetType().Assembly); }
                catch (Exception) { }
            }
        }
        catch (Exception) { }

        var assembliesVisible = new HashSet<AssemblySlim>(assemblies.WhereNotNull().Select(o => new AssemblySlim(o)));


        if (recursive)
        {
            var assembliesScanned = new HashSet<AssemblySlim>();
            var queue = new Queue<AssemblySlim>(assembliesVisible);

            while (queue.Count > 0)
            {
                var asm = queue.Dequeue();
                if (!assembliesScanned.Add(asm)) continue;
                foreach (var item in asm.GetReferencedAssemblies().Where(item => !assembliesScanned.Contains(item)))
                {
                    queue.Enqueue(item);
                }
            }

            assembliesVisible.AddRange(assembliesScanned);
        }

        return assembliesVisible;
    }

    #endregion Static

    #region Implicit / Explicit

    public static implicit operator Assembly(AssemblySlim assemblySlim) => assemblySlim.Assembly;
    public static explicit operator AssemblySlim(Assembly assembly) => new(assembly);

    #endregion Implicit / Explicit
}
