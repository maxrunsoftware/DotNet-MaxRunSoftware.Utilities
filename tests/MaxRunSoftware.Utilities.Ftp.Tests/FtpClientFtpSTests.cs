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

// ReSharper disable LocalVariableHidesMember
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

using FluentFTP;

namespace MaxRunSoftware.Utilities.Ftp.Tests;

public class FtpClientFtpSTests : FtpClientTests<FtpClientFtp>
{
    public FtpClientFtpSTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        var fluentFtpVersionBroken = "45.0.4";
        var fluentFtpVersions = new[] { Util.GetAssemblyVersion<FtpClient>(), Util.GetAssemblyFileVersion<FtpClient>() };
        foreach (var version in fluentFtpVersions.TrimOrNull().WhereNotNull())
        {
            Skip.If(
                version.StartsWith(fluentFtpVersionBroken + "."),
                $"FluentFtp {fluentFtpVersionBroken} is broken, waiting for next version in NuGet  https://github.com/robinrodricks/FluentFTP/issues/1156"
            );
        }
    }

    protected override FtpClientFtp CreateClient() => new(new()
    {
        Host = Constants.FTPS_HOST,
        Port = Constants.FTPS_PORT,
        Username = Constants.FTPS_USERNAME,
        Password = Constants.FTPS_PASSWORD,
        WorkingDirectory = Constants.FTPS_DIRECTORY,
        ValidateCertificate = _ => true,
        FtpConfig =
        {
            ValidateAnyCertificate = false,
            ConnectTimeout = 1000 * 10,
            ReadTimeout = 1000 * 10,
        },
    }, LoggerProvider);
}
