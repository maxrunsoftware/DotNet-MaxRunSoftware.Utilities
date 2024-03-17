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

// ReSharper disable RedundantNullableDirective
// ReSharper disable RedundantUsingDirective
// ReSharper disable IdentifierTypo
// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
// ReSharper disable PossibleNullReferenceException
// ReSharper disable StringLiteralTypo
// ReSharper disable AssignNullToNotNullAttribute

#nullable enable

using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MaxRunSoftware;

public static class Extensions
{
    private static readonly StringComparer SC = StringComparer.OrdinalIgnoreCase;
    private static readonly StringComparison SCN = StringComparison.OrdinalIgnoreCase;
    
    public static string? TrimOrNull(this object? o)
    {
        var s = o?.ToString();
        if (s == null) return null;
        s = s.Trim();
        if (s.Length == 0) return null;
        return s;
    }

    public static bool Eq(this string? x, string? y, bool trim = false) => trim
        ? string.Equals(TrimOrNull(x), TrimOrNull(y), SCN)
        : string.Equals(x, y, SCN);

    public static IEnumerable<string> TrimOrNull(this IEnumerable<string?> enumerable, bool removeNull = true) => enumerable.Select(o => o.TrimOrNull()).Where(o => !removeNull || o != null).Select(o => o!);

    public static string? ResolvePath(this object? pathObj)
    {
        var path = pathObj.TrimOrNull();
        if (path == null) return null;
        if (path.Contains('~'))
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            // ~/ foo/bar   ->   /home/user/ foo/bar
            path = path.Replace("~" + Path.DirectorySeparatorChar, userDir);

            // ~ foo/bar   ->   /home/user/ foo/bar
            path = path.Replace("~", userDir);
        }

        if (File.Exists(path)) return new FileInfo(path).FullName;

        if (Directory.Exists(path)) return new DirectoryInfo(path).FullName;

        return Path.GetFullPath(path);
    }

    public static FileInfo? ResolveFile(this object? pathObj)
    {
        var path = pathObj.ResolvePath();
        if (path == null) return null;
        if (File.Exists(path)) return new FileInfo(path);
        if (Directory.Exists(path)) throw new FileNotFoundException($"Path is a Directory not a File: {path}", path);
        return new FileInfo(path);
    }
    
    public static DirectoryInfo? ResolveDirectory(this object? pathObj)
    {
        var path = pathObj.ResolvePath();
        if (path == null) return null;
        if (File.Exists(path)) throw new DirectoryNotFoundException($"Path is a File not a Directory: {path}");
        if (Directory.Exists(path)) return new DirectoryInfo(path);
        return new DirectoryInfo(path);
    }

    public static T Required<T>(
        this T? value, 
        string? propertyName = null,
        bool trim = true,
        [CallerMemberName] string callerMethod = "", 
        [CallerArgumentExpression("value")] string callerPropertyName = ""
        ) where T : class
    {
        if (value != null && value is string s && trim) value = s.TrimOrNull() as T;
        if (value != null) return value;
        
        callerMethod = callerMethod.TrimOrNull() ?? "?";
        propertyName ??= callerPropertyName;
        
        if (callerMethod.StartsWith("Init", SCN))
        {
            throw new ApplicationException($"Property '{propertyName}' cannot be null");
        }
        else
        {
            throw new ApplicationException($"Property '{callerMethod}' references property '{propertyName}' but it is not set");
        }
    }

    public static List<FileInfo> DirectoryGetFiles(this string dirString, string? exts = null)
    {
        var extSet = (exts ?? "")
            .Split(',', '|', ' ')
            .TrimOrNull()
            .Select(o => o.TrimStart('.'))
            .TrimOrNull()
            .SelectMany(o => new[] { o, "." + o })
            .ToHashSet(SC)
            ;
        
        var d = dirString.ResolveDirectory() ?? throw new ArgumentNullException(nameof(dirString));
        
        var fis = new List<FileInfo>();
        foreach (var fi in d.GetFiles())
        {
            if (extSet.Count > 0)
            {
                var ext = fi.Extension.TrimOrNull();
                if (ext == null) continue;
                if (!extSet.Contains(ext)) continue;
            }
            
            fis.Add(fi);
        }

        return fis;
    }

    public  static List<string> SlnRead(this string slnFile) => File.ReadAllLines(slnFile, new UTF8Encoding(false)).TrimOrNull().ToList();
    public static HashSet<string> SlnGetBuildTypes(this string slnFile)
    {
        // GlobalSection(SolutionConfigurationPlatforms) = preSolution
        //   	Debug|Any CPU = Debug|Any CPU
        //   	Release|Any CPU = Release|Any CPU
        // EndGlobalSection

        var buildLines = new List<string>();
        var foundBuilds = false;
        var removeWhitespace = (string x) => new string(x.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
        foreach (var line in slnFile.SlnRead())
        {
            var eq = (string x) => Eq(removeWhitespace(x), removeWhitespace(line));
            if (eq("GlobalSection(SolutionConfigurationPlatforms) = preSolution"))
                foundBuilds = true;
            else if (eq("EndGlobalSection"))
                foundBuilds = false;
            else if (foundBuilds)
                buildLines.Add(line);
        }

        var buildTypes = buildLines
                .SelectMany(o => o.Split('='))
                .Select(o => o.Split('|')[0])
                .TrimOrNull()
                .ToHashSet(SC)
            ;

        if (buildTypes.Count == 0) throw new ApplicationException("Could not determine any valid build types from .sln file");
        return buildTypes;
    }
    
    
    public static List<FileInfo> SlnGetProjectFiles(this string slnFile)
    {
        // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MaxRunSoftware.Utilities.Common", "src\MaxRunSoftware.Utilities.Common\MaxRunSoftware.Utilities.Common.csproj", "{D2F4C033-3B7E-4B55-87A1-5D76297DC512}"
        // EndProject
        // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "MaxRunSoftware.Utilities.Common.Tests", "tests\MaxRunSoftware.Utilities.Common.Tests\MaxRunSoftware.Utilities.Common.Tests.csproj", "{DC93EE06-DD45-4155-B5DB-6EB5B79ED00F}"
        // EndProject
        // Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Common", "Common", "{25854C5F-E393-4378-8AC3-47E670A2C18C}"
        // EndProject

        var slnDirectory = slnFile.GetParentDirectory().FullName;

        FileInfo? ParseFile(string? s)
        {
            if (s == null) return null;
            var csprojParts = s
                .Split('/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Prepend(slnDirectory)
                .Where(o => TrimOrNull(o) != null)
                .Select(o => o)
                .ToArray();
            var fi = Path.Combine(csprojParts).ResolveFile();
            if (fi == null) return null;
            if (fi.Exists) return fi;
            // LogError($".sln refers to .csproj '{csPath}' but file does not exist: {line}");
            return null;            
        }

        return slnFile.SlnRead()
                .Where(o => o.StartsWith("Project(", SCN))
                .SelectMany(o => o.Split('"'))
                .TrimOrNull()
                .Where(o => o.EndsWith(".csproj", SCN))
                .Select(ParseFile)
                .Where(o => o != null)
                .Select(o => o!)
                .DistinctBy(o => o.FullName, SC)
                .OrderBy(o => o.FullName, SC)
                .ToList()
            ;

    }

    public static string ToDelimited(this IEnumerable<object?> enumerable, string delimiter) => string.Join(delimiter, enumerable.ToArray());

    public static DirectoryInfo GetParentDirectory(this object o)
    {
        var m = "Could not determine parent directory for";
        if (o is FileInfo fi) return fi.Directory ?? throw new DirectoryNotFoundException($"{m} file: {fi.FullName}");
        if (o is DirectoryInfo di) return di.Parent ?? throw new DirectoryNotFoundException($"{m} directory: {di.FullName}");
        if (o is string s)
        {
            var ss = s.ResolvePath();
            if (ss == null) throw new ArgumentNullException(nameof(o), $"{m} null object");
            if (File.Exists(ss)) return GetParentDirectory(new FileInfo(ss));
            if (Directory.Exists(ss)) return GetParentDirectory(new DirectoryInfo(ss));
            throw new ArgumentException($"{m} string: {s}", nameof(o));
        }
        throw new ArgumentException($"Invalid type: {o.GetType().FullName}", nameof(o));
    }

    public static bool IsExtension(this FileInfo fi, params string?[] exts)
    {
        foreach (var extt in exts)
        {
            var ext = extt.TrimOrNull()?.Trim('.').TrimOrNull();
            var fext = fi.Extension.TrimOrNull()?.Trim('.').TrimOrNull();
            if (ext == null && fext == null)
            {
                return true;
            }
            if (ext != null && fext != null && string.Equals(fext, ext, SCN))
            {
                return true;
            }
        }

        return false;
    }
}

public class Builder
{
    public static void Main(string[] args) => Console.WriteLine("Not meant to be executed: " + string.Join(", ", args));

    private static readonly StringComparer SC = StringComparer.OrdinalIgnoreCase;
    private static readonly StringComparison SCN = StringComparison.OrdinalIgnoreCase;

    public string? TrimOrNull(object? o) => o.TrimOrNull();
    
    #region Properties
    
    public bool DebugDoNotDelete { get; set; }

    public Action<string, object?, Exception?>? Log { get; set; }

    private void LogMessage(string level, object? msg, Exception? e = null) => Log.Required()(level, msg ?? "", e);
    public void LogD(object? msg, Exception? e = null) => LogMessage("Debug", msg, e);
    public void LogV(object? msg, Exception? e = null) => LogMessage("Verbose", msg, e);
    public void LogI(object? msg, Exception? e = null) => LogMessage("Information", msg, e);
    public void LogW(object? msg, Exception? e = null) => LogMessage("Warning", msg, e);
    public void LogE(object? msg, Exception? e = null) => LogMessage("Error", msg, e);

    public string? ScriptDir { get; set; }

    public string? BuildAction { get; set; }

    public string? SlnFile { get; set; }

    public string SlnDir
    {
        get
        {
            var f = SlnFile.ResolveFile();
            if (f == null) throw new ArgumentNullException(nameof(SlnFile), $"Property {SlnFile} not set");
            if (!f.Exists) throw new FileNotFoundException($"File does not exist: {f.FullName}", f.FullName);
            return f.GetParentDirectory().FullName;
        }
    }
  
    public string? BuildDir { get; set; }
    public string? BuildDirDll { get; set; } 
    public string? BuildDirZip { get; set; } 
    public string? BuildDirNuget { get; set; }
    
    public DateTime? BuildDateTime { get; set; } = DateTime.UtcNow;
    public string? BuildVersionType { get; set; }
    public string? BuildVersionSuffix { get; set; }

    public string? BuildType { get; set; }

    public string? GitId { get; set; }
    public string? GitBranch { get; set; }
    public string? GitUrl { get; set; }
    public string? GitKeyPath { get; set; }

    public ICollection<string>? ProjectFiles { get; set; } = new List<string>();

    public IEnumerable<string> BuildDirNugetFiles
    {
        get
        {
            var d = BuildDirNuget.ResolveDirectory();
            if (d == null) throw new ArgumentNullException(nameof(BuildDirNuget), $"Property {BuildDirNuget} not set");
            if (!d.Exists) throw new DirectoryNotFoundException($"Directory does not exist: {d.FullName}");
            return d.GetFiles()
                .Where(o => o.IsExtension("nupkg"))
                .Select(o => o.FullName)
                .OrderBy(o => o, SC)
                .ToList();
        }
    }
    
    #endregion Properties

    public void Init()
    {
        var vScriptDir = ScriptDir.ResolveDirectory();
        if (vScriptDir == null) throw new ArgumentNullException(nameof(ScriptDir));
        if (!vScriptDir.Exists) throw new DirectoryNotFoundException($"{nameof(ScriptDir)} does not exist: {vScriptDir.FullName}");
        ScriptDir = vScriptDir.FullName;

        var vSlnFile = SlnFile.ResolveFile();
        if (vSlnFile == null)
        {
            var slnFiles = ScriptDir.DirectoryGetFiles("sln");
            if (slnFiles.Count < 1) throw new FileNotFoundException($"Could not find .sln file in directory: {ScriptDir}");
            if (slnFiles.Count > 1) throw new FileNotFoundException($"Found {slnFiles.Count} .sln files in directory: {ScriptDir}");
            vSlnFile = slnFiles[0].FullName.ResolveFile()!;
        }
        if (!vSlnFile.Exists) throw new FileNotFoundException($"{nameof(SlnFile)} does not exist: {vSlnFile.FullName}");
        SlnFile = vSlnFile.FullName;

        string ResolveBuildDir(string? propertyValue, string baseDir, string newDirName)
        {
            var d = propertyValue.ResolveDirectory()?.FullName;
            if (d != null) return d;
            baseDir = baseDir.ResolvePath() ?? throw new ArgumentNullException(nameof(baseDir));
            if (File.Exists(baseDir)) baseDir = baseDir.GetParentDirectory().FullName;
            return Path.Combine(baseDir, newDirName).ResolveDirectory()!.FullName;
        }

        BuildDir = ResolveBuildDir(BuildDir, SlnFile, "build");
        BuildDirDll = ResolveBuildDir(BuildDirDll, BuildDir, "dll");
        BuildDirZip = ResolveBuildDir(BuildDirZip, BuildDir, "zip");
        BuildDirNuget = ResolveBuildDir(BuildDirNuget, BuildDir, "nuget");
        
        BuildVersionType = BuildVersionType.TrimOrNull();
        
        if (BuildVersionSuffix == null)
        {
            var sb = new StringBuilder();
            if (BuildVersionType != null) sb.Append((sb.Length == 0 ? "" : "-") + BuildVersionType);
            if (BuildDateTime != null) sb.Append((sb.Length == 0 ? "" : "-") + BuildDateTime.Value.ToString("yyyyMMdd") + "-" + BuildDateTime.Value.ToString("HHmmss"));
            BuildVersionSuffix = sb.ToString().TrimOrNull();
        }
        

        var buildTypes = SlnFile.SlnGetBuildTypes();
        BuildType = BuildType.TrimOrNull();
        if (BuildType == null)
        {
            BuildType = buildTypes.FirstOrDefault(o => o.Equals("Debug", SCN)) ?? buildTypes.FirstOrDefault(o => o.StartsWith("Debug", SCN));
            if (BuildType == null) throw new ArgumentNullException($"{nameof(BuildType)} is not defined and no 'Debug' build found");
        }

        BuildType = buildTypes.FirstOrDefault(o => o.Equals(BuildType, SCN)) ?? throw new ArgumentException($"{nameof(BuildType)} '{BuildType}' is not a valid build type: " + string.Join(", ", buildTypes.OrderBy(o => o, SC)));
        
        BuildAction = BuildAction.Required(nameof(BuildAction));

        var vGitKeyPath = GitKeyPath.ResolveFile();
        if (vGitKeyPath != null)
        {
            if (!vGitKeyPath.Exists) throw new FileNotFoundException($"{nameof(GitKeyPath)} does not exist: {vGitKeyPath.FullName}");
            GitKeyPath = vGitKeyPath.FullName;
        }

        GitId = GitId.TrimOrNull();
        GitBranch = GitBranch.TrimOrNull();
        GitUrl = GitUrl.TrimOrNull();

        if (ProjectFiles == null || ProjectFiles.Count == 0)
        {
            ProjectFiles = SlnFile.SlnGetProjectFiles().Select(o => o.FullName).ToList();
            if (ProjectFiles.Count == 0) throw new ApplicationException($"{nameof(SlnFile)} does not contain any projects: {SlnFile}");
        }

    }
    
    #region Actions
    
    public string ReadGitKey()
    {
        var gitKeyFileInfo = GitKeyPath.ResolveFile();
        if (gitKeyFileInfo == null) throw new ArgumentNullException(nameof(GitKeyPath));
        if (!gitKeyFileInfo.Exists) throw new FileNotFoundException($"{nameof(GitKeyPath)} file does not exist: {gitKeyFileInfo.FullName}", gitKeyFileInfo.FullName);
        // LogV($"Reading {nameof(GitKeyPath)}: {GitKeyPath}");
        var gitKey = File.ReadAllText(gitKeyFileInfo.FullName, new UTF8Encoding(false)).TrimOrNull();
        return gitKey ?? throw new ApplicationException($"{nameof(GitKeyPath)} file was empty: {gitKeyFileInfo.FullName}");
    }
    
    public void Clean()
    {
        // CleanBuildDirs();
        CleanProjectDirs();
    }

    private void CleanBuildDir(string? path, string?[] extensions, string propertyName)
    {
        var d = path.ResolveDirectory();
        if (d == null) throw new ArgumentNullException(propertyName, $"{propertyName} is not set");
        LogI($"Cleaning [{propertyName}]: {d.FullName}");
        if (!d.Exists)
        {
            LogV($"  Creating [{propertyName}]: {d.FullName}");
            Directory.CreateDirectory(d.FullName);
        }

        var filesToDelete = d.GetFiles().Where(o => o.IsExtension(extensions)).ToList();
        foreach (var fi in filesToDelete)
        {
            if (!fi.Exists) continue;
            LogV($"  Deleting File: {fi.FullName}");
            try
            {
                if (DebugDoNotDelete)
                {
                    LogV($"   !{nameof(DebugDoNotDelete)}={DebugDoNotDelete}  skipping delete of file");
                }
                else
                {
                    if (fi.IsReadOnly) fi.IsReadOnly = false;
                    fi.Delete();
                    LogV($"    success");
                }
            }
            catch (Exception e)
            {
                LogW($"    Error deleting file: {fi.FullName}", e);
            }
        }
        
        LogV($"Cleaning [{propertyName}] complete: {d.FullName}");
    }
    
    public void CleanBuildDirs()
    {
        var exts = ("exe dll xml zip txt nupkg snupkg nuspec".Split(' '));
        CleanBuildDir(BuildDir, exts, nameof(BuildDir));
        CleanBuildDir(BuildDirDll, exts, nameof(BuildDirDll));
        CleanBuildDir(BuildDirZip, exts, nameof(BuildDirZip));
        CleanBuildDir(BuildDirNuget, exts, nameof(BuildDirNuget));
    }
    
    
    private List<(FileInfo, DirectoryInfo)> GetProjects(IEnumerable<string?>? excludedProjects = null)
    {
        var ep = (excludedProjects ?? Enumerable.Empty<string?>()).TrimOrNull().ToHashSet(SC);
        
        static bool IsValidProject(FileInfo fi, HashSet<string> excluded)
        {
            foreach (var n in new[] { fi.Name, fi.GetParentDirectory().Name })
            {
                var name = n.TrimOrNull();
                if (name == null) return false;
                if (excluded.Contains(name)) return false;
                var nameParts = name.Split('.').TrimOrNull().ToArray();
                if (nameParts.Any(excluded.Contains)) return false;
            }

            return true;
        }
        
        var projectFileInfos = (ProjectFiles ?? Enumerable.Empty<string>())
            .Select(o => o.ResolveFile())
            .Where(o => o != null && o.Exists)
            .Select(o => o!)
            .Where(o => IsValidProject(o, ep))
            .ToList();
        
        return projectFileInfos
            .Select(o => (o, o.GetParentDirectory()))
            .Where(o => o.Item2.Exists)
            .ToList();
    }

    public void CleanProjectDirs()
    {
        LogI($"Cleaning [{nameof(ProjectFiles)}]");
        LogV($"Cleaning [{nameof(ProjectFiles)}] Started");

        foreach (var (_, projectDir) in GetProjects())
        {
            LogI($"  Cleaning [{nameof(ProjectFiles)}]: {projectDir.FullName}");
            LogV($"  Cleaning [{nameof(ProjectFiles)}] Started: {projectDir.FullName}");
            var cleanDirNames = new HashSet<string>(new []{"bin", "obj"}, SC);
            foreach (var dirToDelete in projectDir.GetDirectories().Where(o => cleanDirNames.Contains(o.Name)))
            {
                LogV($"  | Deleting Directory: {dirToDelete.FullName}");

                if (DebugDoNotDelete)
                {
                    LogV($"  |  !{nameof(DebugDoNotDelete)}={DebugDoNotDelete}  skipping delete of directory");
                }
                else
                {
                    try
                    {
                        dirToDelete.Delete(recursive: true);
                        LogV($"  |   success");
                    }
                    catch (Exception e)
                    {
                        LogW($"  |   Error deleting directory: {dirToDelete.FullName}", e);
                    }
                }
            }

            LogV($"  Cleaning [{nameof(ProjectFiles)}] Complete: {projectDir.FullName}");
        }
        
        LogV($"Cleaning [{nameof(ProjectFiles)}] Complete");
    }

    public void Zip()
    {
        LogW($"{nameof(Zip)}() not implemented");

        //using var zip = ZipFile.Open("test.zip", ZipArchiveMode.Create);
        //zip.CreateEntryFromFile(@"c:\something.txt", "data/path/something.txt");
    }

    public void CopyNugetFiles(params string?[] projectsToExclude)
    {
        var d = BuildDirNuget.ResolveDirectory();
        if (d == null) throw new ArgumentNullException(nameof(BuildDirNuget), $"Property {BuildDirNuget} not set");

        var buildType = BuildType.TrimOrNull();
        if (buildType == null) throw new ApplicationException($"Cannot execute action '{nameof(CopyNugetFiles)}' because property {BuildType} is not set");
        
        LogI($"Copying Nuget Files [{nameof(BuildDirNuget)}]: {d.FullName}");
        LogV($"Copying Nuget Files [{nameof(BuildDirNuget)}] Started: {d.FullName}");

        if (d.Exists)
        {
            LogV($" | Deleting Directory [{nameof(BuildDirNuget)}]: {d.FullName}");
            if (DebugDoNotDelete)
            {
                LogV($" |  !{nameof(DebugDoNotDelete)}={DebugDoNotDelete}  skipping delete of directory");
            }
            else
            {
                try
                {
                    d.Delete(recursive: true);
                    LogV($" |  success");
                }
                catch (Exception e)
                {
                    LogW($" |  Error deleting directory: {d.FullName}", e);
                }
            }
        }
        if (!d.Exists) d.Create();

        foreach (var (_, pd) in GetProjects(projectsToExclude))
        {
            var binDir = pd.GetDirectories().FirstOrDefault(o => string.Equals(o.Name, "bin", SCN));
            if (binDir == null)
            {
                LogW($" | Could not locate 'bin' directory for project: {pd.FullName}");
                continue;
            }

            var buildDir = binDir.GetDirectories().FirstOrDefault(o => string.Equals(o.Name, buildType, SCN));
            if (buildDir == null)
            {
                LogW($" | Could not locate '{buildType}' directory in project bin directory: {binDir.FullName}");
                continue;
            }

            var files = buildDir.GetFiles().Where(o => o.IsExtension("nupkg")).ToList();
            if (files.Count > 0)
            {
                LogV($" | Copying {files.Count} files for project [{nameof(BuildDirNuget)}]: {pd.FullName}");
                foreach (var file in files)
                {
                    var targetFile = Path.Join(d.FullName, file.Name).ResolveFile()!;
                    LogV(" |   " + (targetFile.Exists ? "Overwriting existing" : "Copying") + $" file:  {file.FullName}  -->  {targetFile.FullName}");
                    file.CopyTo(targetFile.FullName, true);
                    LogI($"  {file.Name}");
                }
            }
        }
        
        LogV($"Copying Nuget Files [{nameof(BuildDirNuget)}] Complete: {d.FullName}");
    }

    #endregion Actions

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetType().FullName!.Split('_')[0]); // Might add _ suffix in PowerShell to class 

        static string ItemToString(object? o)
        {
            var s = o?.ToString()?.TrimOrNull();

            if (s == null) return string.Empty;
            try
            {
                var path = s.ResolvePath();
                if (File.Exists(path))
                {
                    s = new FileInfo(path).Name + "  -->  " + s;
                }
                else if (Directory.Exists(path))
                {
                    s = new DirectoryInfo(path).Name + "  -->  " + s;
                }
            }
            catch (Exception) { }

            return s;
        }

        foreach (var pi in GetType().GetProperties().OrderBy(o => o.Name, SC))
        {
            if (!pi.CanRead) continue;
            var p = $"  {pi.Name}: ";
            var pError = $"! {pi.Name}: ";
            try
            {
                var o = pi.GetValue(this) ?? "";
                if (o is IEnumerable enumerable and not string)
                {
                    var items = enumerable.OfType<object?>().Where(oo => oo != null).ToList();

                    if (items.Count > 20)
                    {
                        sb.AppendLine($"{p}({items.Count} items)");
                    }
                    else
                    {
                        sb.AppendLine($"{p}");
                        foreach (var item in items)
                        {
                            sb.AppendLine($"    {ItemToString(item)}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"{p}{ItemToString(o)}");
                }
            }
            catch (Exception e)
            {
                sb.AppendLine($"{pError}[{e.GetType().Name}] {e.Message}");
            }
        }

        return sb.ToString();
    }

 


}
