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
    
    private static readonly ImmutableArray<Type> EX_IOException_SecurityException = [typeof(IOException), typeof(SecurityException)];

    private static long GetLength_ByStream(this FileInfo file)
    {
        // https://stackoverflow.com/a/26473940
        // https://stackoverflow.com/a/57454136
        using Stream fs = Util.FileOpenRead(file.FullName);
        return fs.Length;
    }
    
    public static long? GetLength(this FileInfo? file)
    {
        if (file == null) return null;
        if (file.IsSymbolic_Safe() ?? false)
        {
            return GetLength_ByStream(file);
        }

        return file.Length;
    }
    
    public static long? GetLength_Safe(this FileInfo? file)
    {
        if (file == null) return null;
        if (file.IsSymbolic_Safe() ?? false)
        {
            var length = Util.CatchN(() => GetLength_ByStream(file));
            if (length != null) return length;
        }

        return Util.CatchN(() => file.Length, EX_IOException_SecurityException);
    }
    
    
    
    /// <summary>
    /// Checks whether a file is a Symbolic link.
    /// This is unreliable https://stackoverflow.com/a/26473940
    /// </summary>
    /// <param name="info">File to check</param>
    /// <returns>true if file is a symbolic link, false if is not a symbolic link or null if it could not be determined</returns>
    public static bool? IsSymbolic(this FileSystemInfo? info) => info?.Attributes.HasFlag(FileAttributes.ReparsePoint);

    /// <summary>
    /// Checks whether a file is a Symbolic link without throwing an exception.
    /// This is unreliable https://stackoverflow.com/a/26473940
    /// </summary>
    /// <param name="info">File to check</param>
    /// <returns>true if file is a symbolic link, false if is not a symbolic link or null if it could not be determined</returns>
    public static bool? IsSymbolic_Safe(this FileSystemInfo? info)
    {
        var attr = info?.Attributes_Safe();
        return attr?.HasFlag(FileAttributes.ReparsePoint);
    }

    public static string? FullName_Safe(this FileSystemInfo? info) => Util.Catch(() => info?.FullName, EX_IOException_SecurityException);

    public static FileAttributes? Attributes_Safe(this FileSystemInfo? info) => info == null ? null : Util.CatchN(() => info.Attributes);

    private static int IsDirectoryOrFile(FileSystemInfo? info)
    {
        // https://stackoverflow.com/a/1395212
        // https://stackoverflow.com/a/1395226

        const int unknown = 0, directory = 1, file = 2;
        
        if (info == null) return unknown;
        
        var name = info.FullName_Safe();
        if (name != null)
        {
            if (File.Exists(name)) return file;
            if (Directory.Exists(name)) return directory;
        }
        
        var attr = info.Attributes_Safe() ?? FileAttributes.None;
        if (attr.HasFlag(FileAttributes.Directory)) return directory;
        if (attr.HasFlag(FileAttributes.Normal)) return file;
        
        return unknown;
    }
    
    public static bool? IsFile(this FileSystemInfo info) => IsDirectoryOrFile(info) switch
    {
        0 => null,
        1 => false,
        2 => true,
        _ => throw new NotImplementedException()
    };

    public static bool? IsDirectory(this FileSystemInfo info) => IsDirectoryOrFile(info) switch
    {
        0 => null,
        1 => true,
        2 => false,
        _ => throw new NotImplementedException()
    };

    
    
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
