namespace MaxRunSoftware.Utilities.Common;

public sealed class TempDirectory : IDisposable
{
    private static readonly object locker = new();
    private readonly ILogger log;
    public string Path { get; }
    
    private static string GetTemp(string? path)
    {
        lock (locker)
        {
            path ??= System.IO.Path.GetTempPath();
            string p;
            do
            {
                p = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, System.IO.Path.GetRandomFileName()));
            } while (Directory.Exists(p));
            
            return p;
        }
    }
    
    public TempDirectory(string? path = null, ILogger? log = null)
    {
        this.log = log ?? Constant.LoggerNull;
        path = GetTemp(path);
        this.log.LogDebug("Creating temporary directory {Path}", path);
        Directory.CreateDirectory(path);
        Path = path;
    }
    
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
            {
                log.LogDebug("Deleting temporary directory {Path}", Path);
                Directory.Delete(Path, true);
                log.LogDebug("Successfully deleted temporary directory {Path}", Path);
            }
        }
        catch (Exception e) { log.LogWarning(e, "Error deleting temporary directory {Path}", Path); }
    }
}
