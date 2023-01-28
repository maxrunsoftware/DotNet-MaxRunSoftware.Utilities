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

namespace MaxRunSoftware.Utilities.Ftp;

public interface IFtpClient : IDisposable
{
    string? ServerInfo { get; }

    /// <summary>
    /// The current directory on the remote host. It is expected to be an absolute path.
    /// </summary>
    string WorkingDirectory { get; set; }

    string DirectorySeparator { get; }

    byte[] GetFile(string remoteFile, Action<FtpClientProgress>? handlerProgress);

    void GetFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress);

    void PutFile(string remoteFile, byte[] data, Action<FtpClientProgress>? handlerProgress);

    void PutFile(string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress);

    void DeleteFile(string remoteFile);

    void CreateDirectory(string remotePath);

    void DeleteDirectory(string remotePath);

    IEnumerable<FtpClientRemoteFileSystemObject> ListObjects(string? remotePath);

    IEnumerable<FtpClientRemoteFileSystemObject> ListObjectsRecursive(string? remotePath, Func<string, Exception, bool>? handlerException);

    bool DirectoryExists(string remotePath);

    bool FileExists(string remotePath);

}


public static class FtpClientExtensions
{
    public static byte[] GetFile(this IFtpClient client, string remoteFile) => client.GetFile(remoteFile, null);

    public static void GetFile(this IFtpClient client, string remoteFile, string localFile) => client.GetFile(remoteFile, localFile, null);

    public static void PutFile(this IFtpClient client, string remoteFile, string localFile) => client.PutFile(remoteFile, localFile, null);

    public static void PutFile(this IFtpClient client, string remoteFile, byte[] data) => client.PutFile(remoteFile, data, null);

    public static IEnumerable<FtpClientRemoteFileSystemObject> ListObjectsRecursive(this IFtpClient client, string? remotePath) => client.ListObjectsRecursive(remotePath, null);
}
