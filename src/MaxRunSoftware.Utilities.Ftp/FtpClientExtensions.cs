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

namespace MaxRunSoftware.Utilities.Ftp;

public static class FtpClientExtensions
{
    #region GetFile

    public static void GetFile(this IFtpClient client, string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress)
    {
        localFile = Path.GetFullPath(localFile);
        if (File.Exists(localFile))
        {
            //log.LogInformationMethod(new(remoteFile, localFile), "Deleting existing local file: {LocalFile}", localFile);
            File.Delete(localFile);
        }

        using (var localStream = Util.FileOpenWrite(localFile))
        {
            client.GetFile(remoteFile, localStream, handlerProgress, true);
        }
    }

    public static byte[] GetFile(this IFtpClient client, string remoteFile, Action<FtpClientProgress>? handlerProgress)
    {
        using (var localStream = new MemoryStream())
        {
            client.GetFile(remoteFile, localStream, handlerProgress, true);
            return localStream.ToArray();
        }
    }

    public static byte[] GetFile(this IFtpClient client, string remoteFile) => client.GetFile(remoteFile, (Action<FtpClientProgress>?)null);

    public static void GetFile(this IFtpClient client, string remoteFile, string localFile) => client.GetFile(remoteFile, localFile, null);

    #endregion GetFile

    #region PutFile

    public static void PutFile(this IFtpClient client, string remoteFile, byte[] data, Action<FtpClientProgress>? handlerProgress)
    {
        using (var localStream = new MemoryStream(data))
        {
            client.PutFile(remoteFile, localStream, handlerProgress);
        }
    }

    public static void PutFile(this IFtpClient client, string remoteFile, string localFile, Action<FtpClientProgress>? handlerProgress)
    {
        localFile = Path.GetFullPath(localFile);
        localFile.CheckFileExists();

        using (var localStream = Util.FileOpenRead(localFile))
        {
            client.PutFile(remoteFile, localStream, handlerProgress);
            localStream.Flush();
        }
    }

    public static void PutFile(this IFtpClient client, string remoteFile, string localFile) => client.PutFile(remoteFile, localFile, null);

    public static void PutFile(this IFtpClient client, string remoteFile, byte[] data) => client.PutFile(remoteFile, data, null);

    #endregion PutFile

    #region ListObjects

    public static IEnumerable<FtpClientRemoteFileSystemObject> ListObjects(this IFtpClient client, string remotePath, bool recursive) =>
        client.ListObjects(remotePath, recursive, null);

    public static IEnumerable<FtpClientRemoteFileSystemObject> ListObjects(this IFtpClient client, string remotePath) =>
        client.ListObjects(remotePath, false);

    #endregion ListObjects

    #region Exists

    public static bool DirectoryExists(this IFtpClient client, string remotePath) =>
        client.GetObject(remotePath)?.Type == FtpClientRemoteFileSystemObjectType.Directory;

    public static bool FileExists(this IFtpClient client, string remotePath) =>
        client.GetObject(remotePath)?.Type == FtpClientRemoteFileSystemObjectType.File;

    #endregion Exists

    public static FtpClientRemoteFileSystemObject GetObjectParent(this IFtpClient client, FtpClientRemoteFileSystemObject obj)
    {
        if (obj.NameFull == client.DirectorySeparator) return obj; // root "/"
        var (left, _) = obj.NameFull.SplitOnLast(client.DirectorySeparator);
        var objParent = client.GetObject(left ?? client.DirectorySeparator);
        if (objParent != null) return objParent;
        throw new ArgumentException($"Could not determine parent directory of {obj.NameFull}", nameof(obj));
    }
}
