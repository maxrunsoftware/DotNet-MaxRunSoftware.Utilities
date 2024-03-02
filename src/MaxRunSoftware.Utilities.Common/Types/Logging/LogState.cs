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

namespace MaxRunSoftware.Utilities.Common;

public class LogState
{
    public static bool IsNullMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return true;
        var messageTrimmed = message.Trim();
        if (messageTrimmed.Length == 0) return true;
        if (messageTrimmed.Length is 4 or 6 && messageTrimmed.Contains("null", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static readonly IReadOnlyList<KeyValuePair<string, object?>> EMPTY = Array.Empty<KeyValuePair<string, object?>>();

    public string Message { get; }
    public IReadOnlyList<KeyValuePair<string, object?>> Arguments { get; }

    public static LogState? Create(object? state)
    {
        if (state == null) return null;
        var message = state.ToString();
        if (IsNullMessage(message)) return null;

        var list = new List<KeyValuePair<string, object?>>();
        if (state is string stateString)
        {
            list.Add(new(stateString, null));
        }
        else if (state is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is KeyValuePair<string, object?> kvp)
                {
                    list.Add(new(kvp.Key, kvp.Value));
                }
            }
        }

        return new(message!, list.Count == 0 ? EMPTY : list);
    }
    private LogState(string message, IReadOnlyList<KeyValuePair<string, object?>> arguments)
    {
        Message = message;
        Arguments = arguments;
    }
}
