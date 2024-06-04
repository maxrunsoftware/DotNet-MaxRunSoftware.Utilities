using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace MaxRunSoftware.Utilities.Web;

public class GenHTTPResourceBytes : IResource
{
    #region Get-/Setters
    
    public ImmutableArray<byte> Content { get; }
    private readonly byte[] content;
    
    public string? Name { get; }
    
    public DateTime? Modified { get; }
    
    public FlexibleContentType? ContentType { get; }
    
    public ulong? Length => (ulong)Content.Length;
    
    public ulong Checksum { get; }
    
    #endregion
    
    #region Initialization
    
    public GenHTTPResourceBytes(ReadOnlySpan<byte> content, string name, FlexibleContentType? contentType = null, DateTime? modified = null)
    {
        this.content = content.ToArray();
        Content = [..this.content];
        Checksum = CalcChecksum(this.content);
        Name = name;
        ContentType = contentType ?? FlexibleContentType.Get(Name.GuessContentType() ?? GenHTTP.Api.Protocol.ContentType.ApplicationForceDownload);
        Modified = modified;
        
        return;
        
        static ulong CalcChecksum(byte[] bytes)
        {
            if (bytes.Length == 0) return 0UL;
            using var stream = new MemoryStream(bytes, false);
            var result = CalcChecksumTask(stream).Result;
            if (result == null) throw new InvalidOperationException($"Unable to calculate checksum of byte[{bytes.Length}]");
            return result.Value;
            
            static async Task<ulong?> CalcChecksumTask(Stream stream) => await stream.CalculateChecksumAsync();
        }
        
        
    }
    
    #endregion
    
    #region Functionality
    
    public ValueTask<Stream> GetContentAsync() => new(new MemoryStream(content, false));
    
    public async ValueTask<ulong> CalculateChecksumAsync() => await new ValueTask<ulong>(Checksum);
    
    public ValueTask WriteAsync(Stream target, uint bufferSize) => target.WriteAsync(content.AsMemory());
    
    #endregion
    
}

public sealed class GenHTTPResourceBytesBuilder : IResourceBuilder<GenHTTPResourceBytesBuilder>
{
    private byte[]? _Content;
    private string? _Name;
    private FlexibleContentType? _ContentType;
    private DateTime? _Modified;
    
    #region Functionality
    
    public GenHTTPResourceBytesBuilder Content(ReadOnlySpan<byte> content)
    {
        _Content = content.ToArray();
        return this;
    }
    
    public GenHTTPResourceBytesBuilder Content(Span<byte> content)
    {
        _Content = content.ToArray();
        return this;
    }
    
    public GenHTTPResourceBytesBuilder Content(IEnumerable<byte> content)
    {
        _Content = content.ToArray();
        return this;
    }
    
    public GenHTTPResourceBytesBuilder Name(string name)
    {
        _Name = name;
        return this;
    }
    
    public GenHTTPResourceBytesBuilder Type(FlexibleContentType contentType)
    {
        _ContentType = contentType;
        return this;
    }
    
    public GenHTTPResourceBytesBuilder Modified(DateTime modified)
    {
        _Modified = modified;
        return this;
    }
    
    public IResource Build()
    {
        var content = _Content ?? throw new BuilderMissingPropertyException(nameof(_Content).TrimStart('_'));
        var name = _Name ?? throw new BuilderMissingPropertyException(nameof(_Name).TrimStart('_'));
        
        return new GenHTTPResourceBytes(content, name, _ContentType, _Modified);
    }
    
    #endregion
}
