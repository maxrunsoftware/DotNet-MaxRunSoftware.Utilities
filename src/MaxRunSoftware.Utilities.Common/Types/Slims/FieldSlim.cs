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

namespace MaxRunSoftware.Utilities.Common;

public sealed class FieldSlim : MemberSlim<FieldSlim, FieldInfo>, ISlimValueGetter, ISlimValueSetter
{
    // ReSharper disable once RedundantOverriddenMember
    public override bool Equals(object? other) => base.Equals(other);

    // ReSharper disable once RedundantOverriddenMember
    public override int GetHashCode() => base.GetHashCode();

    private readonly Lzy<TypeSlim> type;
    public TypeSlim Type => type.Value;
    
    public bool IsConstant => Info.IsLiteral;
    public bool IsReadonly => Info.IsInitOnly;
    public bool IsStatic => Info.IsStatic;
    private readonly Lzy<Func<object?, object?>> getMethodCompiled;
    private readonly Lzy<Action<object?, object?>> setMethodCompiled;
    
    private FieldSlim(FieldInfo info) : base(info)
    {
        type = Lzy.Create(() => (TypeSlim)Info.FieldType);
        getMethodCompiled = Lzy.Create(() => Info.CreateFieldGetter());
        setMethodCompiled = Lzy.Create(() => Info.CreateFieldSetter());
    }
    
    public static bool operator ==(FieldSlim? left, FieldSlim? right) => Equals(left, right);
    public static bool operator !=(FieldSlim? left, FieldSlim? right) => !Equals(left, right);
    
    public static implicit operator FieldInfo(FieldSlim obj) => obj.Info;
    public static implicit operator FieldSlim(FieldInfo obj) => new(obj);
    
    protected override bool Equals_Internal(FieldSlim other) => 
        Equals(DeclaringType, other.DeclaringType)
        && StringComparer.Ordinal.Equals(Name, other.Name)
        && Equals(Type, other.Type);

    protected override int GetHashCode_Internal()
    {
        var h = new HashCode();
        h.Add(DeclaringType);
        h.Add(Name, StringComparer.Ordinal);
        h.Add(Type);
        return h.ToHashCode();
    }

    protected override int CompareTo_Internal(FieldSlim other) =>
        Compare(DeclaringType, other.DeclaringType)
        ?? Compare_OrdinalIgnoreCase_Ordinal(Name, other.Name)
        ?? Compare(Type, other.Type)
        ?? 0;

    public object? GetValue(object? instance) => getMethodCompiled.Value(instance);

    public void SetValue(object? instance, object? value) => setMethodCompiled.Value(instance, value);
}


public static class FieldSlimExtensions
{
    public static FieldInfo ToFieldInfo(this FieldSlim obj) => obj;
    public static FieldSlim ToFieldSlim(this FieldInfo obj) => obj;
    
    public static ImmutableArray<FieldSlim> GetFieldSlims(this TypeSlim type, BindingFlags flags) =>
        type.Type.GetFieldSlims(flags);
    
    public static ImmutableArray<FieldSlim> GetFieldSlims(this Type type, BindingFlags flags) =>
        type.GetFields(flags).Select(o => o.ToFieldSlim()).ToImmutableArray();
    
    public static FieldSlim? GetFieldSlim(this TypeSlim type, string name, BindingFlags? flags = null) =>
        type.Type.GetFieldSlim(name, flags);
    
    public static FieldSlim? GetFieldSlim(this Type type, string name, BindingFlags? flags = null)
    {
        var flagsList = flags != null
            ? new[] { flags.Value }
            : new[]
            {
                BindingFlags.Public | BindingFlags.Instance,
                BindingFlags.Public | BindingFlags.Static,
                BindingFlags.NonPublic | BindingFlags.Instance,
                BindingFlags.NonPublic | BindingFlags.Static,
            };
        
        foreach (var f in flagsList)
        {
            var items = GetFieldSlims(type, f);
            foreach (var sc in Constant.StringComparers)
            {
                var matches = items.Where(prop => sc.Equals(prop.Name, name)).ToList();
                if (matches.Count == 1) return matches[0];
                if (matches.Count > 1) throw new AmbiguousMatchException($"Found {matches.Count} fields on {type.FullNameFormatted()} with name {name}: " + matches.Select(o => o.Name).ToStringDelimited(", "));
            }
        }
        
        return null;
    }
}
