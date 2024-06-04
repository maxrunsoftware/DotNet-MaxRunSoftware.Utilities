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

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using FluentFTP;

namespace MaxRunSoftware.Utilities.Ftp;

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
