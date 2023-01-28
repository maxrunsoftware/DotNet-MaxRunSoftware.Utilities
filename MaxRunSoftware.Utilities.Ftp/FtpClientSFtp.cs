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

using Renci.SshNet;
using Renci.SshNet.Common;

namespace MaxRunSoftware.Utilities.Ftp;

public class FtpClientSFtp : FtpClientBase
{
    public FtpClientSFtp(FtpClientSftpConfig config, ILoggerProvider loggerProvider) : base(loggerProvider)
    {
        log = loggerProvider.CreateLogger<FtpClientSFtp>();
        client = Ssh.CreateClient<SftpClient>(config, loggerProvider);

        if (config.WorkingDirectory != null) WorkingDirectory = config.WorkingDirectory;
    }

    private readonly ILogger log;

    private SftpClient? client;

    private SftpClient Client
    {
        get
        {
            var c = client;
            if (c == null) throw new ObjectDisposedException(GetType().FullNameFormatted());

            return c;
        }
    }

    protected override string GetDirectorySeparator() => "/"; // https://stackoverflow.com/a/45693117
    protected override string GetWorkingDirectory() => Client.WorkingDirectory ?? DirectorySeparator;
    protected override void SetWorkingDirectory(string directory) => Client.ChangeDirectory(directory);


    private static Percent CalcProgress(long? total, ulong bytes)
    {
        if (total == null) return Percent.MinValue;
        if (total == 0L) return Percent.MaxValue;

        var totalDouble = (double)total.Value;
        var bytesDouble = (double)bytes;
        const double multiplierDouble = 100;

        var percent = bytesDouble / totalDouble * multiplierDouble;
        return (Percent)percent;
    }

    protected override void GetFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
    {
        long? remoteFileLength = null;
        try
        {
            remoteFileLength = Client.Get(remoteFile)?.Length;
        }
        catch (Exception e)
        {
            log.LogDebug(e, "Could not get remote file length {RemoteFile}", remoteFile);
        }

        Client.DownloadFile(remoteFile, localStream, bytes =>
        {
            handlerProgress(new()
            {
                BytesTransferred = (long)bytes,
                Progress = CalcProgress(remoteFileLength, bytes)
            });
        });
    }

    protected override void PutFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
    {
        log.LogTraceMethod(new(remoteFile, localStream), LOG_ATTEMPT);
        long? localStreamLength = null;
        try
        {
            localStreamLength = localStream.Length;
        }
        catch (Exception e)
        {
            log.LogDebugMethod(new(remoteFile, localStream), e, "Could not get local file stream length");
        }


        string? GetRemoteFileDirName()
        {
            var rf = remoteFile.Trim();
            while (rf.Length > 0 && rf.StartsWith(DirectorySeparator)) rf = rf.RemoveLeft().Trim();
            while (rf.Length > 0 && rf.EndsWith(DirectorySeparator)) rf = rf.RemoveRight().Trim();

            // https://stackoverflow.com/a/21733934
            var idx = remoteFile.LastIndexOf(DirectorySeparator, StringComparison.Ordinal);
            var dirName = idx < 0 ? null : remoteFile[..idx];
            return dirName;
        }

        void UploadFile()
        {
            Client.UploadFile(localStream, remoteFile, true, bytes =>
            {
                handlerProgress(new()
                {
                    BytesTransferred = (long)bytes,
                    Progress = CalcProgress(localStreamLength, bytes)
                });
            });
        }


        try
        {
            UploadFile();
        }
        catch (SftpPathNotFoundException e)
        {
            log.LogDebugMethod(new(remoteFile, localStream), e, LOG_FAILED + " got " + nameof(SftpPathNotFoundException) + ", perhaps directory does not exist, attempting " + nameof(CreateDirectory));
            var dirName = GetRemoteFileDirName();
            if (dirName == null)
            {
                log.LogErrorMethod(new(remoteFile, localStream), LOG_FAILED + " could not determine actual directory name from {RemotePath}", remoteFile);
                throw;
            }

            bool success;
            try
            {
                CreateDirectory(dirName);
                success = true;
            }
            catch (Exception ee)
            {
                log.LogWarningMethod(new(remoteFile, localStream), ee, LOG_FAILED + " could not create directory {Directory} for file {File}", dirName, remoteFile);
                success = false;
            }

            if (success)
            {
                try
                {
                    UploadFile();
                    return;
                }
                catch (Exception eee)
                {
                    log.LogWarningMethod(new(remoteFile, localStream), eee, LOG_FAILED + " second attempt to upload file {File}", remoteFile);
                    throw;
                }
            }

            throw;
        }
    }

    public override void CreateDirectory(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        if (Client.Exists(remotePath))
        {
            log.LogTraceMethod(new(remotePath), LOG_IGNORED + " because directory already exists");
            return;
        }

        try
        {
            Client.CreateDirectory(remotePath);
        }
        catch (SftpPathNotFoundException sftpPathNotFoundException)
        {
            log.LogDebugMethod(new(remotePath), sftpPathNotFoundException, LOG_FAILED + " so attempting to create each directory in parent hierarchy");
            var dirs = GetAbsolutePath(remotePath).Split(DirectorySeparator).Where(o => o.Trim().Length > 0).ToArray();
            log.LogTraceMethod(new(remotePath), "Creating {DirectoriesToCreateCount} directories: {DirectoriesToCreate}", dirs.Length, dirs.ToStringDelimited(" > "));
            var dirsAdded = new List<string>();
            foreach (var dir in dirs)
            {
                var dirCreate = dir;
                if (dirsAdded.Count > 0) dirCreate = dirsAdded.ToStringDelimited(DirectorySeparator) + DirectorySeparator + dirCreate;
                dirCreate = DirectorySeparator + dirCreate;
                log.LogTraceMethod(new(remotePath), LOG_ATTEMPT + " create directory {RemoteDirectory}", dirCreate);
                if (!Client.Exists(dirCreate)) Client.CreateDirectory(dirCreate);
                log.LogTraceMethod(new(remotePath), LOG_SUCCESS + " create directory {RemoteDirectory}", dirCreate);
                dirsAdded.Add(dir);
            }
        }

        log.LogTraceMethod(new(remotePath), LOG_SUCCESS);
        log.LogInformation("Created remote directory {RemotePath}", remotePath);
    }

    protected override void DeleteDirectoryInternal(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        Client.DeleteDirectory(remotePath);
        log.LogTraceMethod(new(remotePath), LOG_SUCCESS);
        log.LogInformation("Deleted remote directory {RemotePath}", remotePath);
    }

    protected override void ListObjects(string? remotePath, List<FtpClientRemoteFileSystemObject> list)
    {
        remotePath = string.IsNullOrWhiteSpace(remotePath) ? WorkingDirectory : remotePath;
        foreach (var file in Client.ListDirectory(remotePath).OrEmpty())
        {
            var name = file.Name;
            if (name == null) continue;
            var fullName = file.FullName ?? name;
            if (!fullName.StartsWith(DirectorySeparator)) fullName = DirectorySeparator + fullName;


            var type = FtpClientRemoteFileSystemObjectType.Unknown;
            if (file.IsDirectory) { type = FtpClientRemoteFileSystemObjectType.Directory; }
            else if (file.IsRegularFile) { type = FtpClientRemoteFileSystemObjectType.File; }
            else if (file.IsSymbolicLink) type = FtpClientRemoteFileSystemObjectType.Link;

            list.Add(new(name, fullName, type));
        }
    }

    protected override string? GetServerInfo() => Client.ConnectionInfo?.ClientVersion;

    protected override void DeleteFileSingle(string remoteFile)
    {
        log.LogTraceMethod(new(remoteFile), LOG_ATTEMPT);
        Client.DeleteFile(remoteFile);
        log.LogTraceMethod(new(remoteFile), LOG_SUCCESS);
    }

    public uint BufferSize
    {
        get => Client.BufferSize;
        set
        {
            log.LogTraceMethod(new(value), LOG_ATTEMPT);
            if (Client.BufferSize == value)
            {
                log.LogTraceMethod(new(value), LOG_IGNORED + " because same as current buffer size");
                return;
            }
            Client.BufferSize = value;
            log.LogDebugMethod(new(value), LOG_SUCCESS);
        }
    }

    public override void Dispose()
    {
        var c = client;
        client = null;
        Ssh.Dispose(c, log);
    }

    protected override bool DirectoryExistsInternal(string remotePath) => Client.Exists(remotePath);
    protected override bool FileExistsInternal(string remotePath) => Client.Exists(remotePath);
}
