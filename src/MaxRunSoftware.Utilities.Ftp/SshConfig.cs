using Renci.SshNet.Common;

namespace MaxRunSoftware.Utilities.Ftp;

[PublicAPI]
public class SshConfig
{
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 22;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public List<(byte[], string?)> PrivateKeys { get; set; } = [];
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ChannelCloseTimeout { get; set; } = TimeSpan.FromSeconds(1);
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public ushort RetryAttempts { get; set; } = 10;
    public ushort MaxSessions { get; set; } = 10;
    public Func<HostKeyEventArgs, bool> HostKeyCheck { get; set; } = (_) => true;
}
