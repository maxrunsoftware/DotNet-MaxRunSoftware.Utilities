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

namespace MaxRunSoftware.Utilities.Ftp;

public abstract class FtpClientBase : IFtpClient
{
    protected FtpClientBase(ILoggerFactory loggerFactory)
    {
        log = loggerFactory.CreateLogger<FtpClientBase>();
        serverInfo = Lzy.Create(ServerInfoGet);
        directorySeparator = Lzy.Create(DirectorySeparatorGet);
    }

    private readonly ILogger log;
    protected const string LOG_ATTEMPT = "Attempting";
    protected const string LOG_COMPLETE = "Complete";
    protected const string LOG_SUCCESS = "Success";
    protected const string LOG_FAILED = "Failed";
    protected const string LOG_IGNORED = "Ignored";

    #region ServerInfo

    public string? ServerInfo => serverInfo.Value;
    private readonly Lzy<string?> serverInfo;
    protected abstract string? ServerInfoGet();

    #endregion ServerInfo

    #region WorkingDirectory

    private string? workingDirectoryCached;

    public string WorkingDirectory
    {
        get
        {
            if (workingDirectoryCached == null)
            {
                var ds = DirectorySeparator;
                workingDirectoryCached = ds + WorkingDirectoryGet().TrimStart(ds).TrimEnd(ds);
            }

            return workingDirectoryCached;
        }
        set
        {
            var workingDirectoryCurrent = WorkingDirectory;
            if (workingDirectoryCurrent.EqualsOrdinal(value))
            {
                log.LogDebugMethod(new(value), LOG_IGNORED + " for '{Directory}' because we are already in that directory", value);
            }
            else
            {
                workingDirectoryCached = null;
                log.LogInformation("Changing " + nameof(WorkingDirectory) + " from '{WorkingDirectoryCurrent}' to '{WorkingDirectoryNew}'", workingDirectoryCurrent, value);
                WorkingDirectorySet(value);
                log.LogDebug(nameof(WorkingDirectory) + " is now '{Directory}'", WorkingDirectory);
            }
        }
    }

    protected abstract string WorkingDirectoryGet();
    protected abstract void WorkingDirectorySet(string directory);

    #endregion WorkingDirectory

    #region DirectorySeparator

    public string DirectorySeparator => directorySeparator.Value;
    private readonly Lzy<string> directorySeparator;
    protected abstract string DirectorySeparatorGet();

    #endregion DirectorySeparator

    #region GetFile

    protected abstract void GetFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress);

    public void GetFile(string remoteFile, Stream stream, Action<FtpClientProgress>? handlerProgress, bool flushStream)
    {
        log.LogDebugMethod(new(remoteFile), LOG_ATTEMPT + " downloading {RemoteFile}", remoteFile);

        var methodInfo = new CallerInfoMethod(remoteFile).OffsetLineNumber(1);
        handlerProgress ??= progress => log.LogDebugMethod(methodInfo, " {Progress}", progress);

        var streamCounted = new StreamCounted(stream); // Don't USING because we don't want to dispose underlying stream
        GetFileInternal(remoteFile, streamCounted, handlerProgress);
        if (flushStream) streamCounted.Flush();

        var dataLength = streamCounted.BytesWritten.ToStringCommas();
        log.LogDebugMethod(new(remoteFile), LOG_SUCCESS + " downloading {DataLength} bytes", dataLength);
        log.LogInformation("Downloaded {DataLength} bytes {RemoteFile}", dataLength, remoteFile);
    }

    #endregion GetFile

    #region PutFile

    protected abstract void PutFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress);

    public void PutFile(string remoteFile, Stream stream, Action<FtpClientProgress>? handlerProgress)
    {
        //remoteFile = ToAbsolutePath(remoteFile);
        var dataLength = "?";
        try
        {
            dataLength = stream.Length.ToStringCommas();
        }
        catch (NotSupportedException e)
        {
            log.LogDebugMethod(new(remoteFile), e, "Could not determine {StreamType} length", stream.GetType().NameFormatted());
        }

        log.LogDebugMethod(new(remoteFile), LOG_ATTEMPT + " uploading {DataLength} bytes to {RemoteFile}", dataLength, remoteFile);

        var methodInfo = new CallerInfoMethod(remoteFile).OffsetLineNumber(1);
        handlerProgress ??= progress => log.LogDebugMethod(methodInfo, " {Progress}", progress);

        var streamCounted = new StreamCounted(stream); // Don't USING because we don't want to dispose underlying stream
        PutFileInternal(remoteFile, streamCounted, handlerProgress);

        dataLength = streamCounted.BytesRead.ToStringCommas();
        log.LogDebugMethod(new(remoteFile), LOG_SUCCESS + " uploading {DataLength} bytes to {RemoteFile}", dataLength, remoteFile);
        log.LogInformation("Uploaded {FileLength} bytes --> {RemoteFile}", dataLength, remoteFile);
    }

    #endregion PutFile

    #region DeleteFile

    protected abstract void DeleteFileInternal(string remoteFile);

    public void DeleteFile(string remoteFile)
    {
        remoteFile = ToAbsolutePath(remoteFile);

        log.LogTraceMethod(new(remoteFile), LOG_ATTEMPT + " delete of remote file {RemoteFile}", remoteFile);
        DeleteFileInternal(remoteFile);
        log.LogTraceMethod(new(remoteFile), LOG_SUCCESS + " delete of remote file {RemoteFile}", remoteFile);
        log.LogInformation("Deleted remote file {RemoteFile}", remoteFile);
    }

    #endregion DeleteFile

    #region CreateDirectory

    public FtpClientRemoteObject CreateDirectory(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        var o = GetObject(remotePath);
        if (o != null)
        {
            if (o.Type == FtpClientRemoteObjectType.Directory) return o;
            throw new ArgumentException($"Object {remotePath} already exists but is a {o.Type} not a {FtpClientRemoteObjectType.Directory}", nameof(remotePath));
        }

        CreateDirectoryInternal(remotePath);

        o = GetObject(remotePath);
        if (o == null)
        {
            throw new ArgumentException($"Successfully created directory {remotePath} but could not retrieve directory object", nameof(remotePath));
        }

        log.LogTraceMethod(new(remotePath), LOG_SUCCESS);
        return o;
    }

    protected abstract void CreateDirectoryInternal(string remotePath);

    #endregion CreateDirectory

    public bool DeleteDirectory(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        var obj = GetObject(remotePath);
        if (obj == null) return false;

        try
        {
            DeleteDirectoryInternal(remotePath);
        }
        catch (Exception e)
        {
            log.LogDebugMethod(new(remotePath), e, LOG_FAILED + " " + nameof(DeleteDirectoryInternal) + " failed, retrying with recursive delete");

            // Maybe there are files/dirs in the directory that need to be deleted first
            bool success;
            try
            {
                var remoteDirectory = ParseDirectory(remotePath);
                var objsAll = ListObjects(remotePath, true, null).ToArray();
                var objsChildren = objsAll.Where(o => IsChild(remoteDirectory, o)).ToArray();
                foreach (var file in objsChildren.Where(o => o.Type != FtpClientRemoteObjectType.Directory))
                {
                    DeleteFile(file.NameFull);
                }

                foreach (var dir in objsChildren.Where(o => o.Type == FtpClientRemoteObjectType.Directory).OrderByDescending(o => o.NameFull.Length))
                {
                    DeleteDirectoryInternal(dir.NameFull);
                }

                DeleteDirectoryInternal(remoteDirectory.NameFull);
                success = true;
            }
            catch (Exception ee)
            {
                log.LogDebugMethod(new(remotePath), ee, LOG_FAILED);
                success = false;
            }

            if (!success) throw;
        }

        return true;
    }

    protected bool IsChild(FtpClientRemoteObject parent, FtpClientRemoteObject child)
    {
        if (parent.Equals(child)) return false;
        if (child.NameFull.Length <= parent.NameFull.Length) return false;
        if (!child.NameFull.StartsWith(parent.NameFull)) return false;
        return true;
    }

    protected FtpClientRemoteObject ParseDirectory(string remotePath)
    {
        var wdCurrent = WorkingDirectory;
        string? wdNew = null;
        try
        {
            WorkingDirectory = remotePath;
            wdNew = WorkingDirectory;
            var name = wdNew.Split(DirectorySeparator).Where(o => !string.IsNullOrWhiteSpace(o)).ToArray().LastOrDefault();
            if (name == null)
            {
                // We are trying to get ROOT dir
                return new(string.Empty, DirectorySeparator, FtpClientRemoteObjectType.Directory);
            }
            else
            {
                return new(name, wdNew, FtpClientRemoteObjectType.Directory);
            }
        }
        finally
        {
            try
            {
                WorkingDirectory = wdCurrent;
            }
            catch (Exception ee)
            {
                log.LogError(ee, "Could not change " + nameof(WorkingDirectory) + " back to {WorkingDirectoryOld} from {WorkingDirectoryNew}", wdCurrent, wdNew ?? remotePath);
                Dispose(); // Unstable state because couldn't change WorkingDirectory back
                throw;
            }
        }
    }

    protected abstract void DeleteDirectoryInternal(string remotePath);

    #region ListObjects

    private IComparer<FtpClientRemoteObject>? objectComparer;
    public IComparer<FtpClientRemoteObject> ObjectComparer => objectComparer ??= new FtpClientRemoteObjectComparer(DirectorySeparator);

    protected abstract void ListObjectsInternal(string remotePath, List<FtpClientRemoteObject> list);

    protected virtual bool ListObjectsIsIncluded(FtpClientRemoteObject obj) =>
        obj is not { Type: FtpClientRemoteObjectType.Directory, Name: "." or ".." };

    public virtual IEnumerable<FtpClientRemoteObject> ListObjects(string remotePath, bool recursive, Func<string, Exception, bool>? handlerException)
    {
        log.LogTraceMethod(new(remotePath, handlerException), LOG_ATTEMPT);

        var remotePathObj = GetObject(remotePath);
        if (remotePathObj == null) throw new DirectoryNotFoundException($"Remote directory {remotePath} does not exist");
        if (remotePathObj.Type != FtpClientRemoteObjectType.Directory) throw new DirectoryNotFoundException($"Remote object {remotePathObj.NameFull} is not a {FtpClientRemoteObjectType.Directory} it is a {remotePathObj.Type}");
        remotePath = remotePathObj.NameFull;
        log.LogTraceMethod(new(remotePath, handlerException), LOG_ATTEMPT + " (AbsolutePath: {Path})", remotePath);
        Debug.Assert(remotePath.StartsWith(DirectorySeparator));

        var stack = new Stack<string>();
        var checkedDirs = new HashSet<string>();
        stack.Push(remotePath);

        var countObjectsFound = 0;
        while (stack.Count > 0)
        {
            var remoteDirectory = stack.Pop();
            if (!checkedDirs.Add(remoteDirectory)) continue;
            log.LogDebugMethod(new(remotePath, handlerException), "Listing {RemoteDirectory}", remoteDirectory);
            var items = new List<FtpClientRemoteObject>();
            var shouldContinue = true;
            try
            {
                ListObjectsInternal(remoteDirectory, items);
                items.Sort(ObjectComparer);
                if (log.IsEnabled(LogLevel.Trace))
                {
                    log.LogTrace("Retrieved directory listing of {ObjectsCount} objects for directory: {RemoteDirectory}", items.Count, remoteDirectory);
                    for (var i = 0; i < items.Count; i++)
                    {
                        var notIncludedMarker = ListObjectsIsIncluded(items[i]) ? " " : "-";
                        log.LogTrace("  {NotIncludedMarker}[{RunningCount}] {FileSystemObject}", notIncludedMarker, Util.FormatRunningCount(i, items.Count), items[i]);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogDebugMethod(new(remotePath, handlerException), e, "Error listing {RemoteDirectory}", remoteDirectory);
                items.Clear(); // don't allow any results for dir if listing failed
                if (handlerException == null) throw;
                shouldContinue = handlerException(remoteDirectory, e);
            }

            if (!shouldContinue)
            {
                log.LogTraceMethod(new(remotePath, handlerException), "Breaking out of listing early because of exception listing {RemoteDirectory}", remoteDirectory);
                yield break;
            }

            foreach (var item in items.Where(ListObjectsIsIncluded))
            {
                if (item.Type == FtpClientRemoteObjectType.Directory && !checkedDirs.Contains(item.NameFull))
                {
                    if (recursive) stack.Push(item.NameFull);
                }

                countObjectsFound++;
                yield return item;
            }
        }

        log.LogDebugMethod(new(remotePath, handlerException), LOG_COMPLETE + ", found {CountObjects} items", countObjectsFound);
    }

    #endregion ListObjects

    #region GetObject

    public virtual FtpClientRemoteObject? GetObject(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        var obj = GetObjectInternal(remotePath);
        if (obj != null)
        {
            log.LogTraceMethod(new(remotePath), LOG_SUCCESS + " {Path}", obj.NameFull);
        }
        else
        {
            log.LogTraceMethod(new(remotePath), LOG_FAILED + " object does not exist {Path}", remotePath);
        }

        return obj;
    }

    protected abstract FtpClientRemoteObject? GetObjectInternal(string remotePath);

    #endregion GetObject

    #region GetAbsolutePath

    protected virtual bool IsAbsolutePath(string remotePath)
    {
        if (!remotePath.StartsWith(DirectorySeparator)) return false;
        var pathParts = remotePath.Split(DirectorySeparator);
        if (pathParts.Any(o => o is "." or "..")) return false;
        return true;
    }


    public string GetAbsolutePath(string remotePath)
    {
        try
        {
            var remotePathAbsolute = GetAbsolutePathInternal(remotePath);
            if (remotePathAbsolute != null) return remotePathAbsolute;
        }
        catch (NotImplementedException) { }

        if (!remotePath.StartsWith(DirectorySeparator))
        {
            // relative path to current working directory
            remotePath = WorkingDirectory + DirectorySeparator + remotePath;
        }

        if (IsAbsolutePath(remotePath)) return remotePath;

        var remotePathParts = remotePath.Split(DirectorySeparator).Where(o => o.Length > 0).ToArray();
        var remotePathReassembled = new Stack<string>();
        foreach (var remotePathPart in remotePathParts)
        {
            if (remotePathPart == ".") continue;
            if (remotePathPart == "..")
            {
                if (remotePathReassembled.Count > 0) remotePathReassembled.Pop();
                continue;
            }

            remotePathReassembled.Push(remotePathPart);
        }

        if (remotePathReassembled.Count == 0) return DirectorySeparator; // we ended up at root
        remotePathParts = remotePathReassembled.Reverse().ToArray();
        return DirectorySeparator + remotePathParts.ToStringDelimited(DirectorySeparator);
    }

    protected abstract string? GetAbsolutePathInternal(string remotePath);

    #endregion GetAbsolutePath

    #region CreateFileSystemObject

    protected virtual FtpClientRemoteObject? CreateFileSystemObject(string? name, string? nameFull, FtpClientRemoteObjectType type)
    {
        if (nameFull != null && nameFull.Length == 0) nameFull = null;
        if (name == null && nameFull == null) return null;

        // is root? "/"
        if (name == DirectorySeparator || nameFull == DirectorySeparator) return new(DirectorySeparator, DirectorySeparator, type);

        if (name != null && name.Contains(DirectorySeparator)) throw new ArgumentException($"Argument '{nameof(name)}' with value '{name}' cannot contain {nameof(DirectorySeparator)} '{DirectorySeparator}'", nameof(name));

        if (name != null && nameFull == null)
        {
            nameFull = WorkingDirectory + DirectorySeparator + name;
        }

        if (nameFull != null)
        {
            while (nameFull.Length > 0 && nameFull.EndsWith(DirectorySeparator)) nameFull = nameFull.RemoveRight();

            if (!nameFull.StartsWith(DirectorySeparator)) nameFull = DirectorySeparator + nameFull;
        }

        if (name == null && nameFull != null)
        {
            var (_, right) = nameFull.SplitOnLast(DirectorySeparator);
            name = string.IsNullOrEmpty(right) ? nameFull : right;
        }

        if (name == null || nameFull == null)
        {
            var infoMethod = new CallerInfoMethod();
            var sb = new StringBuilder();
            sb.Append("Could not determine " + (name == null ? nameof(name) : nameof(nameFull)) + $" {infoMethod.MemberName}(");
            for (var i = 0; i < infoMethod.Args.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var (argName, argValue) = infoMethod.Args[i];
                if (!string.IsNullOrWhiteSpace(argName)) sb.Append($"{argName}: ");
                var v = argValue switch
                {
                    null => "null",
                    string argValueString => "\"" + argValueString + "\"",
                    _ => argValue.ToString() ?? string.Empty,
                };

                sb.Append(v);
            }

            sb.Append(')');

            throw new ArgumentException(sb.ToString(), name == null ? nameof(name) : nameof(nameFull));
        }

        return new(name, nameFull, type);
    }

    #endregion CreateFileSystemObject

    #region Dispose

    private readonly SingleUse isDisposed = new();
    public bool IsDisposed => isDisposed.IsUsed;
    public void Dispose()
    {
        if (!isDisposed.TryUse()) return;
        log.LogTraceMethod(new(), LOG_ATTEMPT);
        try
        {
            DisposeInternal();
        }
        catch (Exception e)
        {
            log.LogWarningMethod(new(), e, LOG_FAILED);
        }

        log.LogTraceMethod(new(), LOG_SUCCESS);
    }

    protected abstract void DisposeInternal();

    #endregion Dispose

    #region Helpers

    /*
    protected static string CombinePath(string[] remotePathParts, string remoteFile = null)
    {
        if (remoteFile.TrimOrNull() == null) remoteFile = null;

        if (remotePathParts.Length == 0)
        {
            return remoteFile == null ? string.Empty : "/" + remoteFile;
        }

        var sb = new StringBuilder();
        sb.Append("/");
        foreach (var remotePathPart in remotePathParts)
        {
            sb.Append(remotePathPart);
            sb.Append("/");
        }
        if (remoteFile == null)
        {
            sb.Remove(sb.Length - 1, 1);
        }
        else
        {
            sb.Append(remoteFile);
        }

        return sb.ToString();
    }
    */


    protected virtual string ToAbsolutePath(string path)
    {
        if (!path.StartsWith('/'))
        {
            // relative path or just name
            var wd = WorkingDirectory;
            if (!wd.EndsWith('/')) path = "/" + path;

            path = wd + path;
        }

        var pathParts = path.Split('/').Where(part => part.TrimOrNull() != null).ToArray();
        var stack = new Stack<string>();
        foreach (var pathPart in pathParts)
        {
            if (pathPart.TrimOrNull() == ".") { }
            else if (pathPart.TrimOrNull() == "..")
            {
                if (stack.Count > 0) stack.Pop();
            }
            else { stack.Push(pathPart); }
        }

        path = "/" + stack.ToArray().Reverse().ToStringDelimited("/");
        return path;
    }

    #endregion Helpers
}
