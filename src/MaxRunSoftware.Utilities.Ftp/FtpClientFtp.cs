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

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using FluentFTP;
using FluentFTP.Client.BaseClient;

namespace MaxRunSoftware.Utilities.Ftp;

public class FtpClientFtp : FtpClientBase
{
    private class FtpClientLogger(ILoggerFactory loggerProvider) : IFtpLogger
    {
        private readonly ILogger log = loggerProvider.CreateLogger<FtpClient>();
        
        public void Log(FtpLogEntry entry)
        {
            log.Log(ParseLevel(entry.Severity), entry.Exception, "{Message}", entry.Message);
            
            return;
            
            static LogLevel ParseLevel(FtpTraceLevel ftpTraceLevel) => ftpTraceLevel switch
            {
                FtpTraceLevel.Verbose => LogLevel.Debug,
                FtpTraceLevel.Info => LogLevel.Information,
                FtpTraceLevel.Warn => LogLevel.Warning,
                FtpTraceLevel.Error => LogLevel.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(ftpTraceLevel), ftpTraceLevel, null),
            };
        }
    }
    
    //private readonly Func<FtpClientFtpRemoteCertificateInfo, bool> validateCertificate;
    public FtpClientFtp(FtpClientFtpConfig config, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        log = loggerFactory.CreateLogger<FtpClientFtp>();
        
        var host = config.Host;
        var port = config.Port;
        var ftpClientLogger = new FtpClientLogger(loggerFactory);

        if (config.Username == null)
        {
            if (config.Password != null) throw new InvalidOperationException("Password specified but no Username specified");
            client = new(
                host: host,
                port: port,
                config: config.FtpConfig,
                logger: ftpClientLogger
            );
        }
        else
        {
            client = new(
                host,
                port: port,
                user: config.Username,
                pass: config.Password ?? string.Empty,
                config: config.FtpConfig,
                logger: ftpClientLogger
            );
        }
        
        var validateCertificate = config.ValidateCertificate;
        var ftpClientFtpRemoteCertificateInfoLog = loggerFactory.CreateLogger<FtpClientFtpRemoteCertificateInfo>();
        var _this = this;
        void ValidateCertificate(BaseFtpClient _, FtpSslValidationEventArgs args)
        {
            var rc = new FtpClientFtpRemoteCertificateInfo(_this, config, args);
            rc.Log(ftpClientFtpRemoteCertificateInfoLog, host, port);
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

    protected override string ServerInfoGet() => $"{Client.ServerOS} : {Client.ServerType}";

    protected override string WorkingDirectoryGet() => Client.GetWorkingDirectory() ?? DirectorySeparator;
    protected override void WorkingDirectorySet(string directory) => Client.SetWorkingDirectory(directory);

    private readonly string directorySeparator;
    protected override string DirectorySeparatorGet() => directorySeparator;

    protected override void GetFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress) =>
        Client.DownloadStream(
            localStream, 
            remoteFile, 
            progress: progress => handlerProgress(new(progress.Progress, progress.TransferredBytes)));

    protected override void PutFileInternal(string remoteFile, Stream localStream, Action<FtpClientProgress> handlerProgress)
    {
        Action<FtpProgress> ftpProgressHandler = progress => handlerProgress(new(progress.Progress, progress.TransferredBytes));

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

    protected override void CreateDirectoryInternal(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        if (Client.DirectoryExists(remotePath))
        {
            log.LogTraceMethod(new(remotePath), LOG_IGNORED + " because directory already exists");
            return;
        }

        Client.CreateDirectory(remotePath);

        log.LogTraceMethod(new(remotePath), LOG_SUCCESS);
        log.LogInformation("Created remote directory {RemotePath}", remotePath);
    }

    protected override void DeleteDirectoryInternal(string remotePath)
    {
        log.LogTraceMethod(new(remotePath), LOG_ATTEMPT);
        if (!Client.DirectoryExists(remotePath))
        {
            log.LogTraceMethod(new(remotePath), LOG_IGNORED + " because directory does not exist");
            return;
        }

        Client.DeleteDirectory(remotePath);

        log.LogTraceMethod(new(remotePath), LOG_SUCCESS);
        log.LogInformation("Deleted remote directory {RemotePath}", remotePath);
    }

    protected virtual FtpClientRemoteObject? CreateFileSystemObject(FtpListItem? item)
    {
        if (item == null) return null;
        var type = item.Type switch
        {
            FtpObjectType.Directory => FtpClientRemoteObjectType.Directory,
            FtpObjectType.File => FtpClientRemoteObjectType.File,
            FtpObjectType.Link => FtpClientRemoteObjectType.Link,
            _ => FtpClientRemoteObjectType.Unknown,
        };

        return CreateFileSystemObject(item.Name, item.FullName, type);
    }
    protected override void ListObjectsInternal(string remotePath, List<FtpClientRemoteObject> list) =>
        list.AddRange(Client.GetListing(remotePath).OrEmpty().Select(CreateFileSystemObject).WhereNotNull());

    protected override FtpClientRemoteObject? GetObjectInternal(string remotePath) =>
        CreateFileSystemObject(Client.GetObjectInfo(remotePath));

    protected override string GetAbsolutePathInternal(string remotePath) => throw new NotImplementedException();


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


public class FtpClientFtpConfig
{
    public FtpClientFtpConfig()
    {
        ValidateCertificate = _ => false;
        
        FtpConfig = new()
        {
            LogHost = true,
            LogUserName = true,
            LogPassword = false,
            ReadTimeout = 1000 * 60,
            DataConnectionReadTimeout = 1000 * 60,
            EncryptionMode = FtpEncryptionMode.Auto,
            ValidateAnyCertificate = false,
            // SslBuffering = FtpsBuffering.Off,
        };
        //FtpConfig.Clone();
    }
    
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 21;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public FtpConfig FtpConfig { get; set; }
    public string? WorkingDirectory { get; set; }
    public Func<FtpClientFtpRemoteCertificateInfo, bool> ValidateCertificate { get; set; }
    
    /// <summary>
    /// <para>
    /// FTP(S) does not mandate directory separator character unlike SFTP (which uses '/')
    /// </para>
    /// <para>
    /// https://stackoverflow.com/questions/37411642/how-to-get-ftp-servers-file-separator
    /// </para>
    /// </summary>
    public string DirectorySeparator { get; set; } = "/";
}

public class FtpClientFtpRemoteCertificateInfo
{
    public FtpClientFtpRemoteCertificateInfo(FtpClientFtp client, FtpClientFtpConfig config, FtpSslValidationEventArgs args)
    {
        Client = client;
        Config = config;

        Certificate = args.Certificate.CheckNotNull();
        Certificate2 = new(Certificate);

        Chain = args.Chain.CheckNotNull();
        PolicyErrors = args.PolicyErrors;
    }

    public FtpClientFtp Client { get; }
    public FtpClientFtpConfig Config { get; }

    public X509Certificate Certificate { get; }
    public X509Certificate2 Certificate2 { get; }
    public X509Chain Chain { get; }
    public SslPolicyErrors PolicyErrors { get; }

    public void Log(ILogger log, string host, ushort port)
    {
        log.LogTrace("Remote server {Host}:{Port} " + nameof(X509Certificate2) + "." + nameof(Certificate2.GetRawCertDataString) + ": {CertificateDataString}", host, port, Certificate2.GetRawCertDataString());

        log.LogDebug("Remote server {Host}:{Port} " + nameof(X509Certificate2) + "." + nameof(Certificate2.Thumbprint) + ": {CertificateThumbprint}", host, port, Certificate2.Thumbprint);
        log.LogDebug("Remote server {Host}:{Port} " + nameof(X509Certificate2) + "." + nameof(Certificate2.Thumbprint) + "Formatted" + ": {CertificateThumbprintFormatted}", host, port, Certificate2.Thumbprint.ToLowerInvariant().Chunk(2).Select(o => new string(o)).ToStringDelimited(":"));
        log.LogDebug("Remote server {Host}:{Port} " + nameof(X509Certificate2) + "." + "ToString(detail: false)" + ": {CertificateToString}", host, port, Environment.NewLine + Certificate2.ToString(false) + Environment.NewLine);
        log.LogDebug("Remote server {Host}:{Port} " + nameof(X509Certificate2) + "." + "ToString(detail: true)" + ": {CertificateToStringDetailed}", host, port, Environment.NewLine + Certificate2.ToString(false) + Environment.NewLine);

        log.LogDebug("Remote server {Host}:{Port} " + nameof(SslPolicyErrors) + ": {CertificateSslPolicyErrors}", host, port, PolicyErrors);
    }
}
