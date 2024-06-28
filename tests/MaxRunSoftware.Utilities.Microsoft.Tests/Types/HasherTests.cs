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

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MaxRunSoftware.Utilities.Microsoft.Tests;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
public class HasherTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableFact]
    public void Can_Hash()
    {
        
        var hashers = Hasher.Factories
                .Select(o => o.Key)
                .Select(Hasher.GetHasher)
                .OrderBy(o => o.Name, Constant.StringComparer_OrdinalIgnoreCase_Ordinal)
                .ToArray()
            ;
        
        foreach (var hasher in hashers)
        {
            WriteLine("");    
        }

    }
    
    [SkippableFact]
    public void Zero_Bytes_Hash()
    {
        var hashers = Hasher.Factories
                .Select(o => o.Key)
                .Select(Hasher.GetHasher)
                .OrderBy(o => o.Name, Constant.StringComparer_OrdinalIgnoreCase_Ordinal)
                .ToArray()
            ;

        var bytes = Array.Empty<byte>();
        foreach (var hasher in hashers)
        {
            var hash = hasher.Hash(bytes);
            log.LogInformation("{Hasher} -> [{Length}] -> {Hash16}  {Hash32}  {Hash64}", hasher.Name, hash.Length, Util.Base16(hash), Util.Base32(hash), Util.Base64(hash));    
        }

    }
    
    [SkippableFact]
    public void Zero_Bytes_Hash_Base64()
    {
        var hashers = Hasher.Factories
                .Select(o => o.Key)
                .Select(Hasher.GetHasher)
                .OrderBy(o => o.Name, Constant.StringComparer_OrdinalIgnoreCase_Ordinal)
                .ToArray()
            ;

        var bytes = Array.Empty<byte>();
        foreach (var hasher in hashers)
        {
            var hash = hasher.Hash(bytes);
            var hashHex = Util.Base64(hash);
            log.LogInformation("{Hasher} -> [{Length}] -> {Hash}", hasher.Name, hash.Length, hashHex);    
        }

    }
    
    [SkippableTheory]
    [InlineData("/Users/user/Temp/testitem.mp4", 1)]
    [InlineData("/Users/user/Temp/testitem.mp4", 3)]
    [InlineData("/Users/user/Temp/testitem.mp4", 5)]
    public void Hash_File_Performance(string filename, int loopCount)
    {
        LogLevel = LogLevel.Debug;
        filename.CheckFileExists();
        
        var hashers = Hasher.Factories
                .Select(o => o.Key)
                .Select(Hasher.GetHasher)
                .OrderBy(o => o.Name, Constant.StringComparer_OrdinalIgnoreCase_Ordinal)
                .ToArray()
            ;

        var fileinfo = new FileInfo(filename);
        var results = new Dictionary<string, List<TimeSpan>>();
        var stopwatch = new Stopwatch();
        var padding = hashers.Select(o => o.Name).MaxLength();
        
        for (var i = 0; i < loopCount; i++)
        {
            foreach (var hasher in hashers)
            {
                stopwatch.Restart();
                var hash = hasher.Hash(fileinfo);
                stopwatch.Stop();
                var hashhex = Util.Base16(hash);
                log.LogDebug("{File} {Hasher} {Time} ms   -> {Hash}", fileinfo.Name, hasher.Name.PadLeft(padding), stopwatch.ElapsedMilliseconds, hashhex);
                results.AddToList(hasher.Name, stopwatch.Elapsed);
            }
        }

        var resultsMedian = new Dictionary<string, TimeSpan>();
        foreach (var (k, v) in results)
        {
            resultsMedian.Add(k, GetMedian(v));
            static TimeSpan GetMedian(IEnumerable<TimeSpan> items) {
                // https://stackoverflow.com/a/8328226
                var sortedNumbers = items.Select(o => o.Ticks).ToArray();
                Array.Sort(sortedNumbers);

                //get the median
                var size = sortedNumbers.Length;
                var mid = size / 2;
                long median;
                if (size == 1)
                {
                    median = sortedNumbers[0];
                }
                else if (size % 2 == 0)
                {
                    median = sortedNumbers[mid];
                    median += sortedNumbers[mid - 1];
                    median /= 2;
                }
                else
                {
                    median = sortedNumbers[mid];
                }
                return TimeSpan.FromTicks(median);
            }
        }
        
        
        log.LogInformation("{File}", fileinfo.FullName);
        foreach (var (k, v) in resultsMedian.OrderBy(o => o.Value))
        {
            log.LogInformation("  {Hasher} -> {Time} ms", k.PadLeft(padding), v.TotalMilliseconds);
        }
        

    }

   
}
