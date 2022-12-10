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

namespace MaxRunSoftware.Utilities.Database;

public class SqlDialectSettings
{
    public ISet<string> DatabaseUserExcluded { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public string DefaultDataTypeString { get; set; } = "TEXT";
    public string DefaultDataTypeInteger { get; set; } = "INT";
    public string DefaultDataTypeDateTime { get; set; } = "DATETIME";

    public char EscapeLeft { get; set; } = '\'';
    public char EscapeRight { get; set; } = '\'';

    public uint CommandInsertBatchSizeMax { get; set; } = 2000;

    public ISet<char> IdentifierCharactersValid { get; } = new HashSet<char>(Constant.CharComparer_OrdinalIgnoreCase);

    public ISet<string> ReservedWords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static HashSet<string> ReservedWordsParse(string words) => words.SplitOnWhiteSpace().TrimOrNull().WhereNotNull().ToHashSet(StringComparer.OrdinalIgnoreCase);


}
