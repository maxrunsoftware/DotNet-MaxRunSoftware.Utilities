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
/// https://docs.oracle.com/database/121/REFRN/GUID-6A0C9B51-1714-4223-B166-9D54C4E65D67.htm#REFRN30105
/// https://docs.oracle.com/en/database/oracle/oracle-database/21/refrn/V-INSTANCE.html
/// </summary>
public class OracleSqlServerProperties : DatabaseServerProperties
{
    protected override string LoadConvertName(string name) => base.LoadConvertName(name)
        .Replace("_", "")
        .Replace("#", "Number");

    public override void Load(Sql sql)
    {
        var sqlStatement = "SELECT * FROM v$instance";
        var result = sql.Query(sqlStatement).CheckNotNull("v$instance");
        Load(result);
    }

    /// <summary>
    /// INSTANCE_NUMBER
    /// <br />NUMBER
    /// <br />Instance number used for instance registration (corresponds to the INSTANCE_NUMBER initialization parameter)
    /// <br />See Also:
    /// https://docs.oracle.com/en/database/oracle/oracle-database/21/refrn/INSTANCE_NUMBER.html#GUID-6A58795A-E9A8-4249-B734-B864FADCD070
    /// </summary>
    public int? InstanceNumber { get; set; }

    /// <summary>
    /// INSTANCE_NAME
    /// <br />VARCHAR2(16)
    /// <br />Name of the instance
    /// </summary>
    public string? InstanceName { get; set; }

    /// <summary>
    /// HOST_NAME
    /// <br />VARCHAR2(64)
    /// <br />Name of the host machine
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// VERSION
    /// <br />VARCHAR2(17)
    /// <br />Database version
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// VERSION_LEGACY
    /// <br />VARCHAR2(17)
    /// <br />The legacy database version used before Oracle Database 18c. This column displays the same value as the VERSION
    /// column
    /// </summary>
    public string? VersionLegacy { get; set; }

    /// <summary>
    /// VERSION_FULL
    /// <br />VARCHAR2(17)
    /// <br />The version string with the new Oracle Database version scheme introduced in Oracle Database 18c.
    /// </summary>
    public string? VersionFull { get; set; }

    /// <summary>
    /// STARTUP_TIME
    /// <br />DATE
    /// <br />Time when the instance was started
    /// </summary>
    public DateTime? StartupTime { get; set; }

    /// <summary>
    /// STATUS
    /// <br />VARCHAR2(12)
    /// <br />Status of the instance:
    /// <br />STARTED - After STARTUP NOMOUNT
    /// <br />MOUNTED - After STARTUP MOUNT or ALTER DATABASE CLOSE
    /// <br />OPEN - After STARTUP or ALTER DATABASE OPEN
    /// <br />OPEN MIGRATE - After ALTER DATABASE OPEN { UPGRADE | DOWNGRADE }
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// PARALLEL
    /// <br />VARCHAR2(3)
    /// <br />Indicates whether the instance is mounted in cluster database mode (YES) or not (NO)
    /// </summary>
    public bool? Parallel { get; set; }

    /// <summary>
    /// THREAD#
    /// <br />NUMBER
    /// <br />Redo thread opened by the instance
    /// </summary>
    public int? ThreadNumber { get; set; }

    /// <summary>
    /// ARCHIVER
    /// <br />VARCHAR2(7)
    /// <br />Automatic archiving status:
    /// <br />STOPPED
    /// <br />STARTED
    /// <br />FAILED - Archiver failed to archive a log last time but will try again within 5 minutes
    /// </summary>
    public string? Archiver { get; set; }

    /// <summary>
    /// LOG_SWITCH_WAIT
    /// <br />VARCHAR2(15)
    /// <br />Event that log switching is waiting for:
    /// <br />ARCHIVE LOG
    /// <br />CLEAR LOG
    /// <br />CHECKPOINT
    /// <br />NULL - ALTER SYSTEM SWITCH LOGFILE is hung but there is room in the current online redo log
    /// </summary>
    public string? LogSwitchWait { get; set; }

    /// <summary>
    /// LOGINS
    /// <br />VARCHAR2(10)
    /// <br />Indicates whether the instance is in unrestricted mode, allowing logins by all users (ALLOWED,
    /// or in restricted mode, allowing logins by database administrators only (RESTRICTED)
    /// </summary>
    public string? Logins { get; set; }

    /// <summary>
    /// SHUTDOWN_PENDING
    /// <br />VARCHAR2(3)
    /// <br />Indicates whether a shutdown is pending (YES) or not (NO)
    /// </summary>
    public bool? ShutdownPending { get; set; }

    /// <summary>
    /// DATABASE_STATUS
    /// <br />VARCHAR2(17)
    /// <br />Status of the database:
    /// <br />ACTIVE
    /// <br />SUSPENDED
    /// <br />INSTANCE RECOVERY
    /// </summary>
    public string? DatabaseStatus { get; set; }

    /// <summary>
    /// INSTANCE_ROLE
    /// <br />VARCHAR2(18)
    /// <br />Indicates whether the instance is an active instance (PRIMARY_INSTANCE) or an inactive secondary
    /// instance (SECONDARY_INSTANCE), or UNKNOWN if the instance has been started but not mounted
    /// </summary>
    public string? InstanceRole { get; set; }

    /// <summary>
    /// ACTIVE_STATE
    /// <br />VARCHAR2(9)
    /// <br />Quiesce state of the instance:
    /// <br />NORMAL - Database is in a normal state.
    /// <br />QUIESCING - ALTER SYSTEM QUIESCE RESTRICTED has been issued: no new user transactions, queries,
    /// or PL/SQL statements are processed in this instance. User transactions, queries, or PL/SQL statements
    /// issued before the ALTER SYSTEM QUIESCE RESTRICTED statement are unaffected. DBA transactions, queries,
    /// or PL/SQL statements are also unaffected.
    /// <br />QUIESCED - ALTER SYSTEM QUIESCE RESTRICTED has been issued: no user transactions, queries, or PL/SQL
    /// statements are processed. DBA transactions, queries, or PL/SQL statements are unaffected. User transactions,
    /// queries, or PL/ SQL statements issued after the ALTER SYSTEM QUIESCE RESTRICTED statement are not processed.
    /// <br />A single ALTER SYSTEM QUIESCE RESTRICTED statement quiesces all instances in an Oracle RAC environment.
    /// After this statement has been issued, some instances may enter into a quiesced state before other instances;
    /// the system is quiesced when all instances enter the quiesced state.
    /// </summary>
    public string? ActiveState { get; set; }

    /// <summary>
    /// BLOCKED
    /// <br />VARCHAR2(3)
    /// <br />Indicates whether all services are blocked (YES) or not (NO)
    /// </summary>
    public bool? Blocked { get; set; }

    /// <summary>
    /// CON_ID
    /// <br />NUMBER
    /// <br />The ID of the container to which the data pertains. Possible values include:
    /// <br />0: This value is used for rows containing data that pertain to the entire CDB. This value is also
    /// used for rows in non-CDBs.
    /// <br />1: This value is used for rows containing data that pertain to only the root
    /// <br />n: Where n is the applicable container ID for the rows containing data
    /// </summary>
    public int? ConId { get; set; }

    /// <summary>
    /// INSTANCE_MODE
    /// <br />VARCHAR2(11)
    /// <br />Shows the instance mode of the current instance.
    /// <br />Possible values:
    /// <br />REGULAR: A regular Oracle RAC instance. This value is also always used for any non-Oracle RAC instance.
    /// <br />READ MOSTLY: An Oracle RAC instance that performs very few database writes
    /// <br />READ ONLY: A read-only Oracle RAC instance
    /// </summary>
    public string? InstanceMode { get; set; }

    /// <summary>
    /// EDITION
    /// <br />VARCHAR2(7)
    /// <br />The edition of the database.
    /// <br />Possible values include:
    /// <br />CORE EE: CORE Enterprise Edition
    /// <br />EE: Enterprise Edition
    /// <br />PO: Personal Edition
    /// <br />XE: Express Edition
    /// </summary>
    public string? Edition { get; set; }

    /// <summary>
    /// FAMILY
    /// <br />VARCHAR2(80)
    /// <br />For internal use only.
    /// </summary>
    public string? Family { get; set; }

    /// <summary>
    /// DATABASE_TYPE
    /// <br />VARCHAR2(15)
    /// <br />Database type:
    /// <br />RAC: If the database is a regular Oracle RAC database which may have multiple instances.
    /// <br />RACONENODE: If the database is Oracle RAC, but allows only one instance to run at any time - the RAC One Node
    /// mode.
    /// <br />SINGLE: If the database is running as a single instance.
    /// <br />UNKNOWN: If the database's type can't be determined. This might happen when the database is
    /// registered as a DB resource with CRS but the CRS service has failed to return valid database type
    /// information. Typically, this indicates that either the CRS service is down or it is in a faulty state.
    /// </summary>
    public string? DatabaseType { get; set; }
}
