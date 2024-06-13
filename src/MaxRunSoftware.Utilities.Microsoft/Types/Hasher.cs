using System.IO.Hashing;
using System.Security.Cryptography;

namespace MaxRunSoftware.Utilities.Common;

public interface IHasher
{
    public string Name { get; }
    public byte[] Hash(Stream stream);
    public Task<byte[]> HashAsync(Stream stream, CancellationToken cancellationToken = default);
}

public static class Hasher
{
    public static readonly string MD5 = "MD5";
    public static readonly string SHA1 = "SHA1";
    public static readonly string SHA256 = "SHA256";
    public static readonly string SHA384 = "SHA384";
    public static readonly string SHA512 = "SHA512";
    public static readonly string SHA3_256 = "SHA3_SHA256";
    public static readonly string SHA3_384 = "SHA3_SHA384";
    public static readonly string SHA3_512 = "SHA3_SHA512";
    
    public static readonly string Crc32 = "Crc32";
    public static readonly string Crc64 = "Crc64";
    public static readonly string XxHash3 = "XxHash3";
    public static readonly string XxHash32 = "XxHash32";
    public static readonly string XxHash64 = "XxHash64";
    public static readonly string XxHash128 = "XxHash128";
    
    public static FrozenDictionary<string, Func<IHasher>> FactoriesDefault { get; } = FactoriesDefault_Build();
    
    private static FrozenDictionary<string, Func<IHasher>> FactoriesDefault_Build()
    {
        var d = new Dictionary<string, Func<IHasher>>
        {
            [MD5] = () => new HasherCryptographic(MD5, System.Security.Cryptography.MD5.Create()),
            [SHA1] = () => new HasherCryptographic(SHA1, System.Security.Cryptography.SHA1.Create()),
            [SHA256] = () => new HasherCryptographic(SHA256, System.Security.Cryptography.SHA256.Create()),
            [SHA384] = () => new HasherCryptographic(SHA384, System.Security.Cryptography.SHA384.Create()),
            [SHA512] = () => new HasherCryptographic(SHA512, System.Security.Cryptography.SHA512.Create()),
        };
        
        if (System.Security.Cryptography.SHA3_256.IsSupported) d.Add(SHA3_256, () => new HasherCryptographic(SHA3_256, System.Security.Cryptography.SHA3_256.Create()));
        if (System.Security.Cryptography.SHA3_384.IsSupported) d.Add(SHA3_384, () => new HasherCryptographic(SHA3_384, System.Security.Cryptography.SHA3_384.Create()));
        if (System.Security.Cryptography.SHA3_512.IsSupported) d.Add(SHA3_512, () => new HasherCryptographic(SHA3_512, System.Security.Cryptography.SHA3_512.Create()));
        
        d.Add(Crc32, () => new HasherNonCryptographic(Crc32, new Crc32()));
        d.Add(Crc64, () => new HasherNonCryptographic(Crc64, new Crc64()));
        d.Add(XxHash3, () => new HasherNonCryptographic(XxHash3, new XxHash3()));
        d.Add(XxHash32, () => new HasherNonCryptographic(XxHash32, new XxHash32()));
        d.Add(XxHash64, () => new HasherNonCryptographic(XxHash64, new XxHash64()));
        d.Add(XxHash128, () => new HasherNonCryptographic(XxHash128, new XxHash128()));
        
        return d.ToFrozenDictionary();
    }
    
    public static IDictionary<string, Func<IHasher>> Factories { get; } = FactoriesDefault.ToDictionary();
    
    public sealed class HasherNonCryptographic(string name, NonCryptographicHashAlgorithm algorithm) : IHasher
    {
        public string Name { get; } = name;
        public byte[] Hash(Stream stream)
        {
            algorithm.Append(stream);
            return algorithm.GetHashAndReset();
        }
        
        public async Task<byte[]> HashAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            await algorithm.AppendAsync(stream, cancellationToken);
            return algorithm.GetHashAndReset();
        }
        
    }
    
    public sealed class HasherCryptographic(string name, HashAlgorithm algorithm) : IHasher, IDisposable
    {
        public string Name { get; } = name;
        public byte[] Hash(Stream stream) => algorithm.ComputeHash(stream);
        public async Task<byte[]> HashAsync(Stream stream, CancellationToken cancellationToken = default) => await algorithm.ComputeHashAsync(stream, cancellationToken);
        private readonly SingleUse isDisposed = new();
        
        public void Dispose()
        {
            if (!isDisposed.TryUse()) algorithm.Dispose();
        }
        
        ~HasherCryptographic() => Dispose();
    }
    
    public static byte[] Hash(string name, Stream stream)
    {
        var hasher = GetHasher(name);
        byte[] result;
        try
        {
            result = hasher.Hash(stream);
        }
        finally
        {
            if (hasher is IDisposable d) d.Dispose();
        }
        return result;
    }
    
    public static byte[] Hash(string name, byte[] data)
    {
        var hasher = GetHasher(name);
        byte[] result;
        try
        {
            result = hasher.Hash(data);
        }
        finally
        {
            if (hasher is IDisposable d) d.Dispose();
        }
        return result;
    }
    
    public static async Task<byte[]> HashAsync(string name, Stream stream, CancellationToken cancellationToken = default)
    {
        var hasher = GetHasher(name);
        byte[] result;
        try
        {
            result = await hasher.HashAsync(stream, cancellationToken);
        }
        finally
        {
            if (hasher is IDisposable d) d.Dispose();
        }
        return result;
    }
    
    public static async Task<byte[]> HashAsync(string name, byte[] data, CancellationToken cancellationToken = default)
    {
        var hasher = GetHasher(name);
        byte[] result;
        try
        {
            result = await hasher.HashAsync(data, cancellationToken);
        }
        finally
        {
            if (hasher is IDisposable d) d.Dispose();
        }
        return result;
    }
    
    public static IHasher GetHasher(string name)
    {
        if (!Factories.TryGetValue(name, out var hasher))
            throw new ArgumentException(
                $"No {nameof(IHasher)} named '{name}' in {nameof(Hasher)}.{nameof(Factories)}",
                nameof(name)
            );
        return hasher();
    }
}

public static class HasherExtensions
{
    public static byte[] Hash(this IHasher hasher, byte[] data)
    {
        var stream = new MemoryStream(data, false);
        stream.Position = 0;
        var result = hasher.Hash(stream);
        stream.Dispose();
        return result;
    }
    
    public static async Task<byte[]> HashAsync(this IHasher hasher, byte[] data, CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream(data, false);
        stream.Position = 0;
        var result = await hasher.HashAsync(stream, cancellationToken);
        await stream.DisposeAsync();
        return result;
    }

    public static byte[] Hash(this IHasher hasher, FileInfo info)
    {
        using var stream = Util.FileOpenRead(info.FullName);
        return hasher.Hash(stream);
    }
    
    public static async Task<byte[]> HashAsync(this IHasher hasher, FileInfo info, CancellationToken cancellationToken = default)
    {
        await using var stream = Util.FileOpenRead(info.FullName);
        return await hasher.HashAsync(stream, cancellationToken);
    }
}
