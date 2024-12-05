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

namespace MaxRunSoftware.Utilities.Data;

/// <summary>
/// Describes a parameter for query parameter substitution.
/// </summary>
/// <param name="name">The name of the parameter</param>
/// <param name="type">The database type for the parameter</param>
public class DatabaseParameter(string name, DbType type)
{
    public string Name { get; } = name;
    public DbType Type { get; } = type;
}

/// <summary>
/// Describes a parameter for query parameter substitution with a value.
/// </summary>
/// <param name="name">The name of the parameter</param>
/// <param name="type">The database type for the parameter</param>
/// <param name="value">The value of the parameter</param>
public class DatabaseParameterValue(string name, DbType type, object? value) : DatabaseParameter(name, type)
{
    public object? Value { get; } = value;
}
