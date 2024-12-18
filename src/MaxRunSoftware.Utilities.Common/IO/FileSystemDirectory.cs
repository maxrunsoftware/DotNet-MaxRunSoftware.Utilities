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

namespace MaxRunSoftware.Utilities.Common;

public sealed class FileSystemDirectory : FileSystemObject
{
    private class RecursiveObjects(
        ICollection<FileSystemObject> objectsRecursive,
        ICollection<FileSystemDirectory> directoriesRecursive,
        ICollection<FileSystemFile> filesRecursive
    )
    {
        public readonly ICollection<FileSystemObject> objectsRecursive = objectsRecursive;
        public readonly ICollection<FileSystemDirectory> directoriesRecursive = directoriesRecursive;
        public readonly ICollection<FileSystemFile> filesRecursive = filesRecursive;
    }

    private readonly Lzy<long> size;

    public override bool IsExist => Directory.Exists(Path);
    public override long Size => size.Value;
    public override bool IsDirectory => true;

    private readonly Lzy<long> sizeRecursive;
    public long SizeRecursive => sizeRecursive.Value;

    private readonly Lzy<IReadOnlyCollection<FileSystemFile>> files;
    public IReadOnlyCollection<FileSystemFile> Files => files.Value;

    private readonly Lzy<IReadOnlyCollection<FileSystemDirectory>> directories;
    public IReadOnlyCollection<FileSystemDirectory> Directories => directories.Value;

    private readonly Lzy<IReadOnlyCollection<FileSystemObject>> objects;
    public IReadOnlyCollection<FileSystemObject> Objects => objects.Value;

    private readonly Lzy<RecursiveObjects> recursiveObjects;
    public ICollection<FileSystemObject> ObjectsRecursive => recursiveObjects.Value.objectsRecursive;
    public ICollection<FileSystemDirectory> DirectoriesRecursive => recursiveObjects.Value.directoriesRecursive;
    public ICollection<FileSystemFile> FilesRecursive => recursiveObjects.Value.filesRecursive;


    internal FileSystemDirectory(string path) : base(path)
    {
        files = new(() => Directory.GetFiles(Path).Select(o => new FileSystemFile(o)).ToList());
        directories = new(() => Directory.GetDirectories(Path).Select(o => new FileSystemDirectory(o)).ToList());
        objects = new(() =>
        {
            var objs = new List<FileSystemObject>(Files.Count + Directories.Count);
            objs.AddRange(Files);
            objs.AddRange(Directories);
            return objs;
        });

        size = new(() => GetSizes(Files));
        sizeRecursive = new(() => Size + GetSizes(DirectoriesRecursive));
        recursiveObjects = new(GetObjectsRecursive);
    }

    private long GetSizes(IEnumerable<FileSystemObject> enumerable)
    {
        long s = 0;
        //var log = Constant.GetLogger(GetType());
        foreach (var obj in enumerable)
        {
            try { s += obj.Size; }
            catch (Exception e)
            {
                // TODO: log this
                // ReSharper disable once InconsistentLogPropertyNaming
                //log.LogWarning("Error reading file size from {Path}  --> {ExceptionMessage}", Path, e.Message);
                Console.Error.WriteLine(e);
            }
        }

        return s;
    }

    private RecursiveObjects GetObjectsRecursive()
    {
        var setDirectories = new HashSet<FileSystemDirectory>();
        var setFiles = new HashSet<FileSystemFile>();

        var queue = new Queue<FileSystemDirectory>();
        queue.Enqueue(this);
        //var log = Constant.GetLogger(GetType());
        while (queue.Count > 0)
        {
            var currentDirectory = queue.Dequeue();

            // ignore links to other paths
            if (!IsParentOf(currentDirectory)) continue;

            IReadOnlyCollection<FileSystemDirectory> currentDirectoryDirectories = new List<FileSystemDirectory>();
            try
            {
                currentDirectoryDirectories = currentDirectory.Directories;
            }
            catch (Exception e)
            {
                // TODO: log this
                //log.LogWarning("Error reading directory list from {CurrentDirectoryPath}  --> {ExceptionMessage}", currentDirectory.Path, e.Message);
                Console.Error.WriteLine(e);
            }

            IReadOnlyCollection<FileSystemFile> currentDirectoryFiles = new List<FileSystemFile>();
            try
            {
                currentDirectoryFiles = currentDirectory.Files;
            }
            catch (Exception e)
            {
                // TODO: log this
                //log.LogWarning("Error reading directory list from {CurrentDirectoryPath}  --> {ExceptionMessage}", currentDirectory.Path, e.Message);
                Console.Error.WriteLine(e);
            }

            foreach (var d in currentDirectoryDirectories.OrEmpty())
            {
                if (ReferenceEquals(this, d)) continue;
                if (!IsParentOf(d)) continue;
                if (!setDirectories.Add(d)) continue;
                queue.Enqueue(d);
            }

            foreach (var f in currentDirectoryFiles.OrEmpty())
            {
                if (!IsParentOf(f)) continue;
                setFiles.Add(f);
            }
        }

        var listObjects = new List<FileSystemObject>(setDirectories.Count + setFiles.Count);
        listObjects.AddRange(setDirectories);
        listObjects.AddRange(setFiles);

        return new(listObjects, setDirectories, setFiles);
    }
}
