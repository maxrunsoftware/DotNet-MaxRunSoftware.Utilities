// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

[PublicAPI]
public record LogEvent(
    string CategoryName,
    LogLevel LogLevel,
    EventId EventId,
    Type StateType,
    object? StateObject,
    LogState? State,
    Exception? Exception,
    LoggerDelegateFormatter Formatter,
    string Text,
    IImmutableStack<LogScope> Scopes
);
