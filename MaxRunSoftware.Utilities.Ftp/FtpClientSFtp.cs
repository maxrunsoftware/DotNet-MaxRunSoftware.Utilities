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

namespace MaxRunSoftware.Utilities.Ftp;

public class FtpClientSftpConfig : SshConfig { }

public class FtpClientSFtp : FtpClientBase
{
    public FtpClientSFtp(FtpClientSftpConfig config)
    {
        log = Constant.GetLogger<FtpClientSFtp>();
        client = Ssh.CreateClient<SftpClient>(config);
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

    public override string WorkingDirectory => Client.WorkingDirectory ?? "/";

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
            handlerProgress(new FtpClientProgress
            {
                BytesTransferred = (long)bytes,
                Progress = CalcProgress(remoteFileLength, bytes)
            });
        });
    }

    protected override void PutFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
    {
        long? localFileLength = null;
        try
        {
            localFileLength = localStream.Length;
        }
        catch (Exception e)
        {
            log.LogDebug(e, "Could not get local file stream length");
        }

        Client.UploadFile(localStream, remoteFile, true, bytes =>
        {
            handlerProgress(new FtpClientProgress
            {
                BytesTransferred = (long)bytes,
                Progress = CalcProgress(localFileLength, bytes)
            });
        });
    }

    protected override void ListFiles(string? remotePath, List<FtpClientRemoteFile> fileList)
    {
        remotePath = remotePath == null || remotePath.TrimOrNull() == null ? "." : remotePath;
        foreach (var file in Client.ListDirectory(remotePath).OrEmpty())
        {
            var name = file.Name;
            if (name == null) continue;
            var fullName = file.FullName ?? name;
            if (!fullName.StartsWith("/")) fullName = "/" + fullName;

            var type = FtpClientRemoteFileType.Unknown;
            if (file.IsDirectory) { type = FtpClientRemoteFileType.Directory; }
            else if (file.IsRegularFile) { type = FtpClientRemoteFileType.File; }
            else if (file.IsSymbolicLink) type = FtpClientRemoteFileType.Link;

            fileList.Add(new FtpClientRemoteFile(name, fullName, type));
        }
    }

    protected override string? GetServerInfo() => Client.ConnectionInfo?.ClientVersion;

    protected override void DeleteFileSingle(string remoteFile)
    {
        log.LogDebug("Deleting remote file: {File}", remoteFile);
        Client.DeleteFile(remoteFile);
    }

    public uint BufferSize
    {
        get => Client.BufferSize;
        set
        {
            if (Client.BufferSize == value) return;
            log.LogDebug("Setting buffer size to {BufferSize}", value);
            Client.BufferSize = value;
        }
    }

    public override void Dispose()
    {
        var c = client;
        client = null;
        Ssh.Dispose(c);
    }

    protected override bool ExistsFile(string remoteFile) => Client.Exists(remoteFile);

    protected override bool ExistsDirectory(string remoteDirectory) => Client.Exists(remoteDirectory);
}
