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

using FluentFTP;

namespace MaxRunSoftware.Utilities.Ftp;

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
            SslBuffering = FtpsBuffering.Off,
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
