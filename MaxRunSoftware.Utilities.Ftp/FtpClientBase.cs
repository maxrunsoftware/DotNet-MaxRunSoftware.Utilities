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
    protected FtpClientBase()
    {
        log = Constant.GetLogger<FtpClientBase>();
        serverInfo = new Lzy<string?>(GetServerInfo);
    }

    private readonly ILogger log;

    #region ServerInfo

    public string? ServerInfo => serverInfo.Value;
    private readonly Lzy<string?> serverInfo;
    protected abstract string? GetServerInfo();

    #endregion ServerInfo

    #region WorkingDirectory

    public abstract string WorkingDirectory { get; }

    #endregion WorkingDirectory

    #region GetFile

    protected abstract void GetFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress);

    public void GetFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress)
    {
        localFile = Path.GetFullPath(localFile);

        if (File.Exists(localFile))
        {
            log.LogDebug("Deleting existing local file: {LocalFile}", localFile);
            File.Delete(localFile);
        }

        //remoteFile = ToAbsolutePath(remoteFile);
        //if (!ExistsFile(remoteFile)) throw new FileNotFoundException($"Remote file {remoteFile} does not exist", remoteFile);
        log.LogDebug("Downloading file {RemoteFile} --> {LocalFile}", remoteFile, localFile);

        handlerProgress ??= progress => log.LogDebug("{Type}.{Method}(remoteFile: {RemoteFile}, localFile: {LocalFile}) {Progress}", GetType().NameFormatted(), nameof(GetFile), remoteFile, localFile, progress);

        using (var localFileStream = Util.FileOpenWrite(localFile))
        {
            GetFile(remoteFile, localFileStream, handlerProgress);
            localFileStream.Flush();
        }

        log.LogInformation("Downloaded {FileLength} byte file {RemoteFile} --> {LocalFile}", Util.FileGetLength(localFile).ToStringCommas(), remoteFile, localFile);
    }

    #endregion GetFile

    #region PutFile

    protected abstract void PutFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress);

    public void PutFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress)
    {
        localFile = Path.GetFullPath(localFile);
        localFile.CheckFileExists();

        //remoteFile = ToAbsolutePath(remoteFile);
        log.LogDebug("Uploading {FileLength} byte file {LocalFile} --> {RemoteFile}", Util.FileGetLength(localFile).ToStringCommas(), localFile, remoteFile);

        handlerProgress ??= progress => log.LogDebug("{Type}.{Method}(remoteFile: {RemoteFile}, localFile: {LocalFile}) {Progress}", GetType().NameFormatted(), nameof(PutFile), remoteFile, localFile, progress);

        using (var localFileStream = Util.FileOpenRead(localFile))
        {
            PutFile(remoteFile, localFileStream, handlerProgress);
            //localFileStream.Flush();
        }

        log.LogInformation("Uploaded {FileLength} byte file {LocalFile} --> {RemoteFile}", Util.FileGetLength(localFile).ToStringCommas(), localFile, remoteFile);
    }

    #endregion PutFile

    #region DeleteFile

    protected abstract void DeleteFileSingle(string remoteFile);

    public void DeleteFile(string remoteFile)
    {
        remoteFile = ToAbsolutePath(remoteFile);
        if (remoteFile.ContainsAny("*", "?")) // wildcard
        {
            var pathParts = remoteFile.Split("/").TrimOrNull().WhereNotNull().ToList();
            var remoteFileName = pathParts.PopTail();
            var path = string.Empty;
            if (pathParts.Count > 0) path = "/" + string.Join("/", pathParts);

            foreach (var file in ListFiles(path))
            {
                if (file.Name.EqualsWildcard(remoteFileName))
                {
                    log.LogDebug("Attempting delete of remote file {RemoteFile}", file.FullName);
                    DeleteFileSingle(file.FullName);
                    log.LogInformation("Successfully deleted remote file {RemoteFile}", file.FullName);
                }
            }
        }
        else
        {
            log.LogDebug("Attempting delete of remote file {RemoteFile}", remoteFile);
            DeleteFileSingle(remoteFile);
            log.LogInformation("Successfully deleted remote file {RemoteFile}", remoteFile);
        }
    }

    #endregion DeleteFile

    #region ListFiles

    protected abstract void ListFiles(string remotePath, List<FtpClientRemoteFile> fileList);

    public IEnumerable<FtpClientRemoteFile> ListFiles(string? remotePath)
    {
        remotePath = remotePath.TrimOrNull() ?? WorkingDirectory;

        log.LogDebug("Getting remote file listing of {RemotePath}", remotePath);
        var list = new List<FtpClientRemoteFile>();
        ListFiles(remotePath, list);
        log.LogDebug("Found {FileCount} files at {RemotePath}", list.Count, remotePath);
        return list;
    }

    public IEnumerable<FtpClientRemoteFile> ListFilesRecursive(string remotePath)
    {
        remotePath = remotePath.TrimOrNull() ?? ".";
        remotePath = ToAbsolutePath(remotePath);

        var list = new List<FtpClientRemoteFile>();
        var n = remotePath.Split('/').LastOrDefault(o => o.TrimOrNull() != null);
        if (n != null)
        {
            list.Add(new FtpClientRemoteFile(n, remotePath, FtpClientRemoteFileType.Directory));
        }

        var checkedDirectories = new HashSet<string>();
        foreach (var dir in list.Where(o => o.Type == FtpClientRemoteFileType.Directory).ToList())
        {
            if (!checkedDirectories.Add(dir.FullName)) continue;
            log.LogDebug("Getting remote file listing of {RemoteDirectory}", dir.FullName);
            ListFiles(dir.FullName, list);
        }

        log.LogDebug("Found {FilesCount} recursive files at {RemotePath}", list.Count, remotePath);
        return list;
    }

    #endregion ListFiles

    #region Exists

    protected abstract bool ExistsFile(string remoteFile);

    protected abstract bool ExistsDirectory(string remoteDirectory);

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

        path = "/" + string.Join("/", stack);
        return path;
    }

    #endregion Helpers
}
