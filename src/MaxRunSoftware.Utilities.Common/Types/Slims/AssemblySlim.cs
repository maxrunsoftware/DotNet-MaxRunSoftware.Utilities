// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common;

[PublicAPI]
public sealed class AssemblySlim : ComparableClass<AssemblySlim, AssemblySlim.Comparer>
{
    public Assembly Assembly { get; }
    public string NameFull { get; }

    public AssemblySlim(Assembly assembly) : base(Comparer.Instance)
    {
        static string NameFullBuild(Assembly assembly)
        {
            var fn = assembly.FullName.TrimOrNull();
            if (fn != null) return fn;

            var assemblyName = assembly.GetName();
            fn = assemblyName.FullName.TrimOrNull()
                 ?? assemblyName.Name.TrimOrNull()
                 ?? assemblyName.ToString().TrimOrNull()
                 ?? assembly.ToString().TrimOrNull();
            if (fn != null) return fn;

            throw new($"Could not determine assembly name for assembly {assembly}");
        }

        Assembly = assembly.CheckNotNull(nameof(assembly));
        NameFull = NameFullBuild(assembly);
    }

    public override string ToString() => NameFull;
    // ReSharper disable RedundantOverriddenMember
    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    // ReSharper restore RedundantOverriddenMember

    public static bool operator ==(AssemblySlim? left, AssemblySlim? right) => Comparer.Instance.Equals(left, right);
    public static bool operator !=(AssemblySlim? left, AssemblySlim? right) => !Comparer.Instance.Equals(left, right);

    public static implicit operator Assembly(AssemblySlim assemblySlim) => assemblySlim.Assembly;
    public static implicit operator AssemblySlim(Assembly assembly) => new(assembly);

    public sealed class Comparer : ComparerBaseClass<AssemblySlim>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(AssemblySlim x, AssemblySlim y) =>
            EqualsStruct(x.GetHashCode(), y.GetHashCode())
            && EqualsOrdinal(x.NameFull, y.NameFull);

        protected override int GetHashCodeInternal(AssemblySlim obj) => Hash(
            HashOrdinal(obj.NameFull)
        );

        protected override int CompareInternal(AssemblySlim x, AssemblySlim y) =>
            CompareOrdinalIgnoreCaseThenOrdinal(x.NameFull, y.NameFull)
            ?? 0;
    }

    #region Static

    public HashSet<AssemblySlim> GetReferencedAssemblies() => GetReferencedAssemblies(out _);

    public HashSet<AssemblySlim> GetReferencedAssemblies(out (AssemblyName ReferencedAssemblyName, Exception Exception)[] exceptions)
    {
        var hs = new HashSet<AssemblySlim>();
        var eList = new List<(AssemblyName ReferencedAssemblyName, Exception Exception)>();
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
                eList.Add((referencedAssemblyName, e));
                //log.LogDebug(e, "Unable to load assembly: {ReferencedAssemblyName}", referencedAssemblyName);
            }
        }

        exceptions = eList.ToArray();
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
                foreach (var item in asm.GetReferencedAssemblies())
                {
                    if (assembliesScanned.Contains(item)) continue;
                    queue.Enqueue(item);
                }
            }

            assembliesVisible.AddRange(assembliesScanned);
        }

        return assembliesVisible;
    }

    #endregion Static
}
