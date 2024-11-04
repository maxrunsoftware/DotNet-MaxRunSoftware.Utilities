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

using System.Diagnostics;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    static Constant()
    {
        Path_Current_Locations_Extensions = new(() => new[] { "exe", "dll", }.SelectMany(PermuteCase).Distinct(Path_StringComparer).ToImmutableArray());
        Path_Current_Locations = new(Path_Current_Locations_DirectoriesAndFiles);
        path_Current_Directories = new (() => Path_Current_Locations.Value.Select(o => o as DirectoryInfo).Where(o => o != null).Select(o => o!).ToImmutableArray());
        path_Current_Files = new (() => Path_Current_Locations.Value.Select(o => o as FileInfo).Where(o => o != null).Select(o => o!).ToImmutableArray());
    }
    
    #region Path_Current_Locations

    private static readonly Lazy<ImmutableArray<string>> Path_Current_Locations_Extensions;
    private static readonly Lazy<ImmutableArray<FileSystemInfo>> Path_Current_Locations;

    /// <summary>
    /// https://stackoverflow.com/questions/616584/how-do-i-get-the-name-of-the-current-executable-in-c
    /// https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview?tabs=cli#api-incompatibility
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<Func<string?>> Path_Current_Locations_DirectoriesAndFiles_Functions() =>
    [
        () => Environment.ProcessPath,
        () => Environment.GetCommandLineArgs().FirstOrDefault(),
        () => Environment.CurrentDirectory,
        () => Process.GetCurrentProcess().MainModule?.FileName,
        () => AppDomain.CurrentDomain.FriendlyName,
        () => Process.GetCurrentProcess().ProcessName,
        () => typeof(Constant).Assembly.Location,
        () => Path.GetFullPath("."),
    ];

    private static ImmutableArray<FileSystemInfo> Path_Current_Locations_DirectoriesAndFiles() =>
        Path_Current_Locations_DirectoriesAndFiles_Functions()
            .Select(o => o.InternalExecuteSafe())
            .Where(o => !string.IsNullOrEmpty(o))
            .Select(o => o!)
            .Distinct(Path_StringComparer)
            .SelectMany(Path_Current_Locations_DirectoriesAndFiles_Single)
            .ToImmutableArray();

    private static IEnumerable<FileSystemInfo> Path_Current_Locations_DirectoriesAndFiles_Single(string s)
    {
        if (string.IsNullOrEmpty(s)) return [];

        var fi = GetFile(s);
        if (fi != null) return [fi];
        
        var di = GetDirectory(s);
        if (di != null) return [di];

        var list = new List<FileSystemInfo>();

        foreach (var ext in Path_Current_Locations_Extensions.Value)
        {
            fi = GetFile(s + "." + ext);
            if (fi != null) list.Add(fi);
        }

        return list;
        
        static FileInfo? GetFile(string path)
        {
            var info = InternalExecuteSafe(() => new FileInfo(path));
            if (info == null) return null;
            if (!InternalExecuteSafe(() => info.IsFile() ?? false, false)) return null;
            if (!InternalExecuteSafe(() => info.Exists, false)) return null;
            return info;
        }
        
        static DirectoryInfo? GetDirectory(string path)
        {
            var info = InternalExecuteSafe(() => new DirectoryInfo(path));
            if (info == null) return null;
            if (!InternalExecuteSafe(() => info.IsDirectory() ?? false, false)) return null;
            if (!InternalExecuteSafe(() => info.Exists, false)) return null;
            return info;
        }
        
    }

    private static readonly Lazy<ImmutableArray<DirectoryInfo>> path_Current_Directories;
    
    public static ImmutableArray<DirectoryInfo> Path_Current_Directories => path_Current_Directories.Value;
    
    public static DirectoryInfo? Path_Current_Directory => Path_Current_Directories.Length == 0 ? null : Path_Current_Directories[0];

    private static readonly Lazy<ImmutableArray<FileInfo>> path_Current_Files;
    
    public static ImmutableArray<FileInfo> Path_Current_Files => path_Current_Files.Value;
    
    public static FileInfo? Path_Current_File => Path_Current_Files.Length == 0 ? null : Path_Current_Files[0];

    #endregion Path_Current_Locations

    #region IsScriptExecuted

    /// <summary>
    /// Are we executing via a batch file or script or running the command directly from the console window?
    /// </summary>
    public static readonly bool IsScriptExecuted = IsScriptExecuted_Create();

    private static bool IsScriptExecuted_Create()
    {
        try
        {
            // http://stackoverflow.com/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected?lq=1
            //return (0 == (System.Console.WindowHeight + System.Console.WindowWidth)) && System.Console.KeyAvailable;
            if (Console.WindowHeight != 0) return false;

            if (Console.WindowWidth != 0) return false;

            if (!Console.KeyAvailable) return false;

            return true;
        }
        catch (Exception e) { LogError(e); }

        return false;
    }

    #endregion IsScriptExecuted
}
