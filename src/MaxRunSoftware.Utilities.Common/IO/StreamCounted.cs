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

/// <summary>
/// Simple stream wrapper that counts the bytes read or written.
/// Another example: https://stackoverflow.com/a/57439154
/// </summary>
/// <param name="stream"></param>
public class StreamCounted(Stream stream) : StreamWrapped(stream)
{
    public long BytesRead { get; private set; }
    public override int Read(byte[] buffer, int offset, int count)
    {
        var c = stream.Read(buffer, offset, count);
        BytesRead += c;
        return c;
    }
    
    
    public long BytesWritten { get; private set; }
    public override void Write(byte[] buffer, int offset, int count)
    {
        stream.Write(buffer, offset, count);
        BytesWritten += count;
    }
}

public sealed class StreamCountedVolatile(Stream stream) : StreamWrapped(stream)
{
    private long bytesRead;
    public long BytesRead => Volatile.Read(ref bytesRead);
    public override int Read(byte[] buffer, int offset, int count)
    {
        var c = stream.Read(buffer, offset, count);
        Interlocked.Add(ref bytesRead, c);
        return c;
    }

    
    private long bytesWritten;
    public long BytesWritten => Volatile.Read(ref bytesWritten);
    public override void Write(byte[] buffer, int offset, int count)
    {
        stream.Write(buffer, offset, count);
        Interlocked.Add(ref bytesWritten, count);
    }
}
