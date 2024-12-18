﻿// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

// ReSharper disable InconsistentNaming
public static partial class Util
{
    #region File

    internal static DirectoryNotFoundException CreateExceptionDirectoryNotFound(string directoryPath) => new("Directory does not exist " + directoryPath);

    internal static FileNotFoundException CreateExceptionFileNotFound(string filePath) => new("File does not exist " + filePath, filePath);

    internal static DirectoryNotFoundException CreateExceptionIsNotDirectory(string directoryPath) => new("Path is a file not a directory " + directoryPath);

    public static (string directoryName, string fileName, string extension) SplitFileName(string file)
    {
        file = Path.GetFullPath(file);
        var dn = Path.GetDirectoryName(file);
        var d = dn == null ? null : Path.GetFullPath(dn);
        var f = Path.GetFileNameWithoutExtension(file);
        var e = Path.GetExtension(file).TrimOrNull();
        if (!string.IsNullOrEmpty(e) && e[0] == '.') e = e.Remove(0, 1).TrimOrNull();

        return (d.TrimOrNull() ?? string.Empty, f, e.TrimOrNull() ?? string.Empty);
    }

    public static long FileGetLength(string file) => new FileInfo(file).Length;

    public static string FileChangeName(string file, string newFileName)
    {
        file = Path.GetFullPath(file);
        var dir = Path.GetDirectoryName(file);
        var name = newFileName;
        var ext = string.Empty;
        if (Path.HasExtension(file)) ext = Path.GetExtension(file);

        if (!ext.StartsWith(".")) ext = "." + ext;

        var f = dir == null ? Path.Combine(name + ext) : Path.Combine(dir, name + ext);
        f = Path.GetFullPath(f);
        return f;
    }

    public static string FileChangeNameRandomized(string file) => FileChangeNameAppendRight(file, "_" + Guid.NewGuid().ToString().Replace("-", ""));

    public static string FileChangeNameAppendRight(string file, string stringToAppend) => FileChangeName(file, Path.GetFileNameWithoutExtension(Path.GetFullPath(file)) + stringToAppend);

    public static string FileChangeNameAppendLeft(string file, string stringToAppend) => FileChangeName(file, stringToAppend + Path.GetFileNameWithoutExtension(Path.GetFullPath(file)));

    public static string FileChangeExtension(string file, string? newExtension)
    {
        file = Path.GetFullPath(file);
        var dir = Path.GetDirectoryName(file);
        var name = Path.GetFileNameWithoutExtension(file);
        var ext = newExtension ?? string.Empty;
        if (!ext.StartsWith(".")) ext = "." + ext;

        var f = dir == null ? Path.Combine(name + ext) : Path.Combine(dir, name + ext);
        f = Path.GetFullPath(f);
        return f;
    }

    public static bool IsDirectory(string path)
    {
        path = path.CheckNotNullTrimmed(path);
        if (File.Exists(path)) return false;

        if (Directory.Exists(path)) return true;

        return false;
    }

    public static bool IsEqualFile(string file1, string file2, bool buffered)
    {
        file1.CheckFileExists("file1");
        file2.CheckFileExists("file2");

        // TODO: Check if file system is case sensitive or not then do a case-insensitive comparison since Windows uses case-insensitive filesystem.
        if (string.Equals(file1, file2)) return true;

        var file1Size = FileGetLength(file1);
        var file2Size = FileGetLength(file2);

        if (file1Size != file2Size) return false;

        if (buffered)
        {
            var file1Bytes = FileRead(file1);
            var file2Bytes = FileRead(file2);

            return file1Bytes.IsEqual(file2Bytes);
        }
        // No buffer needed http://stackoverflow.com/a/2069317 http://blogs.msdn.com/b/brada/archive/2004/04/15/114329.aspx

        // Compare method from http://stackoverflow.com/a/1359947
        const int bytesToRead = sizeof(long);

        var iterations = (int)Math.Ceiling((double)file1Size / bytesToRead);

        using (var fs1 = FileOpenRead(file1))
        using (var fs2 = FileOpenRead(file2))
        {
            var one = new byte[bytesToRead];
            var two = new byte[bytesToRead];

            for (var i = 0; i < iterations; i++)
            {
                // ReSharper disable once MustUseReturnValue
                fs1.Read(one, 0, bytesToRead);

                // ReSharper disable once MustUseReturnValue
                fs2.Read(two, 0, bytesToRead);

                if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0)) return false;
            }
        }

        return true;
    }

    public static bool IsFile(string path)
    {
        path = path.CheckNotNullTrimmed(path);
        if (Directory.Exists(path)) return false;

        if (File.Exists(path)) return true;

        return false;
    }

    public static bool IsFileSymbolic(string path)
    {
        path = path.CheckNotNullTrimmed(path);
        if (!IsFile(path)) return false;
        var pathInfo = new FileInfo(path);

        // TODO: Unreliable  https://stackoverflow.com/a/26473940
        return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    /// <summary>
    /// Returns a list of files and directories including the provided directory
    /// </summary>
    /// <param name="directoryPath">The directory path to list</param>
    /// <param name="recursive">Whether to be recursive or not</param>
    /// <returns>An IEnumerable of FileListResult</returns>
    public static IEnumerable<FileSystemObject> FileList(string directoryPath, bool recursive = false)
    {
        var d = FileSystemObject.GetDirectory(directoryPath);
        return recursive ? d.ObjectsRecursive : d.Objects;
    }

    public static IEnumerable<string> FileListFiles(string directoryPath, bool recursive = false)
    {
        foreach (var o in FileList(directoryPath, recursive))
        {
            if (!o.IsDirectory) yield return o.Path;
        }
    }

    public static IEnumerable<string> FileListDirectories(string directoryPath, bool recursive = false)
    {
        foreach (var o in FileList(directoryPath, recursive))
        {
            if (o.IsDirectory) yield return o.Path;
        }
    }

    public static bool FileIsAbsolutePath(string path)
    {
        if (path.TrimOrNull() == null) return false;

        if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1) return false;

        if (!Path.IsPathRooted(path)) return false;

        var pathRoot = Path.GetPathRoot(path);
        if (pathRoot != null && pathRoot.Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)) return false;

        return true;
    }

    public static string? FileGetParent(string path)
    {
        path = Path.GetFullPath(path);
        string reassemblyChar;
        if (path.Contains("\\"))
            // windows path
        {
            reassemblyChar = "\\";
        }
        else if (path.Contains("/"))
            // linux / mac path
        {
            reassemblyChar = "/";
        }
        else { reassemblyChar = Constant.OS_Windows ? "\\" : "/"; }

        var pathParts = path.Split(reassemblyChar).TrimOrNull().WhereNotNull().ToList();
        if (pathParts.Count == 1) return null;

        pathParts.PopTail();
        path = pathParts.ToStringDelimited(reassemblyChar);
        if (reassemblyChar.Equals("/")) path = "/" + path;

        return Path.GetFullPath(path);
    }

    public static FileStream FileOpenRead(string filename) => File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

    public static FileStream FileOpenWrite(string filename) => File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);

    public static byte[] FileRead(string path)
    {
        // https://github.com/mosa/Mono-Class-Libraries/blob/master/mcs/class/corlib/System.IO/File.cs

        using (var stream = FileOpenRead(path))
        {
            var size = stream.Length;
            // limited to 2GB according to http://msdn.microsoft.com/en-us/library/system.io.file.readallbytes.aspx
            if (size > int.MaxValue) throw new IOException("Reading more than 2GB with this call is not supported");

            var pos = 0;
            var count = (int)size;
            var result = new byte[size];
            while (count > 0)
            {
                var n = stream.Read(result, pos, count);
                if (n == 0) throw new IOException("Unexpected end of stream");

                pos += n;
                count -= n;
            }

            return result;
        }
    }

    public static string FileRead(string path, Encoding encoding) => File.ReadAllText(path, encoding);

    public static void FileWrite(string path, byte[] data, bool append = false)
    {
        path.CheckNotNull(nameof(path));
        if (File.Exists(path) && !append) File.Delete(path);

        using (var stream = File.OpenWrite(path))
        {
            stream.Position = stream.Length;
            stream.Write(data, 0, data.Length);
            stream.Flush(true);
        }
    }

    public static void FileWrite(string path, string data, Encoding encoding, bool append = false) => FileWrite(path, encoding.GetBytes(data), append);

    public static string FilenameSanitize(string path, string replacement)
    {
        //if (replacement == null) throw new ArgumentNullException("replacement");

        var illegalChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        var pathChars = path.ToCharArray();
        var sb = new StringBuilder();

        for (var i = 0; i < pathChars.Length; i++)
        {
            var c = pathChars[i];
            if (illegalChars.Contains(c))
            {
                if (replacement != null) sb.Append(replacement);
            }
            else { sb.Append(c); }
        }

        return sb.ToString();
    }

    #endregion File

    #region Path

    //private static readonly string[] pathDelimitersStrings = Constant.PathDelimiters_String.ToArray();
    //public static string[] PathParts(string path) => path.Split(pathDelimitersStrings, StringSplitOptions.RemoveEmptyEntries).TrimOrNull().WhereNotNull().ToArray();

    /// <summary>
    /// Returns true if the path provided is an absolute (full) path
    /// https://stackoverflow.com/a/47569899
    /// </summary>
    /// <param name="path">File system path to check</param>
    /// <returns>true if path is absolute, false otherwise</returns>
    public static bool PathIsAbsolute(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1) return false;
        if (!Path.IsPathRooted(path)) return false;

        var pathRoot = Path.GetPathRoot(path);
        if (pathRoot == null) return false;

        if (pathRoot.Length <= 2 && pathRoot != "/") return false; // Accepts X:\ and \\UNC\PATH, rejects empty string, \ and X:, but accepts / to support Linux

        if (pathRoot[0] != '\\' || pathRoot[1] != '\\') return true; // Rooted and not a UNC path

        return pathRoot.Trim('\\').IndexOf('\\') != -1; // A UNC server name without a share name (e.g "\\NAME" or "\\NAME\") is invalid
    }

    public static string PathToString(IEnumerable<string> pathParts, string pathDelimiter = "/", bool trailingDelimiter = true)
    {
        var s = string.Join(pathDelimiter, pathParts).TrimOrNull() ?? string.Empty;
        s = pathDelimiter + s;
        if (s.Length > 1 && trailingDelimiter) s = s + pathDelimiter;

        return s;
    }
    
    public static DirectoryInfo? PathDirectoryOrParentDirectory(string? path)
    {
        if (path == null) return null;
        DirectoryInfo? fso;
        try
        {
            if (IsFile(path)) fso = new FileInfo(path).Directory;
            else if (IsDirectory(path)) fso = new(path);
            else return null;
        }
        catch (Exception)
        {
            return null;
        }
        
        if (fso == null) return null;
        while (true)
        {
            try
            {
                if (fso.Exists) break;
                var p = fso.Parent;
                if (p == null) break;
                fso = p;
            }
            catch (Exception)
            {
                break;
            }
        }
        
        return fso;
    }

    #endregion Path
}
