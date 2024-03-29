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

using Microsoft.Extensions.Logging.Abstractions;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    /// <summary>
    /// Case-insensitive mapping of strings to LogLevels
    /// </summary>
    public static readonly ImmutableDictionary<string, LogLevel> String_LogLevel = String_LogLevel_Create(
        (LogLevel.Information, "info"),
        (LogLevel.Warning, "warn")
    );


    private static ImmutableDictionary<string, LogLevel> String_LogLevel_Create(params (LogLevel level, string alias)[] aliases)
    {
        var d = new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().Select(item => (Name: Enum.GetName(item)!, Level: item)))
        {
            d.Add(item.Name, item.Level);
            d.Add(item.Name[0].ToString(), item.Level);
        }

        foreach (var item in aliases)
        {
            d.Add(item.alias, item.level);
        }

        var b = ImmutableDictionary.CreateBuilder<string, LogLevel>(StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in d) b.Add(kvp.Key, kvp.Value);
        return b.ToImmutable();
    }


    public static readonly ILogger LoggerNull = NullLogger.Instance;
    public static readonly ILoggerProvider LoggerProviderNull = NullLoggerProvider.Instance;
}
