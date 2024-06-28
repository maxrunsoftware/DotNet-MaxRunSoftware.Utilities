using System.Buffers;
using System.Security.Cryptography;

namespace MaxRunSoftware.Utilities.Common;

public interface IHashBuilder : IDisposable
{
    public HashAlgorithmName Name { get; }
    public bool IsCryptographic { get; }
    public void Append(ReadOnlySpan<byte> data);
    public ReadOnlySpan<byte> Build();
    public void Reset();
}

public abstract class HashBuilderBase : IHashBuilder
{
    private readonly SingleUse isDisposed = new();

    public abstract HashAlgorithmName Name { get; }
    public abstract bool IsCryptographic { get; }

    public void Append(ReadOnlySpan<byte> data)
    {
        ThrowIfDisposed();
        Append_Internal(data);
    }
    
    protected abstract void Append_Internal(ReadOnlySpan<byte> data);

    public ReadOnlySpan<byte> Build()
    {
        ThrowIfDisposed();
        return Build_Internal();
    }
    
    protected abstract ReadOnlySpan<byte> Build_Internal();
    
    public void Reset()
    {
        ThrowIfDisposed();
        Reset_Internal();
    }
    
    protected abstract void Reset_Internal();

    public void Dispose()
    {
        if (isDisposed.TryUse())
        {
            Dispose_Internal();
        }
    }

    protected virtual void Dispose_Internal() { }

    protected virtual void ThrowIfDisposed() => isDisposed.ThrowIfUsed_ObjectDisposedException(this);
}

public static class HashBuilderExtensions
{
    public static HashBuilderCryptographic CreateHashBuilderCryptographic(this HashAlgorithmName name) => new(name);
    
    public static void Append(this IHashBuilder hashBuilder, byte[] data) => hashBuilder.Append((ReadOnlySpan<byte>)data);

    public static void Append(this IHashBuilder hashBuilder, byte[] data, int offset, int count)
    {
        var array = ArrayPool<byte>.Shared.Rent(count);
        try
        {
            ((ReadOnlySpan<byte>)data).Slice(offset, count).CopyTo((Span<byte>) array);
            hashBuilder.Append((ReadOnlySpan<byte>)data);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array, clearArray: hashBuilder.IsCryptographic);
        }
    }
}

public class HashAlgorithmNameComparer : ComparerBaseStruct<HashAlgorithmName>
{
    public static HashAlgorithmNameComparer Instance { get; } = new();
    
    private HashAlgorithmNameComparer() {}
    
    private static string StripCharacters(HashAlgorithmName hashAlgorithmName, bool caseSensitive)
    {
        var name = hashAlgorithmName.Name;
        if (name == null) return string.Empty;
        var len = name.Length;
        var sb = new StringBuilder(len);
        for (var i = 0; i < len; i++)
        {
            var c = name[i];
            if (char.IsAsciiLetterOrDigit(c))
            {
                if (!caseSensitive) c = char.ToUpper(c);
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
    
    
    protected override bool EqualsInternal(HashAlgorithmName x, HashAlgorithmName y) => StripCharacters(x, false).EqualsOrdinalIgnoreCase(StripCharacters(y, false));

    protected override int GetHashCodeInternal(HashAlgorithmName obj) => StripCharacters(obj, false).GetHashCode();

    protected override int CompareInternal(HashAlgorithmName x, HashAlgorithmName y) => StripCharacters(x, true).CompareToOrdinalIgnoreCaseThenOrdinal(StripCharacters(y, true));
}
