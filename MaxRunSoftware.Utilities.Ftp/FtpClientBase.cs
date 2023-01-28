// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Ftp;

public abstract class FtpClientBase : IFtpClient
{
    protected FtpClientBase(ILoggerProvider loggerProvider)
    {
        log = loggerProvider.CreateLogger<FtpClientBase>();
        serverInfo = Lzy.Create(GetServerInfo);
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
    protected abstract string? GetServerInfo();

    #endregion ServerInfo

    #region WorkingDirectory

    public string WorkingDirectory
    {
        get
        {
            var wd = GetWorkingDirectory();
            wd = RemoveTrailingDirectorySeparator(wd);

            wd = RemoveLeadingDirectorySeparator(wd);

            return DirectorySeparator + wd;
        }
        set
        {
            var wdOld = WorkingDirectory;
            if (StringComparer.Ordinal.Equals(wdOld, value))
            {
                log.LogDebugMethod(new(value), LOG_IGNORED + " for '{Directory}' because we are already in that directory", value);
            }
            else
            {
                log.LogInformation("Changing " + nameof(WorkingDirectory) + " from '{DirectoryOld}' to '{DirectoryNew}'", wdOld, value);
                SetWorkingDirectory(value);
                if (log.IsEnabled(LogLevel.Debug)) log.LogDebug(nameof(WorkingDirectory) + " is now '{Directory}'", GetWorkingDirectory());
            }

        }
    }

    protected string RemoveLeadingDirectorySeparator(string path)
    {
        while (path.Length > 0 && path.StartsWith(DirectorySeparator)) path = path.RemoveLeft();
        return path;
    }
    protected string RemoveTrailingDirectorySeparator(string path)
    {
        while (path.Length > 0 && path.EndsWith(DirectorySeparator)) path = path.RemoveRight();
        return path;
    }

    protected bool IsAbsolutePath(string path) => path.StartsWith(DirectorySeparator);

    protected string GetAbsolutePath(string path) => IsAbsolutePath(path) ? path : (WorkingDirectory + DirectorySeparator + path);

    private string? directorySeparator;
    public string DirectorySeparator => directorySeparator ??= GetDirectorySeparator();

    protected abstract string GetDirectorySeparator();

    protected abstract string GetWorkingDirectory();
    protected abstract void SetWorkingDirectory(string directory);

    #endregion WorkingDirectory

    #region GetFile

    protected abstract void GetFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress);

    public byte[] GetFile(string remoteFile, Action<FtpClientProgress>? handlerProgress)
    {
        //remoteFile = ToAbsolutePath(remoteFile);
        //if (!ExistsFile(remoteFile)) throw new FileNotFoundException($"Remote file {remoteFile} does not exist", remoteFile);
        log.LogDebugMethod(new(remoteFile), LOG_ATTEMPT + " downloading {RemoteFile}", remoteFile);

        var methodInfo = new CallerInfoMethod(remoteFile).OffsetLineNumber(1);
        handlerProgress ??= progress => log.LogDebugMethod(methodInfo, " {Progress}", progress);

        byte[] data;
        using (var localStream = new MemoryStream())
        {
            GetFile(remoteFile, localStream, handlerProgress);
            localStream.Flush();
            data = localStream.ToArray();
        }

        var dataLength = data.Length.ToStringCommas();
        log.LogDebugMethod(new(remoteFile), LOG_SUCCESS + " downloading {DataLength} byte file", dataLength);
        log.LogInformation("Downloaded {DataLength} byte file {RemoteFile}", dataLength, remoteFile);
        return data;
    }

    public void GetFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress)
    {
        localFile = Path.GetFullPath(localFile);
        log.LogDebugMethod(new(remoteFile, localFile), LOG_ATTEMPT + " downloading {RemoteFile}", remoteFile);

        if (File.Exists(localFile))
        {
            log.LogDebugMethod(new(remoteFile, localFile), "Deleting existing local file: {LocalFile}", localFile);
            File.Delete(localFile);
        }

        //remoteFile = ToAbsolutePath(remoteFile);
        //if (!ExistsFile(remoteFile)) throw new FileNotFoundException($"Remote file {remoteFile} does not exist", remoteFile);

        var methodInfo = new CallerInfoMethod(remoteFile, localFile).OffsetLineNumber(1);
        handlerProgress ??= progress => log.LogDebugMethod(methodInfo, " {Progress}", progress);

        using (var localStream = Util.FileOpenWrite(localFile))
        {
            GetFile(remoteFile, localStream, handlerProgress);
            localStream.Flush();
        }

        var fileLength = Util.FileGetLength(localFile).ToStringCommas();
        log.LogDebugMethod(new(remoteFile, localFile), LOG_SUCCESS + " downloading {FileLength} byte file", fileLength);
        log.LogInformation("Downloaded {FileLength} byte file {RemoteFile} --> {LocalFile}", fileLength, remoteFile, localFile);
    }

    #endregion GetFile

    #region PutFile

    protected abstract void PutFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress);

    public void PutFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress)
    {
        localFile = Path.GetFullPath(localFile);
        localFile.CheckFileExists();

        var fileLength = Util.FileGetLength(localFile).ToStringCommas();
        //remoteFile = ToAbsolutePath(remoteFile);
        log.LogDebugMethod(new(remoteFile, localFile), LOG_ATTEMPT + " uploading {FileLength} byte file to {RemoteFile}", fileLength, remoteFile);

        var methodInfo = new CallerInfoMethod(remoteFile, localFile).OffsetLineNumber(1);
        handlerProgress ??= progress => log.LogDebugMethod(methodInfo, " {Progress}", progress);

        using (var localStream = Util.FileOpenRead(localFile))
        {
            PutFile(remoteFile, localStream, handlerProgress);
            //localFileStream.Flush();
        }

        log.LogDebugMethod(new(remoteFile, localFile), LOG_SUCCESS + " uploading {FileLength} byte file to {RemoteFile}", fileLength, remoteFile);
        log.LogInformation("Uploaded {FileLength} byte file {LocalFile} --> {RemoteFile}", fileLength, localFile, remoteFile);
    }

    public void PutFile(string remoteFile, byte[] data, Action<FtpClientProgress>? handlerProgress)
    {
        //remoteFile = ToAbsolutePath(remoteFile);
        log.LogDebugMethod(new(remoteFile, data), LOG_ATTEMPT + " uploading {DataLength} bytes to {RemoteFile}", data.Length.ToStringCommas(), remoteFile);

        var methodInfo = new CallerInfoMethod(remoteFile, data).OffsetLineNumber(1);
        handlerProgress ??= progress => log.LogDebugMethod(methodInfo, " {Progress}", progress);

        using (var localStream = new MemoryStream(data))
        {
            PutFile(remoteFile, localStream, handlerProgress);
            //localFileStream.Flush();
        }

        log.LogDebugMethod(new(remoteFile, data), LOG_SUCCESS + " uploading {DataLength} bytes to {RemoteFile}", data.Length.ToStringCommas(), remoteFile);
        log.LogInformation("Uploaded {FileLength} bytes --> {RemoteFile}", data.Length.ToStringCommas(), remoteFile);
    }

    #endregion PutFile

    #region DeleteFile

    protected abstract void DeleteFileSingle(string remoteFile);

    public void DeleteFile(string remoteFile)
    {
        remoteFile = ToAbsolutePath(remoteFile);
        var remoteFilesToDelete = new List<string>() { remoteFile };

        if (remoteFile.ContainsAny("*", "?")) // wildcard
        {
            var pathParts = remoteFile.Split("/").TrimOrNull().WhereNotNull().ToList();
            var remoteFileName = pathParts.PopTail();
            var path = string.Empty;
            if (pathParts.Count > 0) path = "/" + string.Join("/", pathParts);

            remoteFilesToDelete = ListObjects(path)
                .Where(o => o.Name.EqualsWildcard(remoteFileName))
                .Select(o => o.FullName)
                .ToList();

        }

        foreach (var file in remoteFilesToDelete)
        {
            log.LogDebugMethod(new(remoteFile), LOG_ATTEMPT + " delete of remote file {RemoteFile}", file);
            DeleteFileSingle(file);
            log.LogDebugMethod(new(remoteFile), LOG_SUCCESS + " delete of remote file {RemoteFile}", file);
            log.LogInformation("Deleted remote file {RemoteFile}", file);
        }
    }

    #endregion DeleteFile

    public abstract void CreateDirectory(string remotePath);
    public void DeleteDirectory(string remotePath)
    {
        log.LogDebugMethod(new (remotePath), LOG_ATTEMPT);
        try
        {
            DeleteDirectoryInternal(remotePath);
        }
        catch (Exception e)
        {
            log.LogDebugMethod(new(remotePath), e, LOG_FAILED + " " + nameof(DeleteDirectoryInternal) + " failed, retrying with recursive delete");

            // Maybe there are files/dirs in the directory that need to be deleted first
            try
            {
                var remoteDirectory = ParseDirectory(remotePath);
                var objsAll = ListObjectsRecursive(remotePath, null).ToArray();
                var objsChildren = objsAll.Where(o => IsChild(remoteDirectory, o)).ToArray();
                foreach (var file in objsChildren.Where(o => o.Type != FtpClientRemoteFileSystemObjectType.Directory))
                {
                    DeleteFile(file.FullName);
                }
                foreach (var dir in objsChildren.Where(o => o.Type == FtpClientRemoteFileSystemObjectType.Directory).OrderByDescending(o => o.FullName.Length))
                {
                    DeleteDirectoryInternal(dir.FullName);
                }
                DeleteDirectoryInternal(remoteDirectory.FullName);
                return;
            }
            catch (Exception ee)
            {
                log.LogDebugMethod(new(remotePath), ee, LOG_FAILED);
            }

            throw;
        }
    }


    protected bool IsChild(FtpClientRemoteFileSystemObject parent, FtpClientRemoteFileSystemObject child)
    {
        if (parent.Equals(child)) return false;
        if (child.FullName.Length <= parent.FullName.Length) return false;
        if (!child.FullName.StartsWith(parent.FullName)) return false;
        return true;
    }

    protected FtpClientRemoteFileSystemObject ParseDirectory(string remotePath)
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
                return new(string.Empty, DirectorySeparator, FtpClientRemoteFileSystemObjectType.Directory);
            }
            else
            {
                return new(name, wdNew, FtpClientRemoteFileSystemObjectType.Directory);
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



    #region ListFiles

    private IComparer<FtpClientRemoteFileSystemObject>? objectComparer;
    public IComparer<FtpClientRemoteFileSystemObject> ObjectComparer => objectComparer ??= new FtpClientRemoteFileSystemObjectComparer(DirectorySeparator);
    protected abstract void ListObjects(string remotePath, List<FtpClientRemoteFileSystemObject> list);

    private void ListObjectsLog(string remoteDirectory, List<FtpClientRemoteFileSystemObject> items)
    {
        if (!log.IsEnabled(LogLevel.Trace)) return;

        log.LogTrace("Retrieved directory listing of {ObjectsCount} objects for directory: {RemoteDirectory}", items.Count, remoteDirectory);
        for (var i = 0; i < items.Count; i++)
        {
            log.LogTrace("  [{RunningCount}] {FileSystemObject}", Util.FormatRunningCount(i, items.Count), items[i]);
        }
    }

    private bool ListObjectsIsIncluded(FtpClientRemoteFileSystemObject obj) =>
        obj is not { Type: FtpClientRemoteFileSystemObjectType.Directory, Name: "." or ".." };

    public IEnumerable<FtpClientRemoteFileSystemObject> ListObjects(string? remotePath)
    {
        remotePath = remotePath.TrimOrNull() ?? WorkingDirectory;
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        log.LogDebug("Getting remote file system listing of {RemotePath}", remotePath);
        var items = new List<FtpClientRemoteFileSystemObject>();
        ListObjects(remotePath, items);
        items.Sort(ObjectComparer);
        ListObjectsLog(remotePath, items);
        items = items.Where(ListObjectsIsIncluded).ToList();
        log.LogDebugMethod(new(remotePath), LOG_COMPLETE + ", found {CountObjects} items", items.Count);
        return items;
    }

    public IEnumerable<FtpClientRemoteFileSystemObject> ListObjectsRecursive(string? remotePath, Func<string, Exception, bool>? handlerException)
    {
        remotePath = remotePath.TrimOrNull() ?? WorkingDirectory;
        log.LogTraceMethod(new(remotePath, handlerException), LOG_ATTEMPT);

        var stack = new Stack<string>();
        var checkedDirs = new HashSet<string>();
        stack.Push(remotePath);

        var countObjectsFound = 0;
        while (stack.Count > 0)
        {
            var remoteDirectory = stack.Pop();
            if (!checkedDirs.Add(remoteDirectory)) continue;
            log.LogDebugMethod(new(remotePath, handlerException), "Listing {RemoteDirectory}", remoteDirectory);
            var items = new List<FtpClientRemoteFileSystemObject>();
            var shouldContinue = true;
            try
            {
                ListObjects(remoteDirectory, items);
                items.Sort(ObjectComparer);
                ListObjectsLog(remoteDirectory, items);
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
                if (item.Type == FtpClientRemoteFileSystemObjectType.Directory && !checkedDirs.Contains(item.FullName))
                {
                    stack.Push(item.FullName);
                }

                countObjectsFound++;
                yield return item;
            }
        }
        log.LogDebugMethod(new(remotePath, handlerException), LOG_COMPLETE + ", found {CountObjects} items", countObjectsFound);

    }


    #endregion ListFiles

    #region Exists

    public bool DirectoryExists(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        var result = DirectoryExistsInternal(remotePath);
        log.LogTraceMethod(new(remotePath), LOG_COMPLETE);
        return result;
    }
    protected abstract bool DirectoryExistsInternal(string remotePath);

    public bool FileExists(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        var result = FileExistsInternal(remotePath);
        log.LogTraceMethod(new(remotePath), LOG_COMPLETE);
        return result;
    }

    protected abstract bool FileExistsInternal(string remotePath);

    #endregion Exists

    #region Dispose

    public abstract void Dispose();

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
        if (!path.StartsWith("/"))
        {
            // relative path or just name
            var wd = WorkingDirectory;
            if (!wd.EndsWith("/")) path = "/" + path;

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
