using System.Collections.Concurrent;

namespace MaxRunSoftware.Utilities.Common;

public abstract class MemberSlimBase
{
    #region Helpers
    
    protected static int? Compare<TComparable>(TComparable? x, TComparable? y) where TComparable : class, IComparable<TComparable>
    {
        if (ReferenceEquals(x, y)) return null;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        var c = x.CompareTo(y);
        if (c != 0) return c;
        return null;
    }

    protected static int? Compare_OrdinalIgnoreCase_Ordinal(string? x, string? y)
    {
        if (x == y) return null;
        if (x == null) return -1;
        if (y == null) return 1;
        var c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(x, y);
        if (c != 0) return c;
        return null;
    }

    #endregion Helpers
}

public abstract class MemberSlim : MemberSlimBase, IComparable
{
    protected MemberSlim(MemberInfo info)
    {
        Info = info;
        getHashCode = Lzy.Create(GetHashCode_Internal);
        toString = Lzy.Create(ToString_Internal);

        declaringType = Lzy.Create(DeclaringType_Internal);
        
        attributes = Lzy.Create(Attributes_Internal);
        attributesInherited = Lzy.Create(AttributesInherited_Internal);
        
        metadataToken = Lzy.Create(MetadataToken_Internal);
        module = Lzy.Create(Module_Internal);
    }

    public string Name => Info.Name;
    
    public MemberInfo Info { get; }

    #region Attribute
    
    private readonly Lzy<ImmutableArray<Attribute>> attributes;
    public ImmutableArray<Attribute> Attributes => attributes.Value;
    protected virtual ImmutableArray<Attribute> Attributes_Internal() => Attribute.GetCustomAttributes(Info, false).ToImmutableArray();

    private readonly Lzy<ImmutableArray<Attribute>> attributesInherited;
    public ImmutableArray<Attribute> AttributesInherited => attributesInherited.Value;
    protected virtual ImmutableArray<Attribute> AttributesInherited_Internal() => Attribute.GetCustomAttributes(Info, true).ToImmutableArray();

    #endregion Attribute
    
    #region DeclaringType

    private readonly Lzy<TypeSlim?> declaringType;

    public TypeSlim? DeclaringType => declaringType.Value;

    protected virtual TypeSlim? DeclaringType_Internal() => Info.DeclaringType?.ToTypeSlim();
    
    #endregion DeclaringType
    
    #region MetadataToken

    private readonly Lzy<int?> metadataToken;

    public int? MetadataToken => metadataToken.Value;

    protected virtual int? MetadataToken_Internal()
    {
        try
        {
            // https://learn.microsoft.com/en-us/dotnet/api/System.Reflection.MemberInfo.MetadataToken?view=net-8.0
            return Info.MetadataToken;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    #endregion MetadataToken

    #region Module

    private readonly Lzy<Module?> module;

    public Module? Module => module.Value;

    protected virtual Module? Module_Internal()
    {
        try
        {
            // https://learn.microsoft.com/en-us/dotnet/api/System.Reflection.MemberInfo.Module?view=net-8.0
            return Info.Module;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }

    #endregion Module

    #region Equals

    public override bool Equals(object? other) => throw new NotImplementedException();

    #endregion Equals
    
    #region CompareTo

    public virtual int CompareTo(object? obj) => throw new NotImplementedException();

    #endregion CompareTo
    
    #region GetHashCode
    
    private readonly Lzy<int> getHashCode;

    public override int GetHashCode() => getHashCode.Value;

    protected abstract int GetHashCode_Internal();
    
    #endregion GetHashCode

    #region GetHashCode
    
    private readonly Lzy<string> toString;

    public override string ToString() => toString.Value;

    protected virtual string ToString_Internal()
    {
        var s = DeclaringType?.ToString().TrimOrNull();
        if (s != null) return s + "." + Name;
        return Name;
    }

    #endregion GetHashCode
}

public abstract class MemberSlim<T>(MemberInfo info) : MemberSlim(info), IEquatable<T>, IComparable<T>
    where T : MemberSlim<T>
{
    public override int CompareTo(object? other) => CompareTo(other as T);
    
    public int CompareTo(T? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(other, null)) return 1;
        return CompareTo_Internal(other);
    }

    protected abstract int CompareTo_Internal(T other);

    public override bool Equals(object? other) => Equals(other as T);

    protected virtual bool Equals_IncludeHashcode => true;
    
    public bool Equals(T? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(Info, other.Info)) return true;
        
        if (Equals_IncludeHashcode && GetHashCode() != other.GetHashCode()) return false;
        
        try
        {
            // https://github.com/dotnet/runtime/issues/16288
            Info.HasSameMetadataDefinitionAs(other.Info);
        }
        catch (NotImplementedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (Exception e)
        {
            if (e is not NotImplementedException && e is not InvalidOperationException) throw;
        }

        var xt = MetadataToken;
        var yt = other.MetadataToken;

        if (xt != null && yt != null)
        {
            if (xt.Value != yt.Value) return false;
        }
        else if (xt != null && yt == null) return false;
        else if (xt == null && yt != null) return false;

        var xm = Module;
        var ym = other.Module;

        if (xm != null && ym != null)
        {
            return xm.Equals(ym);
        }
        else if (xm != null && ym == null) return false;
        else if (xm == null && ym != null) return false;
        
        return Equals_Internal(other);
    }

    protected virtual bool Equals_Internal(T other) => throw new NotImplementedException(string.Format(
        "Could not check equality between [{0}]{1} and [{2}]{3}",
        GetType().FullNameFormatted(),
        ToString(),
        other.GetType().FullNameFormatted(),
        other
        ));
    
    public override int GetHashCode() => base.GetHashCode();
}

public abstract class MemberSlim<T, TInfo>(TInfo info) : MemberSlim<T>(info)
    where T : MemberSlim<T, TInfo>
    where TInfo : MemberInfo
{
    public new TInfo Info { get; } = info;
    
}
