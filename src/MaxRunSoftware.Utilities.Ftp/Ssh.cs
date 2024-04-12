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

namespace MaxRunSoftware.Utilities.Ftp;

public class Ssh : IDisposable
{
    public Ssh(SshConfig config, ILoggerFactory loggerProvider)
    {
        log = loggerProvider.CreateLogger<Ssh>();
        client = CreateClient<SshClient>(config, log);
    }
    private readonly ILogger log;
    private SshClient? client;

    private SshClient Client
    {
        get
        {
            var c = client;
            if (c == null) throw new ObjectDisposedException(GetType().FullNameFormatted());

            return c;
        }
    }

    public string? ClientVersion => Client.ConnectionInfo?.ClientVersion;

    public string? RunCommand(string command)
    {
        using var cmd = Client.CreateCommand(command);
        if (cmd == null) throw new NullReferenceException($"{Client.GetType().FullNameFormatted()}.{nameof(Client.CreateCommand)}('{command}')");
        return cmd.Execute();
    }

    #region Create Clients

    public static T CreateClient<T>(SshConfig config, ILoggerFactory loggerProvider) where T : BaseClient =>
        CreateClient<T>(config, loggerProvider.CreateLogger<Ssh>());

    public static T CreateClient<T>(SshConfig config, ILogger log) where T : BaseClient
    {
        var host = config.Host;
        var port = config.Port;
        if (port == 0) port = 22;
        var username = config.Username.CheckNotNullTrimmed(nameof(config.Username));
        var password = config.Password.TrimOrNull();
        var pks = config.PrivateKeys.ToList();

        if (password == null && pks.Count == 0) throw new ArgumentException("No password provided and no private keys provided.", nameof(config.Password));
        if (password != null && pks.Count > 0) throw new ArgumentException("Private keys are not supported when a password is supplied.", nameof(config.PrivateKeys));

        var privateKeyFiles = new List<PrivateKeyFile>();
        foreach (var pk in pks)
        {
            pk.FileName.CheckFileExists();
            PrivateKeyFile pkf;
            if (pk.Password == null)
            {
                pkf = new(pk.FileName);
                log.LogDebug("Using private key file {FileName}", pk.FileName);
            }
            else
            {
                pkf = new(pk.FileName, pk.Password);
                log.LogDebug("Using (password protected) private key file {FileName}", pk.FileName);
            }

            privateKeyFiles.Add(pkf);
        }

        BaseClient? client = null;
        var clientType = typeof(T);
        if (clientType == typeof(SshClient)) client = password == null ? new(host, port, username, privateKeyFiles.ToArray()) : new SshClient(host, port, username, password);
        if (clientType == typeof(SftpClient)) client = password == null ? new(host, port, username, privateKeyFiles.ToArray()) : new SftpClient(host, port, username, password);
        if (client == null) throw new NotImplementedException($"Cannot create SSH Client for type {clientType.FullNameFormatted()}");

        var ci = client.ConnectionInfo;
        if (ci == null) throw new NullReferenceException();
        ci.Timeout = config.Timeout;
        ci.ChannelCloseTimeout = config.ChannelCloseTimeout;
        ci.Encoding = config.Encoding;
        ci.RetryAttempts = config.RetryAttempts;
        ci.MaxSessions = config.MaxSessions;

        try
        {
            log.LogDebug("Connecting {Type} to server {Host}:{Port} with username '{Username}'", clientType.NameFormatted(), host, port, username);
            client.Connect();
            log.LogDebug("Connection successful");
        }
        catch (Exception)
        {
            Dispose(client, log);
            throw;
        }

        return (T)client;
    }

    #endregion Create Clients

    public void Dispose()
    {
        var c = client;
        client = null;
        Dispose(c, log);
    }

    public static void Dispose(BaseClient? client, ILogger log)
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
}
