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

public interface INamed
{
    string? Name { get; }
}

public interface INamedFull
{
    string? NameFull { get; }
}

public interface ISlimValueGetter
{
    object? GetValue(object? instance);
}

public interface ISlimInvokable : ISlimValueGetter
{
    object? Invoke(object? instance, object?[]? args = null, Type[]? genericTypeArguments = null);
}

public interface ISlimValueSetter
{
    void SetValue(object? instance, object? value);
}
