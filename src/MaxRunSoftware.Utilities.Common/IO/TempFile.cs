namespace MaxRunSoftware.Utilities.Common;

public sealed class TempFile : IDisposable
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
            } while (File.Exists(p));
            
            return p;
        }
    }
    
    public TempFile(string? path = null, bool createEmptyFile = false, ILogger? log = null)
    {
        this.log = log ?? Constant.LoggerNull;
        path = GetTemp(path);
        this.log.LogDebug("Creating temporary file {Path}", path);
        if (createEmptyFile) File.WriteAllBytes(path, []);
        Path = path;
    }
    
    public void Dispose()
    {
        try
        {
            if (File.Exists(Path))
            {
                log.LogDebug("Deleting temporary file {Path}", Path);
                File.Delete(Path);
                log.LogDebug("Successfully deleted temporary file {Path}", Path);
            }
        }
        catch (Exception e) { log.LogWarning(e, "Error deleting temporary file {Path}", Path); }
    }
}
