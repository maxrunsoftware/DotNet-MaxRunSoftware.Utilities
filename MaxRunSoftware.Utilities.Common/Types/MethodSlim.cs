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

namespace MaxRunSoftware.Utilities.Common;


[PublicAPI]
public sealed class MethodSlimParameter
{
    public MethodSlim Method { get; }
    public ParameterSlim Parameter { get; }
    public MethodSlimParameter(MethodSlim method, ParameterSlim parameter)
    {
        Method = method;
        Parameter = parameter;
    }
}



[PublicAPI]
public sealed class MethodSlim : IEquatable<MethodSlim>, IComparable<MethodSlim>, IComparable, IEquatable<MethodInfo>, IComparable<MethodInfo>
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

    public override int GetHashCode() => getHashCode.Value;
    private readonly Lzy<int> getHashCode;
    private int GetHashCodeCreate() => Util.Hash(
        TypeDeclaring,
        IsStatic,
        Name,
        Util.HashEnumerable(GenericArguments),
        Util.HashEnumerable(Parameters.Select(o => o.Parameter))
    );

    public bool IsStatic { get; }
    public bool IsOperatorImplicit { get; }
    public bool IsOperatorExplicit { get; }

    public IEnumerable<Attribute> Attributes => Info.GetCustomAttributes();

    public MethodSlim(MethodInfo info)
    {
        Info = info.CheckNotNull(nameof(info));
        TypeDeclaring = info.DeclaringType == null ? null : (TypeSlim?)info.DeclaringType;
        ReturnType = info.ReturnType == typeof(void) ? null : (TypeSlim?)info.ReturnType;
        Name = info.Name;
        nameFull = Lzy.Create(NameFullCreate);
        IsStatic = info.IsStatic;
        GenericArguments = info.GetGenericArguments().Select(o => (TypeSlim)o).ToImmutableArray();
        parameters = Lzy.Create(ParametersCreate);
        getHashCode = Lzy.Create(GetHashCodeCreate);
        invoker = Lzy.Create(() => new MethodCaller(Info));
        parametersRaw = Info.GetParameters().ToImmutableArray();
        IsOperatorImplicit = IsStatic && parametersRaw.Length == 1 && StringComparer.Ordinal.Equals(Name, "op_Implicit");
        IsOperatorExplicit = IsStatic && parametersRaw.Length == 1 && StringComparer.Ordinal.Equals(Name, "op_Explicit");
    }

    #region Override


    public override string ToString() => NameFull;

    #region Equals

    public static bool Equals(MethodSlim? left, MethodSlim? right) => left?.Equals(right) ?? ReferenceEquals(right, null);

    public override bool Equals(object? obj) => obj switch
    {
        null => false,
        MethodSlim slim => Equals(slim),
        MethodInfo other => Equals(other),
        _ => false
    };

    public bool Equals(MethodInfo? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(Info, other)) return true;
        return Equals(new MethodSlim(other));
    }

    public bool Equals(MethodSlim? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (GetHashCode() != other.GetHashCode()) return false;

        if (!Util.IsEqual(TypeDeclaring, other.TypeDeclaring)) return false;
        if (IsStatic != other.IsStatic) return false;
        if (!StringComparer.Ordinal.Equals(Name, other.Name)) return false;

        if (GenericArguments.Length != other.GenericArguments.Length) return false;
        for (var i = 0; i < GenericArguments.Length; i++)
        {
            if (!GenericArguments[i].Equals(other.GenericArguments[i])) return false;
        }

        if (Parameters.Length != other.Parameters.Length) return false;
        for (var i = 0; i < Parameters.Length; i++)
        {
            if (!Parameters[i].Parameter.Equals(other.Parameters[i].Parameter)) return false;
            if (!Parameters[i].Method.GetHashCode().Equals(other.Parameters[i].Method.GetHashCode())) return false; // Probably should do more checks
        }

        return true;
    }

    #endregion Equals

    #region CompareTo

    public int CompareTo(object? obj) => obj switch
    {
        null => 1,
        MethodSlim slim => CompareTo(slim),
        MethodInfo other => CompareTo(other),
        _ => 1
    };

    public int CompareTo(MethodInfo? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(Info, other)) return 0;
        return CompareTo(new MethodSlim(other));
    }

    public int CompareTo(MethodSlim? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;

        if (TypeDeclaring == null)
        {
            if (other.TypeDeclaring != null) return 1;
        }
        else
        {
            if (0 != (c = TypeDeclaring.CompareTo(other.TypeDeclaring))) return c;
        }

        if (0 != (c = IsStatic.CompareTo(other.IsStatic))) return c;

        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;

        if (0 != (c = GenericArguments.Length.CompareTo(other.GenericArguments.Length))) return c;
        for (var i = 0; i < GenericArguments.Length; i++)
        {
            if (0 != (c = GenericArguments[i].CompareTo(other.GenericArguments[i]))) return c;
        }

        if (0 != (c = Parameters.Length.CompareTo(other.Parameters.Length))) return c;
        for (var i = 0; i < Parameters.Length; i++)
        {
            if (0 != (c = Parameters[i].Parameter.CompareTo(other.Parameters[i].Parameter))) return c;
            if (0 != (c = Parameters[i].Method.GetHashCode().CompareTo(other.Parameters[i].Method.GetHashCode()))) return c; // Probably should do more checks
        }

        if (0 != (c = GetHashCode().CompareTo(other.GetHashCode()))) return c;

        return c;
    }



    #endregion CompareTo

    #endregion Override

    #region Implicit / Explicit


    // ReSharper disable ArrangeStaticMemberQualifier

    public static bool operator ==(MethodSlim? left, MethodSlim? right) => MethodSlim.Equals(left, right);
    public static bool operator !=(MethodSlim? left, MethodSlim? right) => !MethodSlim.Equals(left, right);

    // ReSharper restore ArrangeStaticMemberQualifier

    public static implicit operator MethodInfo(MethodSlim obj) => obj.Info;
    public static implicit operator MethodSlim(MethodInfo obj) => new(obj);

    #endregion Implicit / Explicit

    #region Extras

    public object? Invoke(object instance, params object?[] args)
    {
        return invoker.Value.Invoke(instance, args);
    }

    public object? InvokeStatic(params object?[] args)
    {
        return invoker.Value.Invoke(null, args);
    }

    #endregion Extras
}

public static class MethodSlimExtensions
{
    public static MethodSlim[] GetMethodSlims(this TypeSlim type, BindingFlags flags) =>
        type.Type.GetMethodSlims(flags);

    public static MethodSlim[] GetMethodSlims(this Type type, BindingFlags flags) =>
        type.GetMethods(flags).Select(o => new MethodSlim(o)).ToArray();
}
