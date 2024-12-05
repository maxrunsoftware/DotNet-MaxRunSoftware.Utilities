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

namespace MaxRunSoftware.Utilities.Ftp;

public interface IFtpClient : IDisposable
{
    string? ServerInfo { get; }

    /// <summary>
    /// The current directory on the remote host. It is expected to return an absolute path.
    /// </summary>
    string WorkingDirectory { get; set; }

    /// <summary>
    /// The separator between multiple directories and directory and file name.
    /// </summary>
    string DirectorySeparator { get; }

    public bool IsDisposed { get; }

    void GetFile(string remoteFile, Stream stream, Action<FtpClientProgress>? handlerProgress, bool flushStream);

    void PutFile(string remoteFile, Stream stream, Action<FtpClientProgress>? handlerProgress);

    void DeleteFile(string remoteFile);

    FtpClientRemoteObject CreateDirectory(string remotePath);

    bool DeleteDirectory(string remotePath);

    IEnumerable<FtpClientRemoteObject> ListObjects(string remotePath, bool recursive, Func<string, Exception, bool>? handlerException);

    FtpClientRemoteObject? GetObject(string remotePath);

    string GetAbsolutePath(string remotePath);
}
