// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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
