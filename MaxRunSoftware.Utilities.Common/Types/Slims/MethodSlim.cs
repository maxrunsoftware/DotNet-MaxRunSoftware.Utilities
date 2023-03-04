// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

[PublicAPI]
public sealed class MethodSlim : ComparableClass<MethodSlim, MethodSlim.Comparer>, ISlimValueGetterArgs
{
    public string Name { get; }

    public string NameFull => nameFull.Value;
    private readonly Lzy<string> nameFull;
    private string NameFullCreate()
    {
        var sb = new StringBuilder();
        if (TypeDeclaring != null) sb.Append(TypeDeclaring.NameFull.Trim());
        if (sb.Length > 0) sb.Append('.');
        sb.Append(Name);
        if (GenericArguments.Length > 0)
        {
            sb.Append('<');
            sb.Append(GenericArguments.Select(o => o.Name).ToStringDelimited(", "));
            sb.Append('>');
        }

        sb.Append('(');
        if (Parameters.Length > 0)
        {
            var sb2 = new StringBuilder();
            foreach (var p in Parameters)
            {
                if (sb2.Length > 0) sb2.Append(", ");
                if (p.Parameter.IsIn) sb2.Append("in ");
                if (p.Parameter.IsOut) sb2.Append("out ");
                if (p.Parameter.IsRef) sb2.Append("ref ");
                sb2.Append(p.Parameter.Type.Name);
                if (p.Parameter.Name != null) sb2.Append(" " + p.Parameter.Name);
            }

            sb.Append(sb2.ToString());
        }

        sb.Append(')');
        return sb.ToString();
    }

    public TypeSlim? TypeDeclaring { get; }
    public TypeSlim? ReturnType { get; }
    public MethodInfo Info { get; }
    public ImmutableArray<TypeSlim> GenericArguments { get; }

    private readonly ImmutableArray<ParameterInfo> parametersRaw;
    public ImmutableArray<MethodSlimParameter> Parameters => parameters.Value;
    private readonly Lzy<ImmutableArray<MethodSlimParameter>> parameters;
    private ImmutableArray<MethodSlimParameter> ParametersCreate() =>
        parametersRaw
            .OrderBy(o => o.Position)
            .Select(o => new ParameterSlim(o))
            .Select(o => new MethodSlimParameter(this, o))
            .ToImmutableArray();

    private readonly Lzy<MethodCaller> invoker;

    public bool IsStatic { get; }
    public bool IsOperatorImplicit { get; }
    public bool IsOperatorExplicit { get; }

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public MethodSlim(MethodInfo info) : base(Comparer.Instance)
    {
        Info = info.CheckNotNull(nameof(info));
        TypeDeclaring = info.DeclaringType == null ? null : (TypeSlim?)info.DeclaringType;
        ReturnType = info.ReturnType == typeof(void) ? null : (TypeSlim?)info.ReturnType;
        Name = info.Name;
        nameFull = Lzy.Create(NameFullCreate);
        IsStatic = info.IsStatic;
        GenericArguments = info.GetGenericArguments().Select(o => (TypeSlim)o).ToImmutableArray();
        parameters = Lzy.Create(ParametersCreate);
        invoker = Lzy.Create(() => new MethodCaller(Info));
        parametersRaw = Info.GetParameters().ToImmutableArray();
        IsOperatorImplicit = IsStatic && parametersRaw.Length == 1 && StringComparer.Ordinal.Equals(Name, "op_Implicit");
        IsOperatorExplicit = IsStatic && parametersRaw.Length == 1 && StringComparer.Ordinal.Equals(Name, "op_Explicit");
    }


    public override string ToString() => NameFull;
    // ReSharper disable RedundantOverriddenMember
    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    // ReSharper restore RedundantOverriddenMember

    public static bool operator ==(MethodSlim? left, MethodSlim? right) => Comparer.Instance.Equals(left, right);
    public static bool operator !=(MethodSlim? left, MethodSlim? right) => !Comparer.Instance.Equals(left, right);

    public static implicit operator MethodInfo(MethodSlim obj) => obj.Info;
    public static implicit operator MethodSlim(MethodInfo obj) => new(obj);

    public sealed class Comparer : ComparerBaseClass<MethodSlim>
    {
        public static Comparer Instance { get; } = new();
        protected override bool EqualsInternal(MethodSlim x, MethodSlim y) =>
            EqualsStruct(x.GetHashCode(), y.GetHashCode())
            && EqualsClass(x.TypeDeclaring, y.TypeDeclaring)
            && EqualsOrdinal(x.Name, y.Name)
            && EqualsClass(x.ReturnType, y.ReturnType)
            && EqualsClassEnumerable(x.GenericArguments, y.GenericArguments)
            && EqualsClassEnumerable(x.Parameters.Select(o => o.Parameter), y.Parameters.Select(o => o.Parameter));

        protected override int GetHashCodeInternal(MethodSlim obj) => Hash(
            obj.TypeDeclaring,
            obj.IsStatic,
            HashOrdinal(obj.Name),
            obj.ReturnType,
            Util.HashEnumerable(obj.GenericArguments),
            Util.HashEnumerable(obj.Parameters.Select(o => o.Parameter))
        );

        protected override int CompareInternal(MethodSlim x, MethodSlim y) =>
            CompareClass(x.TypeDeclaring, y.TypeDeclaring)
            ?? CompareOrdinalIgnoreCaseThenOrdinal(x.Name, y.Name)
            ?? CompareClass(x.ReturnType, y.ReturnType)
            ?? CompareClassEnumerable(x.GenericArguments, y.GenericArguments)
            ?? CompareClassEnumerable(x.Parameters.Select(o => o.Parameter), y.Parameters.Select(o => o.Parameter))
            ?? 0;
    }

    #region Extras

    public object? Invoke(object? instance, params object?[] args) => invoker.Value.Invoke(instance, args);

    public object? GetValue(object? instance) => GetValue(instance, Array.Empty<object?>());
    public object? GetValue(object? instance, object?[] args) => Invoke(instance, args);

    #endregion Extras
}
