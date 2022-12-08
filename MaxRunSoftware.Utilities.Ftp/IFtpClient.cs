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

using System.Text;
using MaxRunSoftware.Utilities.Common;

namespace MaxRunSoftware.Utilities.Ftp;

public class FtpClientProgress
{
    public Percent Progress { get; set; }
    public long BytesTransferred { get; set; }

    public override string ToString() => $"Progress: %{Progress.ToString(0)}  ({BytesTransferred})";
}

public interface IFtpClient : IDisposable
{
    string? ServerInfo { get; }

    string WorkingDirectory { get; }

    void GetFile(string remoteFile, string localFile) => GetFile(remoteFile: remoteFile, localFile: localFile, handlerProgress: null);

    void GetFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress);

    void PutFile(string remoteFile, string localFile) => PutFile(remoteFile: remoteFile, localFile: localFile, handlerProgress: null);

    void PutFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress);

    void DeleteFile(string remoteFile);

    IEnumerable<FtpClientRemoteFile> ListFiles(string? remotePath);
}
