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

namespace MaxRunSoftware.Utilities.Sql;

public static class MicrosoftSqlConstant
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable once StringLiteralTypo

    public static readonly ImmutableArray<string> Excluded_Databases = new[] {"master", "model", "msdb", "tempdb"}.ToImmutableArray();

    public static readonly string Default_DataType_String = nameof(MicrosoftSqlType.NVarChar) + "(MAX)";
    public static readonly string Default_DataType_Integer = nameof(MicrosoftSqlType.Int);
    public static readonly string Default_DataType_DateTime = nameof(MicrosoftSqlType.DateTime);

    public static readonly char Escape_Left = '[';
    public static readonly char Escape_Right = ']';

    public static readonly uint Command_Insert_BatchSize_Max = 2000;

    /// <summary>
    /// https://docs.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers?view=sql-server-ver16
    /// </summary>
    public static readonly ImmutableArray<char> Valid_Identifier_Characters = (Constant.Chars_Alphanumeric_String + "@$#_").ToImmutableArray();

    public static readonly ImmutableArray<string> Reserved_Words = MicrosoftSqlReservedWords.WORDS.SplitOnWhiteSpace().TrimOrNull().WhereNotNull().ToImmutableArray();
}
