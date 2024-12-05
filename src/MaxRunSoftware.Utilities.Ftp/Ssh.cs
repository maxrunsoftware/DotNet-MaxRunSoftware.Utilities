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

using Renci.SshNet;
using Renci.SshNet.Common;

namespace MaxRunSoftware.Utilities.Ftp;

public class Ssh(SshConfig config, ILogger log) : IDisposable
{
    public Ssh(SshConfig config, ILoggerFactory loggerFactory) : this(config, loggerFactory.CreateLogger<Ssh>()) { }

    private SshClient? client = CreateClient<SshClient>(config, log);
    private SshClient Client => client ?? throw new ObjectDisposedException(GetType().FullNameFormatted());
    
    public string? ClientVersion => Client.ConnectionInfo?.ClientVersion.TrimOrNull();

    public string? RunCommand(string command)
    {
        using var cmd = Client.CreateCommand(command);
        if (cmd == null) throw new NullReferenceException($"{Client.GetType().FullNameFormatted()}.{nameof(Client.CreateCommand)}('{command}')");
        return cmd.Execute();
    }

    #region Create Clients
    
    public static SshClient CreateSshClient(SshConfig config, ILoggerFactory loggerFactory) => CreateClient<SshClient>(config, loggerFactory);
    
    public static SshClient CreateSshClient(SshConfig config, ILogger log) => CreateClient<SshClient>(config, log);
    
    public static SftpClient CreateSftpClient(SshConfig config, ILoggerFactory loggerFactory) => CreateClient<SftpClient>(config, loggerFactory);
    
    public static SftpClient CreateSftpClient(SshConfig config, ILogger log) => CreateClient<SftpClient>(config, log);

    private static T CreateClient<T>(SshConfig config, ILoggerFactory loggerFactory) where T : BaseClient =>
        CreateClient<T>(config, loggerFactory.CreateLogger<Ssh>());

    private static T CreateClient<T>(SshConfig config, ILogger log) where T : BaseClient
    {
        var host = config.Host;
        var port = config.Port;
        if (port == 0) port = 22;
        var username = config.Username.CheckNotNullTrimmed();
        var password = config.Password.TrimOrNull();
        var privateKeys = ParsePrivateKeys(config, log);

        if (password == null && privateKeys.Length == 0) throw new ArgumentException("No password provided and no private keys provided.", nameof(config.Password));
        if (password != null && privateKeys.Length > 0) throw new ArgumentException("Private keys are not supported when a password is supplied.", nameof(config.PrivateKeys));

        IBaseClient? client = null;
        var clientType = typeof(T);
        if (clientType == typeof(SshClient))
        {
            if (password == null)
            {
                client = new SshClient(host, port, username, privateKeys);
            }
            else
            {
                client = new SshClient(host, port, username, password);
            }
        }
        else if (clientType == typeof(SftpClient))
        {
            if (password == null)
            {
                client = new SftpClient(host, port, username, privateKeys);
            }
            else
            {
                client = new SftpClient(host, port, username, password);
            }
        }
        
        if (client == null) throw new NotImplementedException($"Cannot create SSH Client for type {clientType.FullNameFormatted()}");

        var ci = client.ConnectionInfo.CheckNotNull();
        ci.Timeout = config.Timeout;
        ci.ChannelCloseTimeout = config.ChannelCloseTimeout;
        ci.Encoding = config.Encoding;
        ci.RetryAttempts = config.RetryAttempts;
        ci.MaxSessions = config.MaxSessions;

        client.HostKeyReceived += (_, args) =>
        {
            log.LogDebug("{Host}:{Port}  HostKeyName={HostKeyName}  HostKeyLength={HostKeyLength}  HostKey={HostKey}  FingerPrint={FingerPrint}  FingerPrintMD5={FingerPrintMD5}  FingerPrintSHA256={FingerPrintSHA256}"
                ,   ci.Host
                ,   ci.Port
                ,   args.HostKeyName
                ,   args.KeyLength
                ,   args.HostKey
                ,   args.FingerPrint
                ,   args.FingerPrintMD5
                ,   args.FingerPrintSHA256
                );
            args.CanTrust = config.HostKeyCheck(args);
        };
        
        try
        {
            log.LogInformation("Connecting {Type} to server {Host}:{Port} with username '{Username}'", clientType.NameFormatted(), host, port, username);
            client.Connect();
            log.LogInformation("Connection successful");
        }
        catch (Exception)
        {
            Dispose(client, log);
            throw;
        }

        return (T)client;
        
        static IPrivateKeySource[] ParsePrivateKeys(SshConfig config, ILogger log)
        {
            var privateKeys = config.PrivateKeys.ToList();
            var list = new List<IPrivateKeySource>();
            foreach (var (pkData, pkPassword) in privateKeys)
            {
                var memoryStream = new MemoryStream(pkData) { Position = 0, };
                PrivateKeyFile privateKeyFile;
                if (pkPassword == null)
                {
                    privateKeyFile = new(memoryStream);
                    log.LogDebug("Using private key {Size} bytes", pkData.Length);
                }
                else
                {
                    privateKeyFile = new(memoryStream, pkPassword);
                    log.LogDebug("Using (password protected) private key {Size} bytes", pkData.Length);
                }
                
                list.Add(privateKeyFile);
            }
            
            return list.ToArray();
        }
    }

    #endregion Create Clients

    public void Dispose()
    {
        var c = client;
        client = null;
        Dispose(c, log);
    }

    public static void Dispose(IBaseClient? client, ILogger log)
    {
        if (client == null) return;
        //var log = GetLogger();

        try
        {
            if (client.IsConnected) client.Disconnect();
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Error disconnecting from server for client {Type}", client.GetType().FullNameFormatted());
        }

        try
        {
            client.Dispose();
        }
        catch (Exception e)
        {
            log.LogWarning(e, "Error disposing of {Type}", client.GetType().FullNameFormatted());
        }
    }

    public static HostKeyEventArgs GetHostKey(string host, ushort port)
    {
        var list = new List<HostKeyEventArgs>();
        if (port == 0) port = 22;
        var client = new SshClient(host, port, "anonymous", "anonymous");
        client.HostKeyReceived += (_, args) =>
        {
            list.Add(args);
            args.CanTrust = false;
        };

        try
        {
            client.Connect();
        }
        catch (SshConnectionException)
        {
            if (list.Count < 1) throw;
        }

        if (list.Count < 1) throw new SshConnectionException("Did not receive the host key");
        return list[0];

    }
}
