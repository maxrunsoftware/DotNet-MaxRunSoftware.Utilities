// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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
