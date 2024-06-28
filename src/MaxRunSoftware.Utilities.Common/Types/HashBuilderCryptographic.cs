using System.Security.Cryptography;

namespace MaxRunSoftware.Utilities.Common;

public class HashBuilderCryptographic : HashBuilderBase
{
    public static HashAlgorithmName MD5 { get; } = HashAlgorithmName.MD5;
    public static HashBuilderCryptographic CreateHashBuilder_MD5() => new(MD5);

    public static HashAlgorithmName SHA1 { get; } = HashAlgorithmName.SHA1;
    public static HashBuilderCryptographic CreateHashBuilder_SHA1() => new(SHA1);

    public static HashAlgorithmName SHA256 { get; } = HashAlgorithmName.SHA256;
    public static HashBuilderCryptographic CreateHashBuilder_SHA256() => new(SHA256);

    public static HashAlgorithmName SHA384 { get; } = HashAlgorithmName.SHA384;
    public static HashBuilderCryptographic CreateHashBuilder_SHA384() => new(SHA384);

    public static HashAlgorithmName SHA512 { get; } = HashAlgorithmName.SHA512;
    public static HashBuilderCryptographic CreateHashBuilder_SHA512() => new(SHA512);

    public static HashAlgorithmName SHA3_256 { get; } = HashAlgorithmName.SHA3_256;
    public static HashBuilderCryptographic CreateHashBuilder_SHA3_256() => new(SHA3_256);

    public static HashAlgorithmName SHA3_384 { get; } = HashAlgorithmName.SHA3_384;
    public static HashBuilderCryptographic CreateHashBuilder_SHA3_384() => new(SHA3_384);

    public static HashAlgorithmName SHA3_512 { get; } = HashAlgorithmName.SHA3_512;
    public static HashBuilderCryptographic CreateHashBuilder_SHA3_512() => new(SHA3_512);

    private static FrozenDictionary<HashAlgorithmName, Func<IHashBuilder>> HashBuilders_Create()
    {
        var d = new Dictionary<HashAlgorithmName, Func<IHashBuilder>>(HashAlgorithmNameComparer.Instance);
        
        d.TryAdd(MD5, CreateHashBuilder_MD5);
        d.TryAdd(SHA1, CreateHashBuilder_SHA1);
        d.TryAdd(SHA256, CreateHashBuilder_SHA256);
        d.TryAdd(SHA384, CreateHashBuilder_SHA384);
        d.TryAdd(SHA512, CreateHashBuilder_SHA512);

        if (System.Security.Cryptography.SHA3_256.IsSupported) d.TryAdd(SHA3_256, CreateHashBuilder_SHA3_256);
        if (System.Security.Cryptography.SHA3_384.IsSupported) d.TryAdd(SHA3_384, CreateHashBuilder_SHA3_384);
        if (System.Security.Cryptography.SHA3_512.IsSupported) d.TryAdd(SHA3_512, CreateHashBuilder_SHA3_512);
        
        HMAC
        return d.ToFrozenDictionary(HashAlgorithmNameComparer.Instance);
    }
    private readonly IncrementalHash incrementalHash;

    #region Constructor

    public HashBuilderCryptographic(HashAlgorithmName name) : this(false, name, ReadOnlySpan<byte>.Empty)
    {
    }

    public HashBuilderCryptographic(HashAlgorithmName name, byte[] key) : this(true, name, key)
    {
    }

    public HashBuilderCryptographic(HashAlgorithmName name, ReadOnlySpan<byte> key) : this(true, name, key)
    {
    }

    private HashBuilderCryptographic(bool isKeyed, HashAlgorithmName name, ReadOnlySpan<byte> key)
    {
        Name = name;
        IsKeyed = isKeyed;
        incrementalHash = isKeyed ? IncrementalHash.CreateHMAC(name, key) : IncrementalHash.CreateHash(name);
    }

    #endregion Constructor

    public override HashAlgorithmName Name { get; }
    public override bool IsCryptographic { get; } = true;
    public bool IsKeyed { get; }

    protected override void Append_Internal(ReadOnlySpan<byte> data) => incrementalHash.AppendData(data);

    protected override ReadOnlySpan<byte> Build_Internal() => incrementalHash.GetCurrentHash();

    protected override void Reset_Internal() => incrementalHash.GetHashAndReset();

    protected override void Dispose_Internal() => incrementalHash.Dispose();
}
