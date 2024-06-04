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

using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace MaxRunSoftware.Utilities.Ftp;

public class FtpClientSFtp : FtpClientBase
{
    public FtpClientSFtp(FtpClientSftpConfig config, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        log = loggerFactory.CreateLogger<FtpClientSFtp>();
        client = Ssh.CreateSftpClient(config, loggerFactory);

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

    protected override string GetDirectorySeparatorInternal() => "/"; // https://stackoverflow.com/a/45693117
    protected override string GetWorkingDirectoryInternal() => Client.WorkingDirectory ?? DirectorySeparator;
    protected override void SetWorkingDirectoryInternal(string directory) => Client.ChangeDirectory(directory);


    private static Percent CalcProgress(long? total, ulong bytes)
    {
        if (total == null) return Percent.MinValue;
        if (total == 0L) return Percent.MaxValue;

        var totalDouble = (double)total.Value;
        var bytesDouble = (double)bytes;
        const double multiplierDouble = 100;

        var percent = bytesDouble / totalDouble * multiplierDouble;
        // ReSharper disable once RedundantCast
        return (Percent)percent;
    }

    protected override void GetFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
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
                Progress = CalcProgress(remoteFileLength, bytes),
            });
        });
    }

    protected override void PutFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
    {
        log.LogTraceMethod(new(remoteFile, localStream, handlerProgress), LOG_ATTEMPT);
        long? localStreamLength = null;
        try
        {
            localStreamLength = localStream.Length;
        }
        catch (Exception e)
        {
            log.LogDebugMethod(new(remoteFile, localStream, handlerProgress), e, "Could not get stream length");
        }

        var remoteFileAbsolute = GetAbsolutePath(remoteFile);


        void UploadFile() =>
            Client.UploadFile(localStream, remoteFileAbsolute, true, bytes =>
            {
                handlerProgress(new()
                {
                    BytesTransferred = (long)bytes,
                    Progress = CalcProgress(localStreamLength, bytes),
                });
            });


        try
        {
            UploadFile();
        }
        catch (SftpPathNotFoundException e)
        {
            log.LogDebugMethod(new(remoteFile, localStream), e, LOG_FAILED + " got " + nameof(SftpPathNotFoundException) + ", perhaps directory does not exist, attempting " + nameof(CreateDirectory));
            var (dirName, _) = remoteFileAbsolute.SplitOnLast(DirectorySeparator);
            if (string.IsNullOrEmpty(dirName))
            {
                // We are trying to create a file in /file.txt and it is failing, so something weird is going on
                log.LogErrorMethod(new(remoteFile, localStream), LOG_FAILED + " could not determine actual directory name from {RemotePath}", remoteFileAbsolute);
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
                log.LogWarningMethod(new(remoteFile, localStream), ee, LOG_FAILED + " could not create directory {Directory} for file {File}", dirName, remoteFileAbsolute);
                success = false;
            }

            if (!success) throw;

            try
            {
                UploadFile();
            }
            catch (Exception eee)
            {
                log.LogWarningMethod(new(remoteFile, localStream), eee, LOG_FAILED + " second attempt to upload file {File}", remoteFileAbsolute);
                throw;
            }
        }
    }

    protected override void CreateDirectoryInternal(string remotePath)
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
            var dirs = GetAbsolutePath(remotePath).Split(DirectorySeparator).Where(o => o.Length > 0).ToArray();
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

    protected virtual FtpClientRemoteFileSystemObject? CreateFileSystemObject(ISftpFile? item)
    {
        if (item == null) return null;
        var type = FtpClientRemoteFileSystemObjectType.Unknown;
        if (item.IsDirectory) { type = FtpClientRemoteFileSystemObjectType.Directory; }
        else if (item.IsRegularFile) { type = FtpClientRemoteFileSystemObjectType.File; }
        else if (item.IsSymbolicLink) type = FtpClientRemoteFileSystemObjectType.Link;

        return CreateFileSystemObject(item.Name, item.FullName, type);
    }
    
    protected override void ListObjectsInternal(string remotePath, List<FtpClientRemoteFileSystemObject> list) =>
        list.AddRange(
            Client.ListDirectory(remotePath)
                .OrEmpty()
                .Select(CreateFileSystemObject)
                .WhereNotNull()
        );
    
    protected override string? GetServerInfoInternal() => Client.ConnectionInfo?.ClientVersion;

    protected override void DeleteFileInternal(string remoteFile)
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

    protected override void DisposeInternal()
    {
        var c = client;
        client = null;
        Ssh.Dispose(c, log);
    }

    protected override FtpClientRemoteFileSystemObject? GetObjectInternal(string remotePath)
    {
        if (!Client.Exists(remotePath)) return null;
        var obj = Client.Get(remotePath);
        if (obj == null) return null;
        return CreateFileSystemObject(obj);
    }

    private class SftpSessionCanonicalPathWrapper
    {
        private readonly FieldSlim? fieldSlim;
        private readonly MethodSlim? methodSlim;

        public bool IsValid => fieldSlim != null && methodSlim != null;
        public string? GetCanonicalPath(string path, SftpClient instance)
        {
            if (!IsValid) return null;
            if (fieldSlim == null || methodSlim == null) return null;

            var sftpSession = fieldSlim.GetValue(instance);
            if (sftpSession == null) instance.Connect();
            sftpSession = fieldSlim.GetValue(instance);
            if (sftpSession == null) return null;

            var pathNew = methodSlim.Invoke(sftpSession, [path, ]);
            return pathNew as string;
        }

        public SftpSessionCanonicalPathWrapper(Type type)
        {
            var fm = FindFieldMethod(type);
            fieldSlim = fm?.Item1;
            methodSlim = fm?.Item2;
        }

        private static (FieldSlim, MethodSlim)? FindFieldMethod(Type type)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = type.GetFieldSlims(flags);
            foreach (var field in fields.Where(o => o.Type.Name == "ISftpSession"))
            {
                var method = FindMethod(field);
                if (method != null) return (field, method);
            }

            foreach (var field in fields.Where(o => o.Name == "_sftpSession"))
            {
                var method = FindMethod(field);
                if (method != null) return (field, method);
            }

            foreach (var field in fields)
            {
                var method = FindMethod(field);
                if (method != null) return (field, method);
            }

            return null;
        }

        private static MethodSlim? FindMethod(FieldSlim field)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var method in field.Type.GetMethodSlims(flags))
            {
                if (method.Name != "GetCanonicalPath") continue;
                if (method.Parameters.Length != 1) continue;
                if (method.Parameters[0].Parameter.Type.Type != typeof(string)) continue;
                if (method.ReturnType != null && method.ReturnType.Type != typeof(string)) continue;
                return method;
            }

            return null;
        }
    }

    // ReSharper disable once InconsistentNaming
    private static readonly DictionaryWeakType<SftpSessionCanonicalPathWrapper> canonicalPathCache = new();
    protected override string? GetAbsolutePathInternal(string remotePath)
    {
        try
        {
            if (Client.Exists(remotePath))
            {
                var pathFull = Client.Get(remotePath)?.FullName;
                if (pathFull != null && IsAbsolutePath(pathFull)) return pathFull;
            }
        }
        catch (SftpPathNotFoundException pathNotFoundException)
        {
            log.LogDebugMethod(new(remotePath), pathNotFoundException, LOG_FAILED + " path does not exist: {RemotePath}", remotePath);
        }

        var canonicalPathWrapper = canonicalPathCache.GetOrAdd(Client.GetType(), t => new(t));
        if (canonicalPathWrapper.IsValid)
        {
            log.LogTraceMethod(new(remotePath), "Attempting reflection to retrieve canonical path");
            var pathFull = canonicalPathWrapper.GetCanonicalPath(remotePath, Client);
            if (pathFull != null && IsAbsolutePath(pathFull)) return pathFull;
        }
        else
        {
            log.LogWarningMethod(new(remotePath), "Could not find GetCanonicalPath method");
        }

        return null;
    }
}
