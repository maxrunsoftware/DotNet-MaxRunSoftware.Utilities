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
