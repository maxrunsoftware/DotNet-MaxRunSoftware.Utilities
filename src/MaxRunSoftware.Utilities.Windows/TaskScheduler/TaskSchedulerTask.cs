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

using Microsoft.Win32.TaskScheduler;

namespace MaxRunSoftware.Utilities.Windows;

[PublicAPI]
public class TaskSchedulerTask
{
    public TaskSchedulerPath? Path { get; set; }
    public IList<string> FilePaths { get; } = new List<string>();
    public IList<Trigger> Triggers { get; } = new List<Trigger>();
    public string? Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
    public string? Description { get; set; }
    public string? Documentation { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public override string ToString()
    {
        var t = GetType();
        var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(prop => !prop.Name.In(StringComparer.OrdinalIgnoreCase, "Password"))
            .Select(prop => (PropertyName: prop.Name, PropertyValue: prop.GetValue(this).ToStringGuessFormat()))
            .ToArray();

        var sb = new StringBuilder();
        sb.Append(t.Name);
        sb.Append('(');
        sb.Append(properties.Select(o => $"{o.PropertyName}: {o.PropertyValue}").ToStringDelimited(", "));
        sb.Append(')');
        return sb.ToString();
    }
    
    
}
