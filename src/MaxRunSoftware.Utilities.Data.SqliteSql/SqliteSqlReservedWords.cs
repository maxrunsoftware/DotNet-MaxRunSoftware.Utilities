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

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable StringLiteralTypo
/// <summary>
///     <see href="https://sqlite.org/lang_keywords.html">
///     Reserved Words
///     </see>
/// </summary>
public static class SqliteSqlReservedWords
{
    public static readonly string WORDS = @"


ABORT
ACTION
ADD
AFTER
ALL
ALTER
ALWAYS
ANALYZE
AND
AS
ASC
ATTACH
AUTOINCREMENT
BEFORE
BEGIN
BETWEEN
BY
CASCADE
CASE
CAST
CHECK
COLLATE
COLUMN
COMMIT
CONFLICT
CONSTRAINT
CREATE
CROSS
CURRENT
CURRENT_DATE
CURRENT_TIME
CURRENT_TIMESTAMP
DATABASE
DEFAULT
DEFERRABLE
DEFERRED
DELETE
DESC
DETACH
DISTINCT
DO
DROP
EACH
ELSE
END
ESCAPE
EXCEPT
EXCLUDE
EXCLUSIVE
EXISTS
EXPLAIN
FAIL
FILTER
FIRST
FOLLOWING
FOR
FOREIGN
FROM
FULL
GENERATED
GLOB
GROUP
GROUPS
HAVING
IF
IGNORE
IMMEDIATE
IN
INDEX
INDEXED
INITIALLY
INNER
INSERT
INSTEAD
INTERSECT
INTO
IS
ISNULL
JOIN
KEY
LAST
LEFT
LIKE
LIMIT
MATCH
MATERIALIZED
NATURAL
NO
NOT
NOTHING
NOTNULL
NULL
NULLS
OF
OFFSET
ON
OR
ORDER
OTHERS
OUTER
OVER
PARTITION
PLAN
PRAGMA
PRECEDING
PRIMARY
QUERY
RAISE
RANGE
RECURSIVE
REFERENCES
REGEXP
REINDEX
RELEASE
RENAME
REPLACE
RESTRICT
RETURNING
RIGHT
ROLLBACK
ROW
ROWS
SAVEPOINT
SELECT
SET
TABLE
TEMP
TEMPORARY
THEN
TIES
TO
TRANSACTION
TRIGGER
UNBOUNDED
UNION
UNIQUE
UPDATE
USING
VACUUM
VALUES
VIEW
VIRTUAL
WHEN
WHERE
WINDOW
WITH
WITHOUT




";
}
