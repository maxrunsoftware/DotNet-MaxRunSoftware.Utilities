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

using MaxRunSoftware.Utilities.Sql;

namespace MaxRunSoftware.Utilities.Database;

public class MicrosoftSqlDialectSettings : SqlDialectSettings
{
    public override ImmutableArray<string> DatabaseUserExcluded { get; } = new[] {"master", "model", "msdb", "tempdb"}.ToImmutableArray();

    public override string DefaultDataTypeString { get; } =
    public override string DefaultDataTypeInteger { get; } =
    public override string DefaultDataTypeDateTime { get; } =

    public override char EscapeLeft { get; set; } =
    public override char EscapeRight { get; } = ']';

    public override uint CommandInsertBatchSizeMax { get; } = 2000;

    /// <summary>
    ///
    /// </summary>
    public override ImmutableArray<char> IdentifierCharactersValid { get; } = (Constant.Chars_Alphanumeric_String + "@$#_").ToImmutableArray();

    public override ImmutableArray<string> ReservedWords { get; } = ReservedWordsCreate(MicrosoftSqlReservedWords.WORDS);
}
