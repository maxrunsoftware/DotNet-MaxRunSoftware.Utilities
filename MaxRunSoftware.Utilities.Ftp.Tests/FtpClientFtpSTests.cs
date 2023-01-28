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

using MaxRunSoftware.Utilities.Common;

namespace MaxRunSoftware.Utilities.Ftp.Tests;

public class FtpClientFtpSTests : FtpClientTests<FtpClientFtp>
{
    // ReSharper disable once InconsistentNaming
    private static readonly object skipMessageLock = new();
    private static int skipMessage = -1;
    public static string? SkipMessage
    {
        get
        {
            int i;
            lock (skipMessageLock)
            {
                i = skipMessage;
                if (i < 0)
                {
                    i = 0;
                    var fluentFtpVersion = typeof(FluentFTP.FtpClient).Assembly.GetVersion().TrimOrNull() ?? "0.0.0.0";
                    var fluentFtpFileVersion = typeof(FluentFTP.FtpClient).Assembly.GetFileVersion().TrimOrNull() ?? "0.0.0.0";
                    if (fluentFtpVersion.StartsWith("44.0.1.") || fluentFtpFileVersion.StartsWith("44.0.1."))
                    {
                        i = 1;
                    }

                    skipMessage = i;
                }
            }

            switch (i)
            {
                case 0: return null;
                case 1: return "FluentFtp 44.0.1 is broken, waiting for next version in NuGet  https://github.com/robinrodricks/FluentFTP/issues/1156";
                default: throw new NotImplementedException();
            }

        }
    }

    public FtpClientFtpSTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        Skip.If(SkipMessage != null, SkipMessage);
    }

    protected override FtpClientFtp CreateClient() => new(new()
    {
        Host = Constants.FTPS_HOST,
        Port = Constants.FTPS_PORT,
        Username = Constants.FTPS_USERNAME,
        Password = Constants.FTPS_PASSWORD,
        WorkingDirectory = Constants.FTPS_DIRECTORY,
        // ReSharper disable once UnusedParameter.Local
        ValidateCertificate = info => true,
        FtpConfig =
        {
            ValidateAnyCertificate = false,
            ConnectTimeout = 1000 * 5,
            ReadTimeout = 1000 * 5,
        },
    }, loggerProvider);
}
