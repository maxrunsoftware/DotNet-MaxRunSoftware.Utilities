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

#nullable enable

public abstract class FtpClientTests<T> : TestBase where T : FtpClientBase
{
    protected T Client { get; private set; }
    protected FtpClientTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Client = CreateClient();
    }

    public override void Dispose()
    {
        var c = Client;
        Client = null!;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        c?.Dispose();

        base.Dispose();
    }

    protected abstract T CreateClient();



    [SkippableFact]
    public void WorkingDirectory() => log.LogInformationMethod(new(), "{WorkingDirectory}", Client.WorkingDirectory);

    [SkippableFact]
    public void WorkingDirectory_Not_Null() => Assert.NotNull(Client.WorkingDirectory);

    [SkippableFact]
    public void WorkingDirectory_Starts_With_DirectorySeparator() => Assert.StartsWith(Client.DirectorySeparator, Client.WorkingDirectory);

    [SkippableFact]
    public void WorkingDirectory_Does_Not_End_With_DirectorySeparator()
    {
        var wd = Client.WorkingDirectory;
        Assert.NotNull(wd);
        if (wd != Client.DirectorySeparator)
        {
            Assert.False(wd.EndsWith(Client.DirectorySeparator));
        }
    }

    [SkippableFact]
    public void ServerInfo() => log.LogInformationMethod(new(), "{ServerInfo}", Client.ServerInfo);

    [SkippableFact]
    public void PutFile()
    {
        var _ = CreateTestWorkingDirectory();
        Assert.Empty(GetFiles());

        HelperDeleteFile("myfile.txt");
        var file = GetFile("myfile.txt");
        Assert.Null(file);
        Client.PutFile("myfile.txt", ToBytes("Hello World"));
        file = GetFile("myfile.txt");
        WriteLine(nameof(Client.PutFile) + ": " + file);
        Assert.NotNull(file);
    }

    [SkippableFact]
    public void PutFile_GetFile()
    {
        CreateTestWorkingDirectory();

        var file = GetFile("myfile.txt");
        Assert.Null(file);
        var fileOldString = "Hello World";
        var fileOldBytes = ToBytes(fileOldString);
        Client.PutFile("myfile.txt", fileOldBytes);
        file = GetFile("myfile.txt");
        Assert.NotNull(file);
        WriteLine(nameof(Client.PutFile) + ": " + file);

        var fileNewBytes = Client.GetFile("myfile.txt");
        Assert.NotEmpty(fileNewBytes);
        Assert.Equal(fileOldBytes.Length, fileNewBytes.Length);
        Assert.True(fileOldBytes.EqualsBytes(fileNewBytes));

        var fileNewString = ToString(fileNewBytes);
        Assert.Equal(fileOldString, fileNewString);
    }


    [SkippableFact]
    public void PutFile_Auto_Create_SubDir_Multiple()
    {
        var dirName = CallerInfo.GetName()!;
        var dir = GetDirectory(dirName);
        if (dir != null)
        {
            Client.DeleteDirectory(dirName);
            dir = GetDirectory(dirName);
        }
        Assert.Null(dir);

        Client.PutFile(ConvertPath($"{dirName}/1/myfile1.txt"), ToBytes("My File 1"));
        Client.PutFile(ConvertPath($"{dirName}/1/2/myfile2.txt"), ToBytes("My File 2"));
        Client.PutFile(ConvertPath($"{dirName}/1/2/3/myfile3.txt"), ToBytes("My File 3"));

        Client.PutFile(ConvertPath($"{dirName}/4/5/6/myfile6.txt"), ToBytes("My File 6"));
        var files = GetFilesRecursive(dirName);

        var wd = Client.WorkingDirectory;
        Assert.Equal(4, files.Length);
        Assert.Contains(files, o => o.NameFull == ConvertPath($"{wd}/{dirName}/1/myfile1.txt"));
        Assert.Contains(files, o => o.NameFull == ConvertPath($"{wd}/{dirName}/1/2/myfile2.txt"));
        Assert.Contains(files, o => o.NameFull == ConvertPath($"{wd}/{dirName}/1/2/3/myfile3.txt"));
        Assert.Contains(files, o => o.NameFull == ConvertPath($"{wd}/{dirName}/4/5/6/myfile6.txt"));
    }

    [SkippableFact]
    public void WorkingDirectory_Change_To_Dot()
    {
        var wd1 = Client.WorkingDirectory;
        log.LogInformationMethod(new(), "wd1: {WorkingDirectory}", wd1);

        Client.WorkingDirectory = ".";
        var wd2 = Client.WorkingDirectory;
        log.LogInformationMethod(new(), "wd2: {WorkingDirectory}", wd2);
        Assert.Equal(wd1, wd2);

        Client.WorkingDirectory = ConvertPath("././././.");
        var wd3 = Client.WorkingDirectory;
        log.LogInformationMethod(new(), "wd3: {WorkingDirectory}", wd3);
        Assert.Equal(wd1, wd3);
    }

    [SkippableFact]
    public void GetAbsolutePath()
    {
        var dir = CreateTestWorkingDirectory();
        var v = ConvertPath($"{dir.NameFull}/dir1/../dir1/./dir2/.");
        var p = Client.GetAbsolutePath(v);
        log.LogInformationMethod(new(), "{Original} --> {Absolute}", v, p);
        Assert.Equal($"{dir.NameFull}/dir1/dir2", p);


    }

    [SkippableFact]
    public void Create_Directory_That_Already_Exists_Does_Not_Throw_Error()
    {
        var dirNameFull = GetTestDirectoryNameFull();

        if (Client.DirectoryExists(dirNameFull)) Client.DeleteDirectory(dirNameFull);
        Client.CreateDirectory(dirNameFull);
        var dir = GetDirectory(dirNameFull);
        Assert.NotNull(dir);

        // actual test
        Client.CreateDirectory(dirNameFull);
        dir = GetDirectory(dirNameFull);
        Assert.NotNull(dir);
    }

    [SkippableFact]
    public void PutFile_Auto_Create_SubDir()
    {
        var _ = CreateTestWorkingDirectory();
        var dirSampleName = "sample";

        Client.PutFile(ConvertPath($"./{dirSampleName}/myfile1.txt"), ToBytes("My File 1"));
        Client.PutFile(ConvertPath($"{dirSampleName}/myfile2.txt"), ToBytes("My File 2"));

        var files = GetFiles(dirSampleName);

        Assert.Equal(2, files.Length);
        Assert.Contains(files, o => o.Name == "myfile1.txt");
        Assert.Contains(files, o => o.Name == "myfile2.txt");
    }

    [SkippableFact]
    public void Directory_Operations()
    {
        var dir = CreateTestWorkingDirectory();
        Assert.Equal(dir.NameFull, Client.WorkingDirectory);

        Client.WorkingDirectory = "..";
        Assert.DoesNotContain(Client.DirectorySeparator + dir.Name, Client.WorkingDirectory);

        Client.PutFile(ConvertPath($"./{dir.Name}/myfile1.txt"), Constant.Encoding_UTF8_Without_BOM.GetBytes("My File 1"));
        Client.PutFile(ConvertPath($"{dir.Name}/myfile2.txt"), Constant.Encoding_UTF8_Without_BOM.GetBytes("My File 2"));
        Client.WorkingDirectory = ConvertPath($"./{dir.Name}");
        Client.PutFile("myfile3.txt", Constant.Encoding_UTF8_Without_BOM.GetBytes("My File 3"));
        Client.WorkingDirectory = "..";

        var files = Client.ListObjects(dir.Name)
            .Where(o => o.Type == FtpClientRemoteFileSystemObjectType.File)
            .Where(o => o.Name.In("myfile1.txt", "myfile2.txt", "myfile3.txt"))
            .ToList();

        Assert.Equal(3, files.Count);
    }

    #region Helpers

    protected string ConvertPath(string unixPath) => unixPath.Replace("/", Client.DirectorySeparator);

    protected static byte[] ToBytes(string str) => Constant.Encoding_UTF8_Without_BOM.GetBytes(str);
    protected static string ToString(byte[] bytes) => Constant.Encoding_UTF8_Without_BOM.GetString(bytes);

    private void HelperDeleteFile(string filename)
    {
        try
        {
            if (GetFile(filename) != null) Client.DeleteFile(filename);
        }
        catch (Exception)
        {
            //output.WriteLine($"Error deleting file {filename}");
            //output.WriteLine(e.ToString());
        }
    }

    private FtpClientRemoteFileSystemObject? GetFile(string fileName)
    {
        var o = Client.GetObject(fileName);
        return o?.Type == FtpClientRemoteFileSystemObjectType.File ? o : null;
    }

    private FtpClientRemoteFileSystemObject? GetDirectory(string directoryName)
    {
        var o = Client.GetObject(directoryName);
        return o?.Type == FtpClientRemoteFileSystemObjectType.Directory ? o : null;
    }

    private FtpClientRemoteFileSystemObject[] GetFiles(string? directoryName = null) =>
        Client.ListObjects(directoryName ?? Client.WorkingDirectory).Where(o => o.Type == FtpClientRemoteFileSystemObjectType.File).ToArray();

    private FtpClientRemoteFileSystemObject[] GetFilesRecursive(string? directoryName = null) =>
        Client.ListObjects(directoryName ?? Client.WorkingDirectory, true).Where(o => o.Type == FtpClientRemoteFileSystemObjectType.File).ToArray();



    //private string GetTestDirectoryName([CallerMemberName] string? directoryName = null) => directoryName.CheckNotNull();
    private string GetTestDirectoryNameFull([CallerMemberName] string? directoryName = null) => Client.GetAbsolutePath(directoryName.CheckNotNull());
    private FtpClientRemoteFileSystemObject CreateTestWorkingDirectory([CallerMemberName] string? directoryName = null)
    {
        Assert.NotNull(directoryName);
        directoryName = directoryName.CheckNotNull();

        using (LogDisable())
        {
            directoryName = Client.GetAbsolutePath(directoryName);
            if (Client.DirectoryExists(directoryName)) Client.DeleteDirectory(directoryName);
            Client.CreateDirectory(directoryName);
            var obj = Client.GetObject(directoryName);
            Assert.NotNull(obj);

            Client.WorkingDirectory = directoryName;
            Assert.Empty(Client.ListObjects(directoryName, true));
            return obj;
        }
    }



    #endregion Helpers


}
