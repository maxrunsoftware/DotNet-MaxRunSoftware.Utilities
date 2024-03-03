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

namespace MaxRunSoftware.Utilities.Ftp.Tests;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo
#nullable enable

public static class Constants
{
    public static readonly ImmutableArray<SkippedTest> IGNORED_TESTS = new[]
    {
        SkippedTest.Create<FtpClientFtpSTests>("no test FTP server"),
        SkippedTest.Create<FtpClientSFtpTests>("no test FTP server"),
    }.ToImmutableArray();

    private static readonly string DEFAULT_SERVER = "172.16.46.16";
    private static readonly string DEFAULT_USERNAME = "testuser";
    private static readonly string DEFAULT_PASSWORD = "testPass1!";

    public static readonly string SFTP_HOST = DEFAULT_SERVER;
    public static readonly ushort SFTP_PORT = 2222;
    public static readonly string SFTP_USERNAME = DEFAULT_USERNAME;
    public static readonly string SFTP_PASSWORD = DEFAULT_PASSWORD;
    public static readonly string SFTP_DIRECTORY = "upload";

    public static readonly string FTPS_HOST = DEFAULT_SERVER;
    public static readonly ushort FTPS_PORT = 2121;
    public static readonly string FTPS_USERNAME = DEFAULT_USERNAME;
    public static readonly string FTPS_PASSWORD = DEFAULT_PASSWORD;
    public static readonly string? FTPS_DIRECTORY = null;
    public static readonly ushort FTPS_PORT_PASSIVE_MIN = 21200;
    public static readonly ushort FTPS_PORT_PASSIVE_MAX = 21299;
}
