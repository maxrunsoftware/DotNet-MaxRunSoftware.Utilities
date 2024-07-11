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

using System.Security;

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
    
    public static long GetLength(this FileInfo? file, bool suppressException = false, long defaultLength = -1)
    {
        if (file == null) return defaultLength;
        // https://stackoverflow.com/a/26473940
        if (file.IsSymbolic(suppressException: suppressException))
        {
            // https://stackoverflow.com/a/57454136
            if (!suppressException) return GetLengthByStream(file);

            try
            {
                return GetLengthByStream(file);
            }
            catch (SecurityException) { }
            catch (IOException) { }

            return defaultLength;

        }

        if (!suppressException) return file.Length;
        try
        {
            return file.Length;
        }
        catch (SecurityException) { }
        catch (IOException) { }
        
        return defaultLength;
        
        static long GetLengthByStream(FileInfo info)
        {
            using Stream fs = Util.FileOpenRead(info.FullName);
            return fs.Length;
        }
    }

    /// <summary>
    /// Checks whether a file is a Symbolic link.
    /// This is unreliable https://stackoverflow.com/a/26473940
    /// </summary>
    /// <param name="info">File to check</param>
    /// <param name="suppressException">If true then no exception is thrown and instead just false is returned</param>
    /// <returns>true if file is a symbolic link, otherwise false</returns>
    public static bool IsSymbolic(this FileSystemInfo? info, bool suppressException = false)
    {
        if (info == null) return false;
        FileAttributes? attr = null;
        if (suppressException)
        {
            try
            {
                attr = info.Attributes;
            }
            catch (SecurityException)
            {
            }
            catch (IOException)
            {
            }
        }
        else
        {
            attr = info.Attributes;
        }

        if (attr == null) return false;

        return (attr.Value & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
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
    
    public record FileSystemInfosTyped(
        List<DirectoryInfo> Directories,
        List<FileInfo> Files
    );
    
    public static FileSystemInfosTyped GetFileSystemInfosTyped(this DirectoryInfo info)
    {
        var fsis = info.GetFileSystemInfos();
        var directories = new List<DirectoryInfo>(fsis.Length);
        var files = new List<FileInfo>(fsis.Length);
        
        foreach (var fsi in fsis)
        {
            switch (fsi)
            {
                case FileInfo fi:
                    files.Add(fi);
                    break;
                case DirectoryInfo di:
                    directories.Add(di);
                    break;
                default:
                    throw new NotImplementedException(fsi.GetType().FullNameFormatted());
            }
        }
        
        return new (directories, files);
    }
    
    private static FileSystemEntry GetFileSystemEntry(DirectoryInfo directory)
    {
        List<DirectoryInfo>? directories = null;
        List<FileInfo>? files = null;
        Exception? exception = null;
        try
        {
            var fsis = directory.GetFileSystemInfos();
            foreach (var fsi in fsis)
            {
                switch (fsi)
                {
                    case FileInfo fi:
                        files ??= new(fsis.Length);
                        files.Add(fi);
                        break;
                    case DirectoryInfo di:
                        directories ??= new(fsis.Length);
                        directories.Add(di);
                        break;
                    default:
                        throw new NotImplementedException(fsi.GetType().FullNameFormatted());
                }
            }
        }
        catch (Exception e)
        {
            exception = e;
        }
        
        return new(
            directory,
            directories.ToArrayOrEmpty(),
            files.ToArrayOrEmpty(),
            exception
        );
    }
    
    public static IEnumerable<FileSystemEntry> GetFileSystemEntries(this DirectoryInfo info, bool recursive)
    {
        var stack = new Stack<DirectoryInfo>();
        stack.Push(info);
        
        while (stack.TryPop(out var directory))
        {
            var item = GetFileSystemEntry(directory);
            if (recursive)
            {
                foreach (var subdir in item.Directories.Reverse())
                {
                    stack.Push(subdir);
                }
            }
            yield return item;
        }
    }
    
    
    
    
    
    
}

public readonly record struct FileSystemEntry(
    DirectoryInfo Directory,
    DirectoryInfo[] Directories,
    FileInfo[] Files,
    Exception? Exception
);
