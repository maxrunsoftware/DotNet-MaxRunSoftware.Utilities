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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MaxRunSoftware;


/// <summary>
/// Set from Powershell
/// </summary>
public partial class Builder
{
    private const string BuildType_Debug = "Debug";
    private const string BuildType_Release = "Release";
    
    public string? ScriptDir { get; set; }
    
    public Action<string, object?, Exception?>? Log { get; set; }

    private string buildType = BuildType_Debug;
    public string BuildType
    {
        get => buildType;
        set
        {
            var v = TrimOrNull(value) ?? BuildType_Debug;
            if (SC.Equals(v, BuildType_Debug)) v = BuildType_Debug;
            if (SC.Equals(v, BuildType_Release)) v = BuildType_Release;
            if (!(SC.Equals(v, BuildType_Debug) || SC.Equals(v, BuildType_Release))) throw new ArgumentException($"Invalid build type [{BuildType_Debug}|{BuildType_Release}]: {v}", nameof(BuildType));
            buildType = v;
        }
    }

    private string? buildVersionType;
    public string? BuildVersionType { get => buildVersionType; set => buildVersionType = TrimOrNull(value); }

    private string? gitKeyPath;
    public string? GitKeyPath { get => gitKeyPath; set => gitKeyPath = TrimOrNull(value); }
    
    private string? gitId;
    public string? GitId { get => gitId; set => gitId = TrimOrNull(value); }

    private string? gitBranch;
    public string? GitBranch { get => gitBranch; set => gitBranch = TrimOrNull(value); }
    
    private string? gitUrl;
    public string? GitUrl { get => gitUrl; set => gitUrl = TrimOrNull(value); }
    
    public DateTime? BuildDateTime { get; set; } = DateTime.UtcNow;
    
    public string? ProjectsToIncludeNuget { get; set; }

    public string? FileExtensionsToCleanInBuildDirs { get; set; } // = "exe dll xml zip txt nupkg snupkg nuspec";
    
    public bool DebugDoNotDelete { get; set; }

}

/// <summary>
/// Properties External
/// </summary>
public partial class Builder
{
    public string SolutionFile
    {
        get
        {
            var scriptDir = ScriptDirRequired;
            var solutionFiles = GetFilesWithExtensions(new DirectoryInfo(scriptDir), "slnx");
            if (solutionFiles.Length < 1) throw new FileNotFoundException($"Could not find .slnx file in directory: {scriptDir}");
            if (solutionFiles.Length > 1) throw new FileNotFoundException($"Found {solutionFiles.Length} .slnx files in directory: {scriptDir}");
            var solutionFile = ResolveFileRequired(solutionFiles[0].FullName, nameof(SolutionFile));
            return solutionFile.FullName;
        }
    }
    
    public string SolutionDir => GetParentDirectory(SolutionFile).FullName;
    
    public string? BuildVersionSuffix
    {
        get
        {
            var sb = new StringBuilder();
            if (BuildVersionType != null)
            {
                sb.Append(sb.Length == 0 ? "" : "-");
                sb.Append(BuildVersionType);
            }
            if (BuildDateTime != null)
            {
                sb.Append(sb.Length == 0 ? "" : "-");
                sb.Append(BuildDateTime.Value.ToString("yyyyMMdd"));
                sb.Append('-');
                sb.Append(BuildDateTime.Value.ToString("HHmmss"));
            }
            return TrimOrNull(sb.ToString());
        }
    }
    
    public IEnumerable<string> BuildDirNugetFiles =>
        GetFilesWithExtensions(
            ResolveDirectoryRequired(BuildDirNuget, nameof(BuildDirNuget)),
            "nupkg"
        ).Select(o => o.FullName)
        .OrderBy(o => o, SC)
        ;
    
   public string GetGitKey() =>
        TrimOrNull(File.ReadAllText(ResolveFileRequired(GitKeyPath, nameof(GitKeyPath)).FullName, Encoding.UTF8))
        ?? throw new ApplicationException($"{nameof(GitKeyPath)} file was empty: {ResolveFileRequired(GitKeyPath, nameof(GitKeyPath)).FullName}");
    
}

/// <summary>
/// Logging
/// </summary>
public partial class Builder
{
    public bool LogHasWarnings { get; private set; }
    public bool LogHasErrors { get; private set; }
    
    private void LogMessage(string level, object? msg, Exception? e = null)
    {
        if (Log == null) throw new ApplicationException($"Property '{nameof(Log)}' not set");
        Log(level, msg ?? "", e);
    }

    public void LogD(object? msg, Exception? e = null) => LogMessage("Debug", msg, e);
    public void LogV(object? msg, Exception? e = null) => LogMessage("Verbose", msg, e);
    public void LogI(object? msg, Exception? e = null) => LogMessage("Information", msg, e);
    public void LogW(object? msg, Exception? e = null)
    {
        LogHasWarnings = true;
        LogMessage("Warning", msg, e);
    }

    public void LogE(object? msg, Exception? e = null)
    {
        LogHasErrors = true;
        LogHasWarnings = true;
        LogMessage("Error", msg, e);
    }
}

/// <summary>
/// Utils
/// </summary>
public partial class Builder
{
    public static void Main(string[] args) => Console.WriteLine("Not meant to be executed: " + string.Join(", ", args));

    private static readonly StringComparer SC = StringComparer.OrdinalIgnoreCase;
    private static readonly StringComparison SCN = StringComparison.OrdinalIgnoreCase;
    
#pragma warning disable CA1822
    // ReSharper disable once MemberCanBeMadeStatic.Global
    // Needed by PowerShell
    public string? TrimOrNull(object? o) => TrimOrNull_Static(o);
#pragma warning restore CA1822

    public string[] SplitAndTrim(string? s) => SplitAndTrim_Static(s);
}



/// <summary>
/// Properties Internal
/// </summary>
public partial class Builder
{
    private string ScriptDirRequired => ResolveDirectoryRequired(ScriptDir, nameof(ScriptDir)).FullName;
    
    private string BuildDir => ResolveDirectory(Path.Combine(SolutionDir, "build"))!.FullName;
    private string BuildDirDll => ResolveDirectory(Path.Combine(BuildDir, "dll"))!.FullName;
    private string BuildDirZip => ResolveDirectory(Path.Combine(BuildDir, "zip"))!.FullName;
    private string BuildDirNuget => ResolveDirectory(Path.Combine(BuildDir, "nuget"))!.FullName;
    
    private ICollection<(string? Folder, string ProjectDir, string ProjectFile, bool IsTest)> Projects
    {
        get
        {
            const string dirSrc = "src";
            const string dirTest = "test";

            var solutionFile = SolutionFile;
            var solutionDir = SolutionDir;
            var list = new List<(string? Folder, string ProjectDir, string ProjectFile, bool IsTest)>();
            var xmlString = File.ReadAllText(solutionFile, Encoding.UTF8);
            var solutionElement = XElement.Parse(xmlString);
            if (!NameIs(solutionElement.Name, "Solution")) throw new ApplicationException($"Invalid solution file: {solutionFile}");

            foreach (var folderElement in solutionElement.Elements().Where(o => NameIs(o.Name, "Folder")))
            {
                var folderName = TrimOrNull(GetAttr(folderElement, "Name")?.Trim('/', '\\'));
                foreach (var projectElement in folderElement.Elements().Where(o => NameIs(o.Name, "Project")))
                {
                    list.Add(ParseProject(folderName, projectElement));
                }
            }

            foreach (var projectElement in solutionElement.Elements().Where(o => NameIs(o.Name, "Project")))
            {
                list.Add(ParseProject(null, projectElement));
            }

            if (list.Count == 0) throw new ApplicationException($"{nameof(SolutionFile)} does not contain any projects: {solutionFile}");

            return list;
            
            (string? Folder, string ProjectDir, string ProjectFile, bool IsTest) ParseProject(string? folderName, XElement projectElement)
            {
                var projectPathRelative = TrimOrNull_Static(GetAttr(projectElement, "Path"));
                if (projectPathRelative == null) throw new ApplicationException($"projectElement {projectElement.Name} does not contain a Path attribute");
                
                LogV($"{nameof(projectPathRelative)}: {projectPathRelative}");
                projectPathRelative = projectPathRelative.Replace('\\', Path.DirectorySeparatorChar);
                LogV($"{nameof(projectPathRelative)}: {projectPathRelative}");

                
                var projectType = projectPathRelative.Split('\\').Select(TrimOrNull_Static).Where(o => o != null).FirstOrDefault();
                if (projectType == null) throw new ApplicationException($"Invalid project location: {projectPathRelative}");
                LogV($"{nameof(projectType)}: {projectType}");
                
                bool isTest;
                
                if (projectType.StartsWith(dirSrc, StringComparison.OrdinalIgnoreCase)) isTest = false;
                else if (projectType.StartsWith(dirTest, StringComparison.OrdinalIgnoreCase)) isTest = true;
                else throw new ApplicationException($"Invalid project location: {projectPathRelative}, expecting '{dirSrc}' or '{dirTest}' but was '{projectType}'");
                LogV($"{nameof(isTest)}: {isTest}");

                
                var solutionDirAndProjectPath = Path.Combine(solutionDir, projectPathRelative);
                LogV($"{nameof(solutionDirAndProjectPath)}: {solutionDirAndProjectPath}");
                
                var projectFile = new FileInfo(solutionDirAndProjectPath);
                if (!projectFile.Exists) throw new FileNotFoundException("Referenced project file not found", projectFile.FullName);
                
                return (folderName, projectFile.Directory!.FullName, projectFile.FullName, isTest);
            }

            static string? GetAttr(XElement element, string attributeName) => element
                .Attributes()
                .Where(o => NameIs(o.Name, attributeName))
                .Select(o => o.Value)
                .Select(TrimOrNull_Static)
                .Where(o => o != null)
                .FirstOrDefault();

            static bool NameIs(XName xname, string name) => SC.Equals(xname.LocalName, name) || SC.Equals(xname.ToString(), name);
        }
    }

    private IEnumerable<(string? Folder, string ProjectDir, string ProjectFile, bool IsTest)> ProjectsNuget
    {
        get
        {
            var projects = Projects.ToList();
            
            if (projects.Count == 0)    yield break;
            
            // ReSharper disable once RedundantAssignment
            var p = projects[0];

            var included = SplitAndTrim_Static(ProjectsToIncludeNuget);
            
            foreach (var project in projects)
            {
                p = project;
                if (ShouldIncludeProject()) yield return p;
            }

            yield break;
            
            bool ShouldIncludeProject()
            {
                if (p.IsTest) return false;
                
                var projectName = Path.GetFileNameWithoutExtension(p.ProjectFile);
                var projectNameParts = projectName.Split('.');
                
                foreach (var include in included)
                {
                    if (SC.Equals(include, projectName)) return true; // exact match
                    if (projectNameParts.Any(projectNamePart => SC.Equals(projectNamePart, include))) return true; // any project name part matches
                    if (p.Folder != null && SC.Equals(p.Folder, include)) return true; // folder matches
                }

                return false;
            }
        }
    }



    
    
}

/// <summary>
/// Actions
/// </summary>
public partial class Builder
{
    public void CleanBuildDirs()
    {
        CleanBuildDir(BuildDir, nameof(BuildDir));
        CleanBuildDir(BuildDirDll, nameof(BuildDirDll));
        CleanBuildDir(BuildDirZip, nameof(BuildDirZip));
        CleanBuildDir(BuildDirNuget, nameof(BuildDirNuget));
    }

    private void CleanBuildDir(string? path, string propertyName)
    {
        var d = RequiredProperty(ResolveDirectory(RequiredProperty(path, propertyName)), propertyName);

        var extensions = SplitAndTrim_Static((FileExtensionsToCleanInBuildDirs ?? string.Empty).Replace(".", string.Empty));
#pragma warning disable CA2208
        if (extensions.Length == 0) throw new ArgumentNullException(nameof(FileExtensionsToCleanInBuildDirs), $"{nameof(FileExtensionsToCleanInBuildDirs)} is not set");
#pragma warning restore CA2208
        
        LogI($"Cleaning [{propertyName}]: {d.Name}");
        LogV($"Cleaning [{propertyName}]: {d.FullName}");
        if (!d.Exists)
        {
            LogV($"  Creating [{propertyName}]: {d.FullName}");
            Directory.CreateDirectory(d.FullName);
        }

        var filesToDelete = GetFilesWithExtensions(d, extensions);
        var exceptions = new List<Exception>();
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
                    try
                    {
                        if (fi.IsReadOnly) fi.IsReadOnly = false;
                        fi.Delete();
                        LogV($"    success");
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
            }
            catch (Exception e)
            {
                LogW($"    Error deleting file: {fi.FullName}", e);
            }
        }

        if (exceptions.Count > 0)
        {
            LogE($"Cleaning [{propertyName}] failed: {d.FullName}");
            throw new AggregateException(exceptions);
        }
        
        LogV($"Cleaning [{propertyName}] complete: {d.FullName}");
    }
    
    public void CleanProjectDirs()
    {
        LogI($"Cleaning [{nameof(Projects)}]");
        LogV($"Cleaning [{nameof(Projects)}] Started");

        foreach (var projectDir in Projects.Select(o => o.ProjectDir))
        {
            LogI($"  Cleaning [{nameof(Projects)}]: {new DirectoryInfo(projectDir).Name}");
            LogV($"  Cleaning [{nameof(Projects)}] Started: {projectDir}");
            foreach (var dirToDelete in GetSubDirectories(projectDir, "bin", "obj"))
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

            LogV($"  Cleaning [{nameof(Projects)}] Complete: {projectDir}");
        }
        
        LogV($"Cleaning [{nameof(Projects)}] Complete");
    }

    public void Zip()
    {
        LogW($"{nameof(Zip)}() not implemented");

        //using var zip = ZipFile.Open("test.zip", ZipArchiveMode.Create);
        //zip.CreateEntryFromFile(@"c:\something.txt", "data/path/something.txt");
    }

    private void CopyNugetFilesProjectSingle(FileInfo source, FileInfo target)
    {
        LogV($" |   Copying File: {source.FullName}");
        if (target.Exists)
        {
            LogV($" |     Deleting existing file: {target.FullName}");
            if (DebugDoNotDelete)
            {
                LogV($" |     !{nameof(DebugDoNotDelete)}={DebugDoNotDelete}  skipping delete+copy of existing file");
                return;
            }

            try
            {
                target.Delete();
                LogV($" |     success");
            }
            catch (Exception e)
            {
                LogE($" |     error deleting existing file: {target.FullName}", e);
                return;
            }
        }
        
        source.CopyTo(target.FullName, false);
        LogV($" |     Success: {source.FullName}  -->  {target.FullName}");
        LogI($"  {target.Name}");
    }

    private void CopyNugetFilesProject(string projectDirString, DirectoryInfo destinationDirectory)
    {
        var dirProject = ResolveDirectoryRequired(projectDirString, nameof(Projects));
        var dirProjectBin = GetSubDirectories(dirProject, "bin").FirstOrDefault();
        if (dirProjectBin == null)
        {
            LogW($" | Could not locate 'bin' directory for project: {dirProject.FullName}");
            return;
        }

        var dirProjectBinType = GetSubDirectories(dirProjectBin, BuildType).FirstOrDefault();
        if (dirProjectBinType == null)
        {
            LogW($" | Could not locate '{BuildType}' directory in project bin directory: {dirProjectBin.FullName}");
            return;
        }

        var files = GetFilesWithExtensions(dirProjectBinType, "nupkg", "snupkg").Where(o => !o.Name.EndsWith(".symbols.nupkg", SCN)).ToArray();
        if (files.Length == 0)
        {
            LogW($" | Could not locate any .nupkg files in project directory: {dirProjectBinType.FullName}");
            return;
        }

        if (!Directory.Exists(destinationDirectory.FullName))
        {
            LogE($"Destination directory {nameof(BuildDirNuget)} does not exist: {destinationDirectory.FullName}");
            throw new DirectoryNotFoundException(destinationDirectory.FullName);
        }
        
        
        LogV($" | Copying {files.Length} files for project [{nameof(BuildDirNuget)}]: {dirProject.Name}");
        foreach (var file in files)
        {
            var destinationFile = ResolveFile(Path.Join(destinationDirectory.FullName, file.Name))!;
            CopyNugetFilesProjectSingle(file, destinationFile);
        }
        
    }

    public void CopyNugetFiles()
    {
        var buildDirNuget = RequiredProperty(ResolveDirectory(BuildDirNuget), nameof(BuildDirNuget));
        
        LogI($"Copying Nuget Files [{nameof(BuildDirNuget)}]: {buildDirNuget.FullName}");
        LogV($"Copying Nuget Files [{nameof(BuildDirNuget)}] Started: {buildDirNuget.FullName}");

        if (buildDirNuget.Exists)
        {
            LogV($" | Deleting Directory [{nameof(BuildDirNuget)}]: {buildDirNuget.FullName}");
            if (DebugDoNotDelete)
            {
                LogV($"  |  !{nameof(DebugDoNotDelete)}={DebugDoNotDelete}  skipping delete of directory");
            }
            else
            {
                try
                {
                    buildDirNuget.Delete(recursive: true);
                    LogV($" |  Success");
                }
                catch (Exception e)
                {
                    LogW($" |  Error deleting directory: {buildDirNuget.FullName}", e);
                    throw;
                }
            }
        }

        if (!buildDirNuget.Exists) buildDirNuget.Create();
        
        foreach (var p in ProjectsNuget)
        {
            CopyNugetFilesProject(p.ProjectDir, buildDirNuget);
        }
        
        LogV($"Copying Nuget Files [{nameof(BuildDirNuget)}] Complete: {buildDirNuget.FullName}");
    }


}


/// <summary>
/// Static
/// </summary>
public partial class Builder
{

    //private static bool Eq(string? x, string? y, bool trim = false) => trim ? string.Equals(TrimOrNull_Static(x), TrimOrNull_Static(y), SCN) : string.Equals(x, y, SCN);

    //public static IEnumerable<string> TrimOrNull_Internal(IEnumerable<string?> enumerable, bool removeNull = true) => enumerable.Select(TrimOrNull_Static).Where(o => !removeNull || o != null).Select(o => o!);

    public static string? ResolvePath(object? pathObj)
    {
        var path = TrimOrNull_Static(pathObj);
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

    private static FileInfo? ResolveFile(object? pathObj)
    {
        var path = ResolvePath(pathObj);
        if (path == null) return null;
        if (File.Exists(path)) return new FileInfo(path);
        if (Directory.Exists(path)) throw new FileNotFoundException($"Path is a Directory not a File: {path}", path);
        return new FileInfo(path);
    }
    
    private static FileInfo ResolveFileRequired(object? pathObj, string propertyName)
    {
        var f = RequiredProperty(ResolveFile(pathObj), propertyName);
        if (!f.Exists) throw new FileNotFoundException($"{propertyName} does not exist: {f.FullName}", f.FullName);
        return f;
    }
    
    private static DirectoryInfo? ResolveDirectory(object? pathObj)
    {
        var path = ResolvePath(pathObj);
        if (path == null) return null;
        if (File.Exists(path)) throw new DirectoryNotFoundException($"Path is a File not a Directory: {path}");
        if (Directory.Exists(path)) return new DirectoryInfo(path);
        return new DirectoryInfo(path);
    }
    
    private static DirectoryInfo ResolveDirectoryRequired(object? pathObj, string propertyName)
    {
        var f = RequiredProperty(ResolveDirectory(pathObj), propertyName);
        if (!f.Exists) throw new DirectoryNotFoundException($"{propertyName} does not exist: {f.FullName}");
        return f;
    }

    private static List<DirectoryInfo> GetSubDirectories_NotNull(DirectoryInfo dir, params string[] names) => dir
        .GetDirectories()
        .Select(o => (Dir: o, DirName: TrimOrNull_Static(o.Name)))
        .Where(o => o.DirName != null)
        .Where(d => names.Any(name => SC.Equals(d.DirName, name)))
        .Select(d => d.Dir)
        .ToList();

    private static List<DirectoryInfo> GetSubDirectories(DirectoryInfo? dir, params string[] names)
    {
        ArgumentNullException.ThrowIfNull(dir);
        return GetSubDirectories_NotNull(dir, names);
    }
    
    private static List<DirectoryInfo> GetSubDirectories(string? dir, params string[] names)
    {
        ArgumentNullException.ThrowIfNull(dir);
        return GetSubDirectories(ResolveDirectory(dir), names);
    }


    private static DirectoryInfo GetParentDirectory(object fileOrDirectory)
    {
        const string exceptionPrefix = "Could not determine parent directory for";
        switch (fileOrDirectory)
        {
            case FileInfo fi: return fi.Directory ?? throw new DirectoryNotFoundException($"{exceptionPrefix} file: {fi.FullName}");
            case DirectoryInfo di: return di.Parent ?? throw new DirectoryNotFoundException($"{exceptionPrefix} directory: {di.FullName}");
            case string s:
            {
                var ss = ResolvePath(s);
                if (ss == null) throw new IOException($"Could not resolve path: {s}");
                if (File.Exists(ss)) return GetParentDirectory(new FileInfo(ss));
                if (Directory.Exists(ss)) return GetParentDirectory(new DirectoryInfo(ss));
                throw new ArgumentException($"{exceptionPrefix} string: {s}", nameof(fileOrDirectory));
            }
            default: throw new ArgumentException($"Invalid type: {fileOrDirectory.GetType().FullName}", nameof(fileOrDirectory));
        }
    }
    
    private static bool HasExtension(FileInfo fi, params string?[] exts)
    {
        var fiExt = ParseExtension(fi.Extension);
        if (fiExt == null) return false;
        return exts
                .Select(ParseExtension)
                .Where(o => o != null)
                .Any(ext => string.Equals(fiExt, ext, SCN))
            ;
        
        static string? ParseExtension(string? ext) => TrimOrNull_Static(TrimOrNull_Static(ext)?.Trim('.'));
    }

    private static FileInfo[] GetFilesWithExtensions(DirectoryInfo d, params string?[] exts) => d.GetFiles().Where(o => HasExtension(o, exts)).ToArray();

    private static string[] SplitAndTrim_Static(string? s) => (s ?? string.Empty).Split(' ', '|', ';').Select(TrimOrNull_Static).Where(o => o != null).Select(o => o!).ToArray();

    private static T RequiredProperty<T>(T? value, [CallerArgumentExpression(nameof(value))] string? propertyName = null) => value ?? throw new ArgumentNullException(propertyName, $"Property {propertyName} has not been set");
    
    private static string? TrimOrNull_Static(object? o)
    {
        var s = o?.ToString();
        if (s == null) return null;
        s = s.Trim();
        if (s.Length == 0) return null;
        return s;
    }

}

/// <summary>
/// ToString
/// </summary>
public partial class Builder
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetType().FullName!.Split('_')[0]); // Might add _ suffix in PowerShell to class 

        static string ItemToString(object? o)
        {
            var s = TrimOrNull_Static(o);

            if (s == null) return string.Empty;
            try
            {
                var path = ResolvePath(s);
                if (File.Exists(path))
                {
                    s = new FileInfo(path).Name + "  -->  " + path;
                }
                else if (Directory.Exists(path))
                {
                    s = new DirectoryInfo(path).Name + "  -->  " + path;
                }
            }
            catch (Exception) { /* ignore */ }

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
