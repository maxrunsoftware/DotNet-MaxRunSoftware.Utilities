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
using FluentFTP.Client.BaseClient;

namespace MaxRunSoftware.Utilities.Ftp;

public class FtpClientFtp : FtpClientBase
{
    private class FtpClientLogger : IFtpLogger
    {
        private readonly ILogger log;
        public FtpClientLogger(ILoggerProvider loggerProvider) => log = loggerProvider.CreateLogger<FtpClient>();

        private static LogLevel ParseLevel(FtpTraceLevel ftpTraceLevel) => ftpTraceLevel switch
        {
            FtpTraceLevel.Verbose => LogLevel.Debug,
            FtpTraceLevel.Info => LogLevel.Information,
            FtpTraceLevel.Warn => LogLevel.Warning,
            FtpTraceLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(ftpTraceLevel), ftpTraceLevel, null)
        };

        public void Log(FtpLogEntry entry) => log.Log(ParseLevel(entry.Severity), entry.Exception, "{Message}", entry.Message);
    }

    private readonly Func<FtpClientFtpRemoteCertificateInfo, bool> validateCertificate;
    public FtpClientFtp(FtpClientFtpConfig config, ILoggerProvider loggerProvider) : base(loggerProvider)
    {
        log = loggerProvider.CreateLogger<FtpClientFtp>();

        validateCertificate = config.ValidateCertificate;
        var host = config.Host;
        var port = config.Port;
        var logger = new FtpClientLogger(loggerProvider);

        if (config.Username == null)
        {
            if (config.Password != null) throw new InvalidOperationException("Password specified but no Username specified");

            client = new(
                host: host,
                port: port,
                config: config.FtpConfig,
                logger: logger
            );
        }
        else
        {
            client = new(
                host: host,
                port: port,
                user: config.Username,
                pass: config.Password ?? string.Empty,
                config: config.FtpConfig,
                logger: logger
            );
        }

        void ValidateCertificate(BaseFtpClient _, FtpSslValidationEventArgs args)
        {
            var rc = new FtpClientFtpRemoteCertificateInfo(this, config, args);
            rc.Log(loggerProvider, host, port);
            var isValid = validateCertificate(rc);
            log.LogDebug("Certificate validated as {CertificateIsValid}", isValid);
            args.Accept = isValid;
        }

        client.ValidateCertificate += ValidateCertificate;

        log.LogDebug("Connecting to FTP server {Host}:{Port} with username '{Username}'", config.Host, config.Port, config.Username);
        client.Connect();
        log.LogDebug("Successfully connected to FTP server {Host}:{Port} with username '{Username}'", config.Host, config.Port, config.Username);

        directorySeparator = config.DirectorySeparator;
        if (config.WorkingDirectory != null) WorkingDirectory = config.WorkingDirectory;
    }


    private readonly ILogger log;

    private FtpClient? client;
    private FtpClient Client => client ?? throw new ObjectDisposedException(GetType().FullNameFormatted());

    protected override string GetServerInfoInternal() => Client.ServerOS + " : " + Client.ServerType;

    protected override string GetWorkingDirectoryInternal() => Client.GetWorkingDirectory() ?? DirectorySeparator;
    protected override void SetWorkingDirectoryInternal(string directory) => Client.SetWorkingDirectory(directory);

    private readonly string directorySeparator;
    protected override string GetDirectorySeparatorInternal() => directorySeparator;

    protected override void GetFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress) =>
        Client.DownloadStream(localStream, remoteFile, progress: progress => handlerProgress(new()
        {
            Progress = (Percent)progress.Progress,
            BytesTransferred = progress.TransferredBytes
        }));

    protected override void PutFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
    {
        Action<FtpProgress> ftpProgressHandler = progress => handlerProgress(new()
        {
            Progress = (Percent)progress.Progress,
            BytesTransferred = progress.TransferredBytes
        });

        try
        {
            Client.UploadStream(localStream, remoteFile, progress: ftpProgressHandler);
            return;
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Error putting file using security protocol, retrying with all known security protocols");
        }

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

    protected override void CreateDirectoryInternal(string remotePath) => throw new NotImplementedException();
    protected override void DeleteDirectoryInternal(string remotePath) => throw new NotImplementedException();

    protected virtual FtpClientRemoteFileSystemObject? CreateFileSystemObject(FtpListItem? item)
    {
        if (item == null) return null;
        var type = item.Type switch
        {
            FtpObjectType.Directory => FtpClientRemoteFileSystemObjectType.Directory,
            FtpObjectType.File => FtpClientRemoteFileSystemObjectType.File,
            FtpObjectType.Link => FtpClientRemoteFileSystemObjectType.Link,
            _ => FtpClientRemoteFileSystemObjectType.Unknown
        };

       return CreateFileSystemObject(item.Name, item.FullName, type);
    }
    protected override void ListObjectsInternal(string remotePath, List<FtpClientRemoteFileSystemObject> list) =>
        list.AddRange(Client.GetListing(remotePath).OrEmpty().Select(CreateFileSystemObject).WhereNotNull());

    protected override FtpClientRemoteFileSystemObject? GetObjectInternal(string remotePath) =>
        CreateFileSystemObject(Client.GetObjectInfo(remotePath));

    protected override string? GetAbsolutePathInternal(string remotePath) => throw new NotImplementedException();


    protected override void DeleteFileInternal(string remoteFile)
    {
        log.LogDebug("Deleting remote file: {RemoteFile}", remoteFile);
        Client.DeleteFile(remoteFile);
    }

    protected override void DisposeInternal()
    {
        var c = client;
        client = null;

        if (c == null) return;

        try
        {
            c.Disconnect();
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Error disconnecting from server");
        }

        try
        {
            c.Dispose();
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Error disposing of {Object}", c.GetType().FullNameFormatted());
        }
    }
}
