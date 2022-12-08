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

using System.Net;
using FluentFTP;

namespace MaxRunSoftware.Utilities.Ftp;

public class FtpClientFtpConfig
{
    public FtpClientFtpConfig()
    {
        FtpLog = Constant.GetLogger<FtpClient>();
        FtpConfig = new FtpConfig
        {
            LogHost = true,
            LogUserName = true,
            LogPassword = false,
            ReadTimeout = 1000 * 60,
            DataConnectionReadTimeout = 1000 * 60,
            ValidateAnyCertificate = true
        };
        FtpConfig.Clone();
    }

    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 21;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public FtpConfig FtpConfig { get; set; }
    public ILogger FtpLog { get; set; }
}

public class FtpClientFtp : FtpClientBase
{
    public FtpClientFtp(FtpClientFtpConfig config)
    {
        log = Constant.GetLogger<FtpClientFtp>();

        if (config.Username == null)
        {
            if (config.Password != null) throw new InvalidOperationException("Password specified but no Username specified");
            client = new FtpClient(
                config.Host,
                config.Port,
                config.FtpConfig.Clone()!,
                config.FtpLog
            );
        }
        else
        {
            client = new FtpClient(
                config.Host,
                port: config.Port,
                user: config.Username,
                pass: config.Password ?? string.Empty,
                config: config.FtpConfig.Clone()!,
                logger: config.FtpLog
            );
        }

        log.LogDebug("Connecting to FTP server {Host}:{Port} with username '{Username}'", config.Host, config.Port, config.Username);
        client.Connect();
        log.LogDebug("Successfully connected to FTP server {Host}:{Port} with username '{Username}'", config.Host, config.Port, config.Username);
    }

    private readonly ILogger log;

    private FtpClient? client;
    private FtpClient Client => client ?? throw new ObjectDisposedException(GetType().FullNameFormatted());

    protected override string GetServerInfo() => Client.ServerOS + " : " + Client.ServerType;

    public override string WorkingDirectory => Client.GetWorkingDirectory() ?? "/";

    protected override void GetFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress) => Client.DownloadStream(localStream, remoteFile, progress: progress => handlerProgress(new FtpClientProgress { Progress = (Percent)progress.Progress, BytesTransferred = progress.TransferredBytes }));

    protected override void PutFile(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
    {
        Action<FtpProgress> ftpProgressHandler = progress => handlerProgress(new FtpClientProgress { Progress = (Percent)progress.Progress, BytesTransferred = progress.TransferredBytes });

        try
        {
            Client.UploadStream(localStream, remoteFile, progress: ftpProgressHandler);
            return;
        }
        catch (Exception e) { log.LogWarning(e, "Error putting file using security protocol, retrying with all known security protocols"); }

        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Client.UploadStream(localStream, remoteFile, progress: ftpProgressHandler);
        }
        catch (Exception ee)
        {
            log.LogError(ee, "Error putting file (second time)");
            throw;
        }
    }

    protected override void ListFiles(string? remotePath, List<FtpClientRemoteFile> fileList)
    {
        foreach (var file in (remotePath.TrimOrNull() == null ? Client.GetListing() : Client.GetListing(remotePath!)).OrEmpty())
        {
            if (file == null) continue;
            var name = file.Name;
            if (name == null) continue;
            var fullName = file.FullName ?? name;
            if (!fullName.StartsWith("/")) fullName = "/" + fullName;

            var type = file.Type switch
            {
                FtpObjectType.Directory => FtpClientRemoteFileType.Directory,
                FtpObjectType.File => FtpClientRemoteFileType.File,
                FtpObjectType.Link => FtpClientRemoteFileType.Link,
                _ => FtpClientRemoteFileType.Unknown
            };

            fileList.Add(new FtpClientRemoteFile(name, fullName, type));
        }
    }


    protected override void DeleteFileSingle(string remoteFile)
    {
        log.LogDebug("Deleting remote file: {RemoteFile}", remoteFile);
        Client.DeleteFile(remoteFile);
    }

    public override void Dispose()
    {
        var c = client;
        client = null;

        if (c == null) return;

        try { c.Disconnect(); }
        catch (Exception e) { log.LogWarning(e, "Error disconnecting from server"); }

        try { c.Dispose(); }
        catch (Exception e) { log.LogWarning(e, "Error disposing of {Object}", c.GetType().FullNameFormatted()); }
    }


    protected override bool ExistsFile(string remoteFile) => Client.FileExists(remoteFile);

    protected override bool ExistsDirectory(string remoteDirectory) => Client.DirectoryExists(remoteDirectory);
}
