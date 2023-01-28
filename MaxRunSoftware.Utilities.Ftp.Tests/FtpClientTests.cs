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

using System.Runtime.CompilerServices;
using MaxRunSoftware.Utilities.Common;
// ReSharper disable StringLiteralTypo
// ReSharper disable AssignNullToNotNullAttribute

namespace MaxRunSoftware.Utilities.Ftp.Tests;

public abstract class FtpClientTests<T> : TestBase, IDisposable where T : FtpClientBase
{
    protected T Client { get; private set; }
    protected FtpClientTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Client = CreateClient();
    }

    protected abstract T CreateClient();



    [Fact]
    public void WorkingDirectory()
    {
        var o = Client.WorkingDirectory;
        Assert.NotNull(o);
        output.WriteLine(nameof(Client.WorkingDirectory) + ": " + o);
    }

    [Fact]
    public void ServerInfo()
    {
        var o = Client.ServerInfo;
        output.WriteLine(nameof(Client.ServerInfo) + ": " + o);
    }

    [Fact]
    public void PutFile()
    {
        HelperDeleteFile("myfile.txt");
        var file = HelperGetFile("myfile.txt");
        Assert.Null(file);
        Client.PutFile("myfile.txt", Constant.Encoding_UTF8.GetBytes("Hello World"));
        file = HelperGetFile("myfile.txt");
        output.WriteLine(nameof(Client.PutFile) + ": " + file);
        Assert.NotNull(file);
    }

    [Fact]
    public void PutFile_GetFile()
    {
        HelperDeleteFile("myfile.txt");
        var file = HelperGetFile("myfile.txt");
        Assert.Null(file);
        var fileOldString = "Hello World";
        var fileOldBytes = Constant.Encoding_UTF8.GetBytes(fileOldString);
        Client.PutFile("myfile.txt", fileOldBytes);
        file = HelperGetFile("myfile.txt");
        Assert.NotNull(file);
        output.WriteLine(nameof(Client.PutFile) + ": " + file);

        var fileNewBytes = Client.GetFile("myfile.txt");
        Assert.NotEmpty(fileNewBytes);
        Assert.Equal(fileOldBytes.Length, fileNewBytes.Length);
        Assert.True(fileOldBytes.EqualsBytes(fileNewBytes));

        var fileNewString = Constant.Encoding_UTF8.GetString(fileNewBytes);
        Assert.Equal(fileOldString, fileNewString);
    }

    protected static byte[] ToBytes(string str) => Constant.Encoding_UTF8.GetBytes(str);
    protected static string ToString(byte[] bytes) => Constant.Encoding_UTF8.GetString(bytes);


    [Fact]
    public void PutFile_Auto_Create_SubDir_Multiple()
    {
        var dirName = CallerInfo.GetName()!;
        var dir = HelperGetDirectory(dirName);
        if (dir != null) Client.DeleteDirectory(dirName);
        dir = HelperGetDirectory(dirName);
        Assert.Null(dir);

        Client.PutFile($"{dirName}/1/myfile1.txt", ToBytes("My File 1"));
        Client.PutFile($"{dirName}/1/2/myfile2.txt", ToBytes("My File 2"));
        Client.PutFile($"{dirName}/1/2/3/myfile3.txt", ToBytes("My File 3"));

        Client.PutFile($"{dirName}/4/5/6/myfile6.txt", ToBytes("My File 6"));
        var files = HelperGetFilesRecursive(dirName);

        var wd = Client.WorkingDirectory;
        Assert.Equal(4, files.Length);
        Assert.Contains(files, o => o.FullName == $"{wd}/{dirName}/1/myfile1.txt");
        Assert.Contains(files, o => o.FullName == $"{wd}/{dirName}/1/2/myfile2.txt");
        Assert.Contains(files, o => o.FullName == $"{wd}/{dirName}/1/2/3/myfile3.txt");
        Assert.Contains(files, o => o.FullName == $"{wd}/{dirName}/4/5/6/myfile6.txt");
    }

    [Fact]
    public void Create_Directory_That_Already_Exists_Does_Not_Throw_Error()
    {
        var dirName = HelperDeleteDirectoryIfExist();

        Client.CreateDirectory(dirName);
        var dir = HelperGetDirectory(dirName);
        Assert.NotNull(dir);

        // actual test
        Client.CreateDirectory(dirName);
        dir = HelperGetDirectory(dirName);
        Assert.NotNull(dir);
    }

    [Fact]
    public void PutFile_Auto_Create_SubDir()
    {
        var dirName = CallerInfo.GetName()!;
        var dir = HelperGetDirectory(dirName);
        if (dir != null) Client.DeleteDirectory(dirName);
        dir = HelperGetDirectory(dirName);
        Assert.Null(dir);

        Client.PutFile($"./{dirName}/myfile1.txt", ToBytes("My File 1"));
        Client.PutFile($"{dirName}/myfile2.txt", ToBytes("My File 2"));

        var files = HelperGetFiles(dirName);

        Assert.Equal(2, files.Length);
        Assert.Contains(files, o => o.Name == "myfile1.txt");
        Assert.Contains(files, o => o.Name == "myfile2.txt");
    }

    [Fact]
    public void Directory_Operations()
    {
        var dir = HelperGetDirectory("mydir");
        if (dir != null) Client.DeleteDirectory("mydir");

        Client.CreateDirectory("mydir");
        dir = HelperGetDirectory("mydir");
        Assert.NotNull(dir);

        Client.PutFile("./mydir/myfile1.txt", Constant.Encoding_UTF8.GetBytes("My File 1"));
        Client.PutFile("mydir/myfile2.txt", Constant.Encoding_UTF8.GetBytes("My File 2"));
        Client.WorkingDirectory = "./mydir";
        Client.PutFile("myfile3.txt", Constant.Encoding_UTF8.GetBytes("My File 3"));
        Client.WorkingDirectory = "..";

        var files = Client.ListObjects("mydir")
            .Where(o => o.Type == FtpClientRemoteFileSystemObjectType.File)
            .Where(o => o.Name.In("myfile1.txt", "myfile2.txt", "myfile3.txt"))
            .ToList();

        Assert.Equal(3, files.Count);
    }

    private void HelperDeleteFile(string filename)
    {
        try
        {
            Client.DeleteFile(filename);
        }
        catch (Exception)
        {
            //output.WriteLine($"Error deleting file {filename}");
            //output.WriteLine(e.ToString());
        }
    }

    private FtpClientRemoteFileSystemObject? HelperGetFile(string filename) =>
        Client.ListObjects(null).FirstOrDefault(o => o.Type == FtpClientRemoteFileSystemObjectType.File && o.Name == filename);

    private FtpClientRemoteFileSystemObject[] HelperGetFiles(string? directoryName = null) =>
        Client.ListObjects(directoryName).Where(o => o.Type == FtpClientRemoteFileSystemObjectType.File).ToArray();

    private FtpClientRemoteFileSystemObject[] HelperGetFilesRecursive(string? directoryName = null) =>
        Client.ListObjectsRecursive(directoryName).Where(o => o.Type == FtpClientRemoteFileSystemObjectType.File).ToArray();

    private FtpClientRemoteFileSystemObject? HelperGetDirectory(string directoryName) =>
        Client.ListObjects(null).FirstOrDefault(o => o.Type == FtpClientRemoteFileSystemObjectType.Directory && o.Name == directoryName);

    private string HelperDeleteDirectoryIfExist([CallerMemberName] string? directoryName = null)
    {
        Assert.NotNull(directoryName);
        directoryName = directoryName.CheckNotNull();

        LogDisable();
        var dir = HelperGetDirectory(directoryName);
        if (dir != null) Client.DeleteDirectory(directoryName);
        dir = HelperGetDirectory(directoryName);
        LogEnable();

        Assert.Null(dir);
        return directoryName;
    }
    public void Dispose()
    {
        var c = Client;
        Client = null!;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        c?.Dispose();
    }
}
