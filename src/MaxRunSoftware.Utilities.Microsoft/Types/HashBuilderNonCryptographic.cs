using System.IO.Hashing;
using System.Security.Cryptography;

namespace MaxRunSoftware.Utilities.Microsoft;

public class HashBuilderNonCryptographic : HashBuilderBase
{
    public static HashAlgorithmName Crc32 { get; } = new("Crc32");
    public static HashBuilderNonCryptographic CreateHashBuilder_Crc32() => new(Crc32);

    public static HashAlgorithmName Crc64 { get; } = new("Crc64");
    public static HashBuilderNonCryptographic CreateHashBuilder_Crc64() => new(Crc64);

    public static HashAlgorithmName XxHash3 { get; } = new("XxHash3");
    public static HashBuilderNonCryptographic CreateHashBuilder_XxHash3() => new(XxHash3);

    public static HashAlgorithmName XxHash32 { get; } = new("XxHash32");
    public static HashBuilderNonCryptographic CreateHashBuilder_XxHash32() => new(XxHash32);

    public static HashAlgorithmName XxHash64 { get; } = new("XxHash64");
    public static HashBuilderNonCryptographic CreateHashBuilder_XxHash64() => new(XxHash64);

    public static HashAlgorithmName XxHash128 { get; } = new("XxHash128");
    public static HashBuilderNonCryptographic CreateHashBuilder_XxHash128() => new(XxHash128);


    public static FrozenDictionary<HashAlgorithmName, Func<IHashBuilder>> HashBuilders { get; } = HashBuilders_Create();

    private static FrozenDictionary<HashAlgorithmName, Func<IHashBuilder>> HashBuilders_Create()
    {
        var d = new Dictionary<HashAlgorithmName, Func<IHashBuilder>>();
        
        d.TryAdd(Crc32, CreateHashBuilder_Crc32);
        d.TryAdd(Crc64, CreateHashBuilder_Crc64);
        d.TryAdd(XxHash3, CreateHashBuilder_XxHash3);
        d.TryAdd(XxHash32, CreateHashBuilder_XxHash32);
        d.TryAdd(XxHash64, CreateHashBuilder_XxHash64);
        d.TryAdd(XxHash128, CreateHashBuilder_XxHash128);

        return d.ToFrozenDictionary();
    }
    private readonly NonCryptographicHashAlgorithm algorithm;

    #region Constructor

    public HashBuilderNonCryptographic(HashAlgorithmName name)
    {
        Name = name;
        if ()
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
    public override bool IsCryptographic { get; } = false;

    protected override void Append_Internal(ReadOnlySpan<byte> data) => algorithm.Append(data);

    protected override ReadOnlySpan<byte> Build_Internal() => algorithm.GetCurrentHash();

    protected override void Reset_Internal() => algorithm.Reset();

    protected override void Dispose_Internal() => algorithm.Reset();
}
