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

// ReSharper disable CommentTypo


// ReSharper disable IdentifierTypo

// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Data;

/// <summary>
/// https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html
/// https://dev.mysql.com/doc/refman/8.0/en/using-system-variables.html
/// https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html
/// </summary>
public class MySqlServerProperties
{
    public MySqlServerPropertiesVariable VariablesGlobal { get; }
    public MySqlServerPropertiesVariable VariablesSession { get; }
    public MySqlServerProperties(Sql sql)
    {
        VariablesGlobal = new(DatabaseServerProperties.ToDictionaryVertical(sql.Query(
            "SELECT * FROM performance_schema.global_variables"
        ).CheckNotNull("performance_schema.global_variables")));

        VariablesSession = new(DatabaseServerProperties.ToDictionaryVertical(sql.Query(
            "SELECT * FROM performance_schema.session_variables"
        ).CheckNotNull("performance_schema.session_variables")));
    }

    public virtual string ToStringDetailed()
    {
        var propsSession = VariablesSession.ToStringPropertiesDictionary();
        var propsGlobal = VariablesSession.ToStringPropertiesDictionary();

        var names = propsSession.Keys.Union(propsGlobal.Keys).Distinct().OrderBy(o => o, Constant.StringComparer_OrdinalIgnoreCase_Ordinal).ToArray();

        var sb = new StringBuilder();
        sb.AppendLine(GetType().NameFormatted());
        sb.AppendLine("  Variables SESSION  (GLOBAL):");
        foreach (var name in names)
        {
            var vSession = propsSession.TryGetValue(name, out var valueSession) ? valueSession.Value : "";
            var vGlobal = propsGlobal.TryGetValue(name, out var valueGlobal) ? valueGlobal.Value : "";

            sb.AppendLine($"    {name}: " + (vSession == vGlobal ? $"{vSession}" : $"{vSession}  ({vGlobal})"));
        }

        return sb.ToString().Trim();
    }
}

public class MySqlServerPropertiesVariable : DatabaseServerProperties
{
    private readonly Dictionary<string, string?> dictionaryStripped;
    private string StripName(string name) => name.Replace("_", "").Trim();
    public MySqlServerPropertiesVariable(Dictionary<string, string?> dictionary) : base(dictionary) => dictionaryStripped = dictionary.ToDictionary(
        kvp => StripName(kvp.Key),
        kvp => kvp.Value,
        StringComparer.OrdinalIgnoreCase
    );

    protected override string? GetStringNullable(string name) =>
        base.GetStringNullable(name)
        ?? base.GetStringNullable(name.SplitOnCamelCase().ToStringDelimited("_"))
        ?? (dictionaryStripped.TryGetValue(StripName(name), out var v) ? v : null);

    /// <summary>
    /// <para>
    /// The autocommit mode. If set to 1, all changes to a table take effect immediately. If set to 0, you must use COMMIT to accept a transaction or ROLLBACK to cancel it. If autocommit is 0 and you change it to 1, MySQL performs an automatic COMMIT of any open transaction. Another way to begin a transaction is to use a START TRANSACTION or BEGIN statement. See Section 13.3.1, “START TRANSACTION, COMMIT, and ROLLBACK Statements”.
    /// </para>
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
    public bool? Autocommit => GetBoolNullable(nameof(Autocommit));

    /// <summary>
    /// <para>
    /// The path to the MySQL installation base directory.
    /// </para>
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
    public string? Basedir => GetStringNullable(nameof(Basedir));


    /// <summary>
    /// <para>
    /// The MySQL server listens on one or more network sockets for TCP/IP connections. Each socket is bound to one address, but it is possible for an address to map onto multiple network interfaces. To specify how the server should listen for TCP/IP connections, set the bind_address system variable at server startup. The server also has an admin_address system variable that enables administrative connections on a dedicated interface. See Section 5.1.12.1, “Connection Interfaces”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--bind-address=addr</description></item>
    /// <item><term>System Variable</term><description>bind_address</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// <item><term>Default Value</term><description>*</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_bind_address">bind_address</see>
    /// </summary>
    public string? BindAddress => GetStringNullable(nameof(BindAddress));


    /// <summary>
    /// <para>
    /// The path to the MySQL server data directory. Relative paths are resolved with respect to the current directory. If you expect the server to be started automatically (that is, in contexts for which you cannot know the current directory in advance), it is best to specify the datadir value as an absolute path.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--datadir=dir_name</description></item>
    /// <item><term>System Variable</term><description>datadir</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Directory name</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_datadir">datadir</see>
    /// </summary>
    public string? Datadir => GetStringNullable(nameof(Datadir));

    /// <summary>
    /// <para>
    /// The default storage engine for tables. See Chapter 16, Alternative Storage Engines. This variable sets the storage engine for permanent tables only. To set the storage engine for TEMPORARY tables, set the default_tmp_storage_engine system variable.
    /// </para>
    /// <para>
    /// To see which storage engines are available and enabled, use the SHOW ENGINES statement or query the INFORMATION_SCHEMA ENGINES table.
    /// </para>
    /// <para>
    /// If you disable the default storage engine at server startup, you must set the default engine for both permanent and TEMPORARY tables to a different engine, or else the server does not start.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--default-storage-engine=name</description></item>
    /// <item><term>System Variable</term><description>default_storage_engine</description></item>
    /// <item><term>Scope</term><description>Global, Session</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Enumeration</description></item>
    /// <item><term>Default Value</term><description>InnoDB</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_default_storage_engine">default_storage_engine</see>
    /// </summary>
    public string? DefaultStorageEngine => GetStringNullable(nameof(DefaultStorageEngine));


    /// <summary>
    /// <para>
    /// The default storage engine for TEMPORARY tables (created with CREATE TEMPORARY TABLE). To set the storage engine for permanent tables, set the default_storage_engine system variable. Also see the discussion of that variable regarding possible values.
    /// </para>
    /// <para>
    /// If you disable the default storage engine at server startup, you must set the default engine for both permanent and TEMPORARY tables to a different engine, or else the server does not start.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--default-tmp-storage-engine=name</description></item>
    /// <item><term>System Variable</term><description>default_tmp_storage_engine</description></item>
    /// <item><term>Scope</term><description>Global, Session</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>Yes</description></item>
    /// <item><term>Type</term><description>Enumeration</description></item>
    /// <item><term>Default Value</term><description>InnoDB</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_default_tmp_storage_engine">default_tmp_storage_engine</see>
    /// </summary>
    public string? DefaultTmpStorageEngine => GetStringNullable(nameof(DefaultTmpStorageEngine));


    /// <summary>
    /// <para>
    /// This variable enables or disables, and starts or stops, the Event Scheduler. The possible status values are ON, OFF, and DISABLED. Turning the Event Scheduler OFF is not the same as disabling the Event Scheduler, which requires setting the status to DISABLED. This variable and its effects on the Event Scheduler's operation are discussed in greater detail in Section 25.4.2, “Event Scheduler Configuration”
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--event-scheduler[=value]</description></item>
    /// <item><term>System Variable</term><description>event_scheduler</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Enumeration</description></item>
    /// <item><term>Default Value</term><description>ON</description></item>
    /// <item><term>Valid Values</term><description>ON, OFF, DISABLED</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_event_scheduler">event_scheduler</see>
    /// </summary>
    public string? EventScheduler => GetStringNullable(nameof(EventScheduler));


    /// <summary>
    /// <para>
    /// If set to 1 (the default), foreign key constraints are checked. If set to 0, foreign key constraints are ignored, with a couple of exceptions. When re-creating a table that was dropped, an error is returned if the table definition does not conform to the foreign key constraints referencing the table. Likewise, an ALTER TABLE operation returns an error if a foreign key definition is incorrectly formed. For more information, see Section 13.1.20.5, “FOREIGN KEY Constraints”.
    /// </para>
    /// <para>
    /// Setting this variable has the same effect on NDB tables as it does for InnoDB tables. Typically you leave this setting enabled during normal operation, to enforce referential integrity. Disabling foreign key checking can be useful for reloading InnoDB tables in an order different from that required by their parent/child relationships. See Section 13.1.20.5, “FOREIGN KEY Constraints”.
    /// </para>
    /// <para>
    /// Setting foreign_key_checks to 0 also affects data definition statements: DROP SCHEMA drops a schema even if it contains tables that have foreign keys that are referred to by tables outside the schema, and DROP TABLE drops tables that have foreign keys that are referred to by other tables.
    /// </para>
    /// <para>
    /// Note: Setting foreign_key_checks to 1 does not trigger a scan of the existing table data. Therefore, rows added to the table while foreign_key_checks = 0 are not verified for consistency.
    /// </para>
    /// <para>
    /// Note: Dropping an index required by a foreign key constraint is not permitted, even with foreign_key_checks=0. The foreign key constraint must be removed before dropping the index.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>foreign_key_checks</description></item>
    /// <item><term>Scope</term><description>Global, Session</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>Yes</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// <item><term>Default Value</term><description>ON</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_foreign_key_checks">foreign_key_checks</see>
    /// </summary>
    public bool ForeignKeyChecks => GetBool(nameof(ForeignKeyChecks));

    /// <summary>
    /// <para>
    /// The server sets this variable to the server host name at startup. The maximum length is 255 characters as of MySQL 8.0.17, per RFC 1034, and 60 characters before that.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>hostname</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term>No<description></description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_hostname">hostname</see>
    /// </summary>
    public string? Hostname => GetStringNullable(nameof(Hostname));

    /// <summary>
    /// <para>
    /// A string to be executed by the server for each client that connects. The string consists of one or more SQL statements, separated by semicolon characters.
    /// </para>
    /// <para>
    /// For users that have the CONNECTION_ADMIN privilege (or the deprecated SUPER privilege), the content of init_connect is not executed. This is done so that an erroneous value for init_connect does not prevent all clients from connecting. For example, the value might contain a statement that has a syntax error, thus causing client connections to fail. Not executing init_connect for users that have the CONNECTION_ADMIN or SUPER privilege enables them to open a connection and fix the init_connect value.
    /// </para>
    /// <para>
    /// init_connect execution is skipped for any client user with an expired password. This is done because such a user cannot execute arbitrary statements, and thus init_connect execution fails, leaving the client unable to connect. Skipping init_connect execution enables the user to connect and change password.
    /// </para>
    /// <para>
    /// The server discards any result sets produced by statements in the value of init_connect.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--init-connect=name</description></item>
    /// <item><term>System Variable</term><description>init_connect</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_init_connect">init_connect</see>
    /// </summary>
    public string? InitConnect => GetStringNullable(nameof(InitConnect));

    /// <summary>
    /// <para>
    /// Defines the name, size, and attributes of InnoDB system tablespace data files. If you do not specify a value for innodb_data_file_path, the default behavior is to create a single auto-extending data file, slightly larger than 12MB, named ibdata1.
    /// </para>
    /// <para>
    /// The full syntax for a data file specification includes the file name, file size, autoextend attribute, and max attribute:
    /// </para>
    /// <para>
    /// <c>file_name:file_size[:autoextend[:max:max_file_size]]</c>
    /// </para>
    /// <para>
    /// File sizes are specified in kilobytes, megabytes, or gigabytes by appending K, M or G to the size value. If specifying the data file size in kilobytes, do so in multiples of 1024. Otherwise, KB values are rounded to nearest megabyte (MB) boundary. The sum of file sizes must be, at a minimum, slightly larger than 12MB.
    /// </para>
    /// <para>
    /// For additional configuration information, see System Tablespace Data File Configuration. For resizing instructions, see Resizing the System Tablespace.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--innodb-data-file-path=file_name</description></item>
    /// <item><term>System Variable</term><description>innodb_data_file_path</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// <item><term>Default Value</term><description>ibdata1:12M:autoextend</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_data_file_path">innodb_data_file_path</see>
    /// </summary>
    public string? InnoDB_DataFilePath => GetStringNullable(nameof(InnoDB_DataFilePath));

    /// <summary>
    /// <para>
    /// The common part of the directory path for InnoDB system tablespace data files. The default value is the MySQL data directory. The setting is concatenated with the innodb_data_file_path setting, unless that setting is defined with an absolute path.
    /// </para>
    /// <para>
    /// A trailing slash is required when specifying a value for innodb_data_home_dir. For example:
    /// </para>
    /// <para>
    /// <c>[mysqld]</c>
    /// <br /><c>innodb_data_home_dir = /path/to/myibdata/</c>
    /// </para>
    /// <para>
    /// This setting does not affect the location of file-per-table tablespaces.
    /// </para>
    /// <para>
    /// For related information, see Section 15.8.1, “InnoDB Startup Configuration”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--innodb-data-home-dir=dir_name</description></item>
    /// <item><term>System Variable</term><description>innodb_data_home_dir</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Directory name</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_data_home_dir">innodb_data_home_dir</see>
    /// </summary>
    public string? InnoDB_DataHomeDir => GetStringNullable(nameof(InnoDB_DataHomeDir));

    /// <summary>
    /// <para>
    /// The InnoDB shutdown mode. If the value is 0, InnoDB does a slow shutdown, a full purge and a change buffer merge before shutting down. If the value is 1 (the default), InnoDB skips these operations at shutdown, a process known as a fast shutdown. If the value is 2, InnoDB flushes its logs and shuts down cold, as if MySQL had crashed; no committed transactions are lost, but the crash recovery operation makes the next startup take longer.
    /// </para>
    /// <para>
    /// The slow shutdown can take minutes, or even hours in extreme cases where substantial amounts of data are still buffered. Use the slow shutdown technique before upgrading or downgrading between MySQL major releases, so that all data files are fully prepared in case the upgrade process updates the file format.
    /// </para>
    /// <para>
    /// Use innodb_fast_shutdown=2 in emergency or troubleshooting situations, to get the absolute fastest shutdown if data is at risk of corruption.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--innodb-fast-shutdown=#</description></item>
    /// <item><term>System Variable</term><description>innodb_fast_shutdown</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Integer</description></item>
    /// <item><term>Default Value</term><description>1</description></item>
    /// <item><term>Valid Values</term><description>0, 1, 2</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_fast_shutdown">innodb_fast_shutdown</see>
    /// </summary>
    public int? InnoDB_FastShutdown => GetIntNullable(nameof(InnoDB_FastShutdown));

    /// <summary>
    /// <para>
    /// When innodb_file_per_table is enabled, tables are created in file-per-table tablespaces by default. When disabled, tables are created in the system tablespace by default. For information about file-per-table tablespaces, see Section 15.6.3.2, “File-Per-Table Tablespaces”. For information about the InnoDB system tablespace, see Section 15.6.3.1, “The System Tablespace”.
    /// </para>
    /// <para>
    /// The innodb_file_per_table variable can be configured at runtime using a SET GLOBAL statement, specified on the command line at startup, or specified in an option file. Configuration at runtime requires privileges sufficient to set global system variables (see Section 5.1.9.1, “System Variable Privileges”) and immediately affects the operation of all connections.
    /// </para>
    /// <para>
    /// When a table that resides in a file-per-table tablespace is truncated or dropped, the freed space is returned to the operating system. Truncating or dropping a table that resides in the system tablespace only frees space in the system tablespace. Freed space in the system tablespace can be used again for InnoDB data but is not returned to the operating system, as system tablespace data files never shrink.
    /// </para>
    /// <para>
    /// The innodb_file_per-table setting does not affect the creation of temporary tables. As of MySQL 8.0.14, temporary tables are created in session temporary tablespaces, and in the global temporary tablespace before that. See Section 15.6.3.5, “Temporary Tablespaces”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description></description></item>
    /// <item><term>System Variable</term><description>innodb_file_per_table</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// <item><term>Default Value</term><description>ON</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_file_per_table">innodb_file_per_table</see>
    /// </summary>
    public bool? InnoDB_FilePerTable => GetBoolNullable(nameof(InnoDB_FilePerTable));

    /// <summary>
    /// <para>
    /// InnoDB performs a bulk load when creating or rebuilding indexes. This method of index creation is known as a “sorted index build”.
    /// </para>
    /// <para>
    /// innodb_fill_factor defines the percentage of space on each B-tree page that is filled during a sorted index build, with the remaining space reserved for future index growth. For example, setting innodb_fill_factor to 80 reserves 20 percent of the space on each B-tree page for future index growth. Actual percentages may vary. The innodb_fill_factor setting is interpreted as a hint rather than a hard limit.
    /// </para>
    /// <para>
    /// An innodb_fill_factor setting of 100 leaves 1/16 of the space in clustered index pages free for future index growth.
    /// </para>
    /// <para>
    /// innodb_fill_factor applies to both B-tree leaf and non-leaf pages. It does not apply to external pages used for TEXT or BLOB entries.
    /// </para>
    /// <para>
    /// For more information, see Section 15.6.2.3, “Sorted Index Builds”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description></description></item>
    /// <item><term>System Variable</term><description>innodb_fill_factor</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Integer</description></item>
    /// <item><term>Default Value</term><description>100</description></item>
    /// <item><term>Minimum Value</term><description>10</description></item>
    /// <item><term>Maximum Value</term><description>100</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_fill_factor">innodb_fill_factor</see>
    /// </summary>
    public int? InnoDB_FillFactor => GetIntNullable(nameof(InnoDB_FillFactor));

    /// <summary>
    /// <para>
    /// Controls the balance between strict ACID compliance for commit operations and higher performance that is possible when commit-related I/O operations are rearranged and done in batches. You can achieve better performance by changing the default value but then you can lose transactions in a crash.
    /// </para>
    /// <list type="bullet">
    /// <item><description>The default setting of 1 is required for full ACID compliance. Logs are written and flushed to disk at each transaction commit.</description></item>
    /// <item><description>With a setting of 0, logs are written and flushed to disk once per second. Transactions for which logs have not been flushed can be lost in a crash.</description></item>
    /// <item><description>With a setting of 2, logs are written after each transaction commit and flushed to disk once per second. Transactions for which logs have not been flushed can be lost in a crash.</description></item>
    /// <item><description>For settings 0 and 2, once-per-second flushing is not 100% guaranteed. Flushing may occur more frequently due to DDL changes and other internal InnoDB activities that cause logs to be flushed independently of the innodb_flush_log_at_trx_commit setting, and sometimes less frequently due to scheduling issues. If logs are flushed once per second, up to one second of transactions can be lost in a crash. If logs are flushed more or less frequently than once per second, the amount of transactions that can be lost varies accordingly.</description></item>
    /// <item><description>Log flushing frequency is controlled by innodb_flush_log_at_timeout, which allows you to set log flushing frequency to N seconds (where N is 1 ... 2700, with a default value of 1). However, any unexpected mysqld process exit can erase up to N seconds of transactions.</description></item>
    /// <item><description>DDL changes and other internal InnoDB activities flush the log independently of the innodb_flush_log_at_trx_commit setting.</description></item>
    /// <item><description>InnoDB crash recovery works regardless of the innodb_flush_log_at_trx_commit setting. Transactions are either applied entirely or erased entirely.</description></item>
    /// </list>
    /// <para>
    /// For durability and consistency in a replication setup that uses InnoDB with transactions:
    /// </para>
    /// <list type="bullet">
    /// <item><description>If binary logging is enabled, set sync_binlog=1.</description></item>
    /// <item><description>Always set innodb_flush_log_at_trx_commit=1.</description></item>
    /// </list>
    /// <para>
    /// For information on the combination of settings on a replica that is most resilient to unexpected halts, see Section 17.4.2, “Handling an Unexpected Halt of a Replica”.
    /// </para>
    /// <para>
    /// Caution: Many operating systems and some disk hardware fool the flush-to-disk operation. They may tell mysqld that the flush has taken place, even though it has not. In this case, the durability of transactions is not guaranteed even with the recommended settings, and in the worst case, a power outage can corrupt InnoDB data. Using a battery-backed disk cache in the SCSI disk controller or in the disk itself speeds up file flushes, and makes the operation safer. You can also try to disable the caching of disk writes in hardware caches.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--innodb-flush-log-at-trx-commit=#</description></item>
    /// <item><term>System Variable</term><description>innodb_flush_log_at_trx_commit</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Enumeration</description></item>
    /// <item><term>Default Value</term><description>1</description></item>
    /// <item><term>Valid Values</term><description>0, 1, 2</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_flush_log_at_trx_commit">innodb_flush_log_at_trx_commit</see>
    /// </summary>
    public int InnoDB_FlushLogAtTrxCommit => GetIntNullable(nameof(InnoDB_FlushLogAtTrxCommit)) ?? 1;

    /// <summary>
    /// <para>
    /// The directory path to the InnoDB redo log files.
    /// </para>
    /// <para>
    /// For related information, see Redo Log Configuration.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--innodb-log-group-home-dir=dir_name</description></item>
    /// <item><term>System Variable</term><description>innodb_log_group_home_dir</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Directory name</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_log_group_home_dir">innodb_log_group_home_dir</see>
    /// </summary>
    public string? InnoDB_LogGroupHomeDir => GetStringNullable(nameof(InnoDB_LogGroupHomeDir));

    /// <summary>
    /// <para>
    /// Starts InnoDB in read-only mode. For distributing database applications or data sets on read-only media. Can also be used in data warehouses to share the same data directory between multiple instances. For more information, see Section 15.8.2, “Configuring InnoDB for Read-Only Operation”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--innodb-read-only[={OFF|ON}]</description></item>
    /// <item><term>System Variable</term><description>innodb_read_only</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// <item><term>Default Value</term><description>OFF</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/innodb-parameters.html#sysvar_innodb_read_only">innodb_read_only</see>
    /// </summary>
    public bool InnoDB_ReadOnly => GetBoolNullable(nameof(InnoDB_ReadOnly)) ?? false;


    /// <summary>
    /// <para>
    /// The type of license the server has.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>license</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// <item><term>Default Value</term><description>GPL</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_license">license</see>
    /// </summary>
    public string? License => GetStringNullable(nameof(License));

    /// <summary>
    /// <para>
    /// This variable describes the case sensitivity of file names on the file system where the data directory is located. OFF means file names are case-sensitive, ON means they are not case-sensitive. This variable is read only because it reflects a file system attribute and setting it would have no effect on the file system.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>lower_case_file_system</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_lower_case_file_system">lower_case_file_system</see>
    /// </summary>
    public bool? LowerCaseFileSystem => GetBoolNullable(nameof(LowerCaseFileSystem));

    /// <summary>
    /// <para>
    /// If set to 0, table names are stored as specified and comparisons are case-sensitive. If set to 1, table names are stored in lowercase on disk and comparisons are not case-sensitive. If set to 2, table names are stored as given but compared in lowercase. This option also applies to database names and table aliases. For additional details, see Section 9.2.3, “Identifier Case Sensitivity”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--lower-case-table-names[=#]</description></item>
    /// <item><term>System Variable</term><description>lower_case_table_names</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Integer</description></item>
    /// Default Value (macOS)	2
    /// <item><term>Default Value (Unix)</term><description>0</description></item>
    /// <item><term>Default Value (Windows)</term><description>1</description></item>
    /// <item><term>Minimum Value</term><description>0</description></item>
    /// <item><term>Maximum Value</term><description>2</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_lower_case_table_names">lower_case_table_names</see>
    /// </summary>
    public int? LowerCaseTableNames => GetIntNullable(nameof(LowerCaseTableNames));

    /// <summary>
    /// <para>
    /// The execution timeout for SELECT statements, in milliseconds. If the value is 0, timeouts are not enabled.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--max-execution-time=#</description></item>
    /// <item><term>System Variable</term><description>max_execution_time</description></item>
    /// <item><term>Scope</term><description>Global, Session</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>Yes</description></item>
    /// <item><term>Type</term><description>Integer</description></item>
    /// <item><term>Default Value</term><description>0</description></item>
    /// <item><term>Minimum Value</term><description>0</description></item>
    /// <item><term>Maximum Value</term><description>4294967295</description></item>
    /// <item><term>Unit</term><description>milliseconds</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_max_execution_time">max_execution_time</see>
    /// </summary>
    public long MaxExecutionTime => GetLongNullable(nameof(MaxExecutionTime)) ?? 0L;


    /// <summary>
    /// <para>
    /// In offline mode, the MySQL instance disconnects client users unless they have relevant privileges, and does not allow them to initiate new connections. Clients that are refused access receive an ER_SERVER_OFFLINE_MODE error.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description></description></item>
    /// <item><term>System Variable</term><description>offline_mode</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// <item><term>Default Value</term><description>OFF</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_offline_mode">offline_mode</see>
    /// </summary>
    public bool OfflineMode => GetBoolNullable(nameof(OfflineMode)) ?? false;

    /// <summary>
    /// <para>
    /// The path name of the file in which the server writes its process ID. The server creates the file in the data directory unless an absolute path name is given to specify a different directory. If you specify this variable, you must specify a value. If you do not specify this variable, MySQL uses a default value of host_name.pid, where host_name is the name of the host machine.
    /// </para>
    /// <para>
    /// The process ID file is used by other programs such as mysqld_safe to determine the server's process ID. On Windows, this variable also affects the default error log file name. See Section 5.4.2, “The Error Log”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--pid-file=file_name</description></item>
    /// <item><term>System Variable</term><description>pid_file</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>File name</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_pid_file">pid_file</see>
    /// </summary>
    public string? PidFile => GetStringNullable(nameof(PidFile));

    /// <summary>
    /// <para>
    /// The path name of the plugin directory.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--plugin-dir=dir_name</description></item>
    /// <item><term>System Variable</term><description>plugin_dir</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Directory name</description></item>
    /// <item><term>Default Value</term><description>BASEDIR/lib/plugin</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_plugin_dir">plugin_dir</see>
    /// </summary>
    public string? PluginDir => GetStringNullable(nameof(PluginDir));

    /// <summary>
    /// <para>
    /// The number of the port on which the server listens for TCP/IP connections. This variable can be set with the --port option.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--port=port_num</description></item>
    /// <item><term>System Variable</term><description>port</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Integer</description></item>
    /// <item><term>Default Value</term><description>3306</description></item>
    /// <item><term>Minimum Value</term><description>0</description></item>
    /// <item><term>Maximum Value</term><description>65535</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_port">port</see>
    /// </summary>
    public ushort Port => GetUShort(nameof(Port));

    /// <summary>
    /// <para>
    /// The version of the client/server protocol used by the MySQL server.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>protocol_version</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Integer</description></item>
    /// <item><term>Default Value</term><description>10</description></item>
    /// <item><term>Minimum Value</term><description>0</description></item>
    /// <item><term>Maximum Value</term><description>4294967295</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_protocol_version">protocol_version</see>
    /// </summary>
    public long? ProtocolVersion => GetLongNullable(nameof(ProtocolVersion));

    /// <summary>
    /// <para>
    /// If the read_only system variable is enabled, the server permits no client updates except from users who have the CONNECTION_ADMIN privilege (or the deprecated SUPER privilege). This variable is disabled by default.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--read-only[={OFF|ON}]</description></item>
    /// <item><term>System Variable</term><description>read_only</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// <item><term>Default Value</term><description>OFF</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_read_only">read_only</see>
    /// </summary>
    public bool ReadOnly => GetBoolNullable(nameof(ReadOnly)) ?? false;


    /// <summary>
    /// <para>
    /// If the read_only system variable is enabled, the server permits no client updates except from users who have the CONNECTION_ADMIN privilege (or the deprecated SUPER privilege). If the super_read_only system variable is also enabled, the server prohibits client updates even from users who have CONNECTION_ADMIN or SUPER. See the description of the read_only system variable for a description of read-only mode and information about how read_only and super_read_only interact.    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--super-read-only[={OFF|ON}]</description></item>
    /// <item><term>System Variable</term><description>super_read_only</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Boolean</description></item>
    /// <item><term>Default Value</term><description>OFF</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_super_read_only">super_read_only</see>
    /// </summary>
    public bool SuperReadOnly => GetBoolNullable(nameof(SuperReadOnly)) ?? false;

    /// <summary>
    /// <para>
    /// The server system time zone. When the server begins executing, it inherits a time zone setting from the machine defaults, possibly modified by the environment of the account used for running the server or the startup script. The value is used to set system_time_zone. To explicitly specify the system time zone, set the TZ environment variable or use the --timezone option of the mysqld_safe script.
    /// </para>
    /// <para>
    /// As of MySQL 8.0.26, in addition to startup time initialization, if the server host time zone changes (for example, due to daylight saving time), system_time_zone reflects that change, which has these implications for applications:
    /// </para>
    /// <para>
    /// Queries that reference system_time_zone will get one value before a daylight saving change and a different value after the change.
    /// </para>
    /// <para>
    /// For queries that begin executing before a daylight saving change and end after the change, the system_time_zone remains constant within the query because the value is usually cached at the beginning of execution.
    /// </para>
    /// <para>
    /// The system_time_zone variable differs from the time_zone variable. Although they might have the same value, the latter variable is used to initialize the time zone for each client that connects. See Section 5.1.15, “MySQL Server Time Zone Support”.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>system_time_zone</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_system_time_zone">system_time_zone</see>
    /// </summary>
    public string? SystemTimeZone => GetStringNullable(nameof(SystemTimeZone));


    /// <summary>
    /// <para>
    /// The number of thread groups in the thread pool. This is the most important parameter controlling thread pool performance. It affects how many statements can execute simultaneously. If a value outside the range of permissible values is specified, the thread pool plugin does not load and the server writes a message to the error log.
    /// </para>
    /// <para>
    /// This variable is available only if the thread pool plugin is enabled. See Section 5.6.3, “MySQL Enterprise Thread Pool”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--thread-pool-size=#</description></item>
    /// <item><term>System Variable</term><description>thread_pool_size</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Integer</description></item>
    /// <item><term>Default Value</term><description>16</description></item>
    /// <item><term>Minimum Value</term><description>1</description></item>
    /// <item><term>Maximum Value (≥ 8.0.19)</term><description>512</description></item>
    /// <item><term>Maximum Value (≤ 8.0.18)</term><description>64</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_thread_pool_size">thread_pool_size</see>
    /// </summary>
    public ushort? ThreadPoolSize => GetUShortNullable(nameof(ThreadPoolSize));


    /// <summary>
    /// <para>
    /// The current time zone. This variable is used to initialize the time zone for each client that connects. By default, the initial value of this is 'SYSTEM' (which means, “use the value of system_time_zone”). The value can be specified explicitly at server startup with the --default-time-zone option. See Section 5.1.15, “MySQL Server Time Zone Support”.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>time_zone</description></item>
    /// <item><term>Scope</term><description>Global, Session</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies (≥ 8.0.17)</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies (≤ 8.0.16)</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// <item><term>Default Value</term><description>SYSTEM</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_time_zone">time_zone</see>
    /// </summary>
    public string? TimeZone => GetStringNullable(nameof(TimeZone));

    /// <summary>
    /// <para>
    /// The path of the directory to use for creating temporary files. It might be useful if your default /tmp directory resides on a partition that is too small to hold temporary tables. This variable can be set to a list of several paths that are used in round-robin fashion. Paths should be separated by colon characters (:) on Unix and semicolon characters (;) on Windows.
    /// </para>
    /// <para>
    /// tmpdir can be a non-permanent location, such as a directory on a memory-based file system or a directory that is cleared when the server host restarts. If the MySQL server is acting as a replica, and you are using a non-permanent location for tmpdir, consider setting a different temporary directory for the replica using the replica_load_tmpdir or slave_load_tmpdir variable. For a replica, the temporary files used to replicate LOAD DATA statements are stored in this directory, so with a permanent location they can survive machine restarts, although replication can now continue after a restart if the temporary files have been removed.
    /// </para>
    /// <para>
    /// For more information about the storage location of temporary files, see Section B.3.3.5, “Where MySQL Stores Temporary Files”.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--tmpdir=dir_name</description></item>
    /// <item><term>System Variable</term><description>tmpdir</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Directory name</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_tmpdir">tmpdir</see>
    /// </summary>
    public string? TmpDir => GetStringNullable(nameof(TmpDir));

    /// <summary>
    /// <para>
    /// The transaction isolation level.
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description>--transaction-isolation=name</description></item>
    /// <item><term>System Variable</term><description>transaction_isolation</description></item>
    /// <item><term>Scope</term><description>Global, Session</description></item>
    /// <item><term>Dynamic</term><description>Yes</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>Enumeration</description></item>
    /// <item><term>Default Value</term><description>REPEATABLE-READ</description></item>
    /// <item><term>Valid Values</term><description>READ-UNCOMMITTED, READ-COMMITTED, REPEATABLE-READ, SERIALIZABLE</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_transaction_isolation">transaction_isolation</see>
    /// </summary>
    public string? TransactionIsolation => GetStringNullable(nameof(TransactionIsolation));

    /// <summary>
    /// <para>
    /// The version number for the server. The value might also include a suffix indicating server build or configuration information. -debug indicates that the server was built with debugging support enabled.
    /// </para>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_version">version</see>
    /// </summary>
    public string? Version => GetStringNullable(nameof(Version));

    /// <summary>
    /// <para>
    /// The CMake configuration program has a COMPILATION_COMMENT_SERVER option that permits a comment to be specified when building MySQL. This variable contains the value of that comment. (Prior to MySQL 8.0.14, version_comment is set by the COMPILATION_COMMENT option.) See Section 2.9.7, “MySQL Source-Configuration Options”.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>version_comment</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_version_comment">version_comment</see>
    /// </summary>
    public string? VersionComment => GetStringNullable(nameof(VersionComment));

    /// <summary>
    /// <para>
    /// The type of the server binary.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>version_compile_machine</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_version_compile_machine">version_compile_machine</see>
    /// </summary>
    public string? VersionCompileMachine => GetStringNullable(nameof(VersionCompileMachine));


    /// <summary>
    /// <para>
    /// The type of operating system on which MySQL was built.
    /// </para>
    /// <list type="table">
    /// <item><term>System Variable</term><description>version_compile_os</description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description>String</description></item>
    /// </list>
    /// <see href="https://dev.mysql.com/doc/refman/8.0/en/server-system-variables.html#sysvar_version_compile_os">version_compile_os</see>
    /// </summary>
    public string? VersionCompileOS => GetStringNullable(nameof(VersionCompileOS));


    /*
    /// <summary>
    /// <para>
    ///
    /// </para>
    /// <list type="table">
    /// <item><term>Command-Line Format</term><description></description></item>
    /// <item><term>System Variable</term><description></description></item>
    /// <item><term>Scope</term><description>Global</description></item>
    /// <item><term>Dynamic</term><description>No</description></item>
    /// <item><term>SET_VAR Hint Applies</term><description>No</description></item>
    /// <item><term>Type</term><description></description></item>
    /// <item><term>Default Value</term><description></description></item>
    /// </list>
    /// <see href=""></see>
    /// </summary>
    public string  => GetString(nameof());
    */
}
