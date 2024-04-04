// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

// ReSharper disable once InconsistentNaming
public static class ExtensionsIO
{
    public static string[] RemoveBase(this FileSystemInfo info, DirectoryInfo baseToRemove, bool caseSensitive = false)
    {
        var sourceParts = info.FullName.Split('/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Where(o => o.TrimOrNull() != null).ToArray();
        var baseParts = baseToRemove.FullName.Split('/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Where(o => o.TrimOrNull() != null).ToArray();

        var msgInvalidParent = $"{nameof(baseToRemove)} of {baseToRemove.FullName} is not a parent directory of {info.FullName}";

        if (baseParts.Length > sourceParts.Length) throw new ArgumentException(msgInvalidParent, nameof(baseToRemove));

        var list = new List<string>();
        for (var i = 0; i < sourceParts.Length; i++)
        {
            if (i >= baseParts.Length) { list.Add(sourceParts[i]); }
            else
            {
                if (caseSensitive)
                {
                    if (!string.Equals(sourceParts[i], baseParts[i])) throw new ArgumentException(msgInvalidParent, nameof(baseToRemove));
                }
                else
                {
                    if (!string.Equals(sourceParts[i], baseParts[i], StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(msgInvalidParent, nameof(baseToRemove));
                }
            }
        }

        return list.ToArray();
    }

    public static long GetLength(this FileInfo? file)
    {
        if (file == null) return -1;

        // https://stackoverflow.com/a/26473940
        if (file.IsSymbolic())
        {
            // https://stackoverflow.com/a/57454136
            using (Stream fs = Util.FileOpenRead(file.FullName)) { return fs.Length; }
        }

        return file.Length;
    }

    /// <summary>
    /// Checks whether a file is a Symbolic link.
    /// This is unreliable https://stackoverflow.com/a/26473940
    /// </summary>
    /// <param name="info">File to check</param>
    /// <returns>true if file is a symbolic link, otherwise false</returns>
    public static bool IsSymbolic(this FileSystemInfo? info)
    {
        if (info == null) return false;
        var attr = info.Attributes;
        return (attr & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
    }

    public static bool IsFile(this FileSystemInfo info)
    {
        // https://stackoverflow.com/a/1395212
        if (File.Exists(info.FullName)) return true;
        if (Directory.Exists(info.FullName)) return false;
        
        // https://stackoverflow.com/a/1395226
        var attr = info.Attributes;
        if ((attr & FileAttributes.Directory) == FileAttributes.Directory) return false;
        return false;
    }
    
    public static bool IsDirectory(this FileSystemInfo info)
    {
        // https://stackoverflow.com/a/1395212
        if (File.Exists(info.FullName)) return false;
        if (Directory.Exists(info.FullName)) return true;
        
        // https://stackoverflow.com/a/1395226
        var attr = info.Attributes;
        if ((attr & FileAttributes.Directory) == FileAttributes.Directory) return true;
        return false;
    }
}
