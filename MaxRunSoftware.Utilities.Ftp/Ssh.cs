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

[PublicAPI]
public class SshConfig
{
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 22;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public List<SshKeyFile> PrivateKeys { get; set; } = new();

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ChannelCloseTimeout { get; set; } = TimeSpan.FromSeconds(1);
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public int RetryAttempts { get; set; } = 10;
    public int MaxSessions { get; set; } = 10;

    public void Load(IReadOnlyDictionary<string, string?> dictionary)
    {
        LoadCaseInsensitive(new Dictionary<string, string?>(dictionary, StringComparer.OrdinalIgnoreCase));
    }

    public void Load(IDictionary<string, string?> dictionary)
    {
        LoadCaseInsensitive(new Dictionary<string, string?>(dictionary, StringComparer.OrdinalIgnoreCase));
    }

    protected virtual void LoadCaseInsensitive(Dictionary<string, string?> dictionary)
    {
        if (dictionary.TryGetValue(nameof(Host), out var host)) Host = host.CheckNotNullTrimmed();
        if (dictionary.TryGetValue(nameof(Port), out var port)) Port = port.CheckNotNullTrimmed().ToUShort();
        if (dictionary.TryGetValue(nameof(Username), out var username)) Username = username;
        if (dictionary.TryGetValue(nameof(Password), out var password)) Password = password;
        // TODO: PrivateKeys

        if (dictionary.TryGetValue(nameof(Timeout), out var timeout)) Timeout = timeout == null ? new SshConfig().Timeout : TimeSpan.Parse(timeout);
        if (dictionary.TryGetValue(nameof(ChannelCloseTimeout), out var channelCloseTimeout)) ChannelCloseTimeout = channelCloseTimeout == null ? new SshConfig().ChannelCloseTimeout : TimeSpan.Parse(channelCloseTimeout);
        //if (dictionary.TryGetValue(nameof(Encoding), out var encoding)) Encoding = Util.ParseEncoding()

        if (dictionary.TryGetValue(nameof(RetryAttempts), out var retryAttempts)) RetryAttempts = retryAttempts.ToIntNullable() ?? new SshConfig().RetryAttempts;



    }
}

public sealed class SshKeyFile
{
    public string FileName { get; }
    public string? Password { get; }

    public SshKeyFile(string fileName, string? password = null)
    {
        FileName = Path.GetFullPath(fileName.CheckNotNullTrimmed(nameof(fileName)));
        Password = password.TrimOrNull();
    }
}

public class Ssh : IDisposable
{
    public Ssh(SshConfig config)
    {
        client = CreateClient<SshClient>(config);
    }

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

    public static T CreateClient<T>(SshConfig config) where T : BaseClient
    {
        var log = GetLogger();
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
                pkf = new PrivateKeyFile(pk.FileName);
                log.LogDebug("Using private key file {FileName}", pk.FileName);
            }
            else
            {
                pkf = new PrivateKeyFile(pk.FileName, pk.Password);
                log.LogDebug("Using (password protected) private key file {FileName}", pk.FileName);
            }

            privateKeyFiles.Add(pkf);
        }

        BaseClient? client = null;
        var clientType = typeof(T);
        if (clientType == typeof(SshClient)) client = password == null ? new SshClient(host, port, username, privateKeyFiles.ToArray()) : new SshClient(host, port, username, password);
        if (clientType == typeof(SftpClient)) client = password == null ? new SftpClient(host, port, username, privateKeyFiles.ToArray()) : new SftpClient(host, port, username, password);
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
            Dispose(client);
            throw;
        }

        return (T)client;
    }

    #endregion Create Clients

    private static ILogger GetLogger() => Constant.GetLogger<Ssh>();

    public void Dispose()
    {
        var c = client;
        client = null;
        Dispose(c);
    }

    public static void Dispose(BaseClient? client)
    {
        if (client == null) return;
        var log = GetLogger();

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
