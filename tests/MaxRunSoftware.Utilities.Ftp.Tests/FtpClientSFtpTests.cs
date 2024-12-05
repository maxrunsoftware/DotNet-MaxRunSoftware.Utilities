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

namespace MaxRunSoftware.Utilities.Ftp.Tests;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
public class FtpClientSFtpTests(ITestOutputHelper testOutputHelper) : FtpClientTests<FtpClientSFtp>(testOutputHelper)
{
    protected override FtpClientSFtp CreateClient() => new(new()
    {
        Host = TestConfig.SFTP_HOST,
        Port = TestConfig.SFTP_PORT,
        Username = TestConfig.SFTP_USERNAME,
        Password = TestConfig.SFTP_PASSWORD,
        WorkingDirectory = TestConfig.SFTP_DIRECTORY,
    }, LoggerProvider);
}
