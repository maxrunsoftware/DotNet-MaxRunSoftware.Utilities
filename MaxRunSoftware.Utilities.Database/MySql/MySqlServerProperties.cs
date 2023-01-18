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

// ReSharper disable CommentTypo

namespace MaxRunSoftware.Utilities.Database;

/// <summary>
/// https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html
/// https://dev.mysql.com/doc/refman/8.0/en/using-system-variables.html
/// </summary>
public class MySqlServerProperties : DatabaseServerProperties
{
    // ReSharper disable InconsistentNaming
    private const string VariablesSession = "VARIABLES SESSION";
    private const string VariablesGlobal = "VARIABLES GLOBAL";
    // ReSharper restore InconsistentNaming

    protected override Dictionary<string, string?> ToDictionary(DataReaderResult result, string? group)
    {
        var dicOriginal = base.ToDictionary(result, group);
        if (group == null) return dicOriginal;
        if (!group.Contains(" GLOBAL ", StringComparison.OrdinalIgnoreCase)) return dicOriginal;
        return dicOriginal.ToDictionary(kvp => kvp.Key + "Global", kvp => kvp.Value);
    }
    public override void Load(Sql sql)
    {
        var result = sql.Query("SHOW SESSION VARIABLES").CheckNotNull("SHOW SESSION VARIABLES");
        Load(result, VariablesSession);

        result = sql.Query("SHOW GLOBAL VARIABLES").CheckNotNull("SHOW GLOBAL VARIABLES");
        Load(result, VariablesGlobal);
    }

    #region VARIABLES

    /// <inheritdoc cref="AutocommitGlobal" />
    [DatabaseServerProperty(VariablesSession, MySqlType.Bit)]
    public bool Autocommit { get; set; }

    /// <summary>
    /// The autocommit mode. If set to 1, all changes to a table take effect immediately. If set to 0, you must
    /// use COMMIT to accept a transaction or ROLLBACK to cancel it. If autocommit is 0 and you change it to 1,
    /// MySQL performs an automatic COMMIT of any open transaction. Another way to begin a transaction is to use
    /// a START TRANSACTION or BEGIN statement.
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--autocommit[={OFF|ON}]</description></item>
    /// <item><term>System Variable</term><description>autocommit</description></item>
    /// <item><term>Scope</term><description>Global, Session</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// <item><term>Default Value</term><description>ON</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_autocommit">autocommit</see>
    /// </summary>
    [DatabaseServerProperty(VariablesGlobal, MySqlType.Bit)]
    public bool AutocommitGlobal { get; set; }

    /// <summary>
    /// The path to the MySQL installation base directory.
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--basedir=dir_name</description></item>
    /// <item><term>System Variable</term><description>basedir</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Directory name</description></item>
    /// <item><term>Default Value</term><description>parent of mysqld installation directory</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_basedir">basedir</see>
    /// </summary>
    [DatabaseServerProperty(VariablesGlobal, MySqlType.Bit)]
    public bool BaseDirGlobal { get; set; }

    #endregion VARIABLES
}
