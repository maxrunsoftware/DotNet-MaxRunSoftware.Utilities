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

using System.Runtime.InteropServices;

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
/// <summary>
/// https://learn.microsoft.com/en-us/sql/t-sql/functions/serverproperty-transact-sql
/// </summary>
public class MicrosoftSqlServerProperties : DatabaseServerProperties
{
    private static readonly ImmutableArray<PropertySlim> PROPERTIES_QUERY = typeof(MicrosoftSqlServerProperties)
        .ToTypeSlim().GetPropertySlims(BindingFlags.Public | BindingFlags.Instance)
        .Where(o => o.IsGettablePublic)
        .Where(o => o.Name.NotIn(nameof(ServerOS)))
        .ToImmutableArray();

    private static Dictionary<string, string?> CreateDictionary(Sql sql) =>
        ToDictionaryHorizontal(sql.Query(
            "SELECT "
            + PROPERTIES_QUERY.Select(o => $"SERVERPROPERTY('{sql.Unescape(o.Name)}') AS {sql.Escape(o.Name)}").ToStringDelimited(", ")
            + ";"
        ).CheckNotNull("SERVERPROPERTY"));

    public MicrosoftSqlServerProperties(Sql sql) : base(CreateDictionary(sql)) { }

    #region Custom

    public OSPlatform ServerOS => PathSeparator switch
    {
        null => OSPlatform.Windows,
        "\\" => OSPlatform.Windows,
        "/" => OSPlatform.Linux,
        _ => OSPlatform.Windows,
    };

    #endregion Custom

    /// <summary>
    /// Version of the Microsoft .NET Framework common language runtime (CLR) that was used while building the instance of SQL
    /// Server.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? BuildClrVersion => GetStringNullable(nameof(BuildClrVersion));

    /// <summary>
    /// Name of the default collation for the server.
    /// <br />NULL = Input isn't valid, or an error.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? Collation => GetStringNullable(nameof(Collation));

    /// <summary>
    /// ID of the SQL Server collation.
    /// <br />Base data type: int
    /// </summary>
    public int CollationID => GetInt(nameof(CollationID));

    /// <summary>
    /// Windows comparison style of the collation.
    /// <br />Base data type: int
    /// </summary>
    public int ComparisonStyle => GetInt(nameof(ComparisonStyle));

    /// <summary>
    /// NetBIOS name of the local computer on which the instance of SQL Server is currently running.
    /// <br />For a clustered instance of SQL Server on a failover cluster, this value changes as the instance of SQL Server
    /// fails over to other nodes in the failover cluster.
    /// <br />On a stand-alone instance of SQL Server, this value remains constant and returns the same value as the
    /// MachineName property.
    /// <br />Note: If the instance of SQL Server is in a failover cluster and you want to obtain the name of the failover
    /// clustered instance, use the MachineName property.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ComputerNamePhysicalNetBIOS => GetStringNullable(nameof(ComputerNamePhysicalNetBIOS));

    /// <summary>
    /// Installed product edition of the instance of SQL Server. Use the value of this property to determine the features and
    /// the limits, such as Compute Capacity Limits by Edition of SQL Server. 64-bit versions of the Database Engine append
    /// (64-bit) to the version.
    /// <br />Returns:
    /// <br />'Enterprise Edition'
    /// <br />'Enterprise Edition: Core-based Licensing'
    /// <br />'Enterprise Evaluation Edition'
    /// <br />'Business Intelligence Edition'
    /// <br />'Developer Edition'
    /// <br />'Express Edition'
    /// <br />'Express Edition with Advanced Services'
    /// <br />'Standard Edition'
    /// <br />'Web Edition'
    /// <br />'SQL Azure' indicates SQL Database or Azure Synapse Analytics
    /// <br />'Azure SQL Edge Developer' indicates the development only edition for Azure SQL Edge
    /// <br />'Azure SQL Edge' indicates the paid edition for Azure SQL Edge
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string Edition => GetString(nameof(Edition));

    /// <summary>
    /// EditionID represents the installed product edition of the instance of SQL Server. Use the value of this property to
    /// determine features and limits, such as Compute Capacity Limits by Edition of SQL Server.
    /// <br />1804890536 = Enterprise
    /// <br />1872460670 = Enterprise Edition: Core-based Licensing
    /// <br />610778273 = Enterprise Evaluation
    /// <br />284895786 = Business Intelligence
    /// <br />-2117995310 = Developer
    /// <br />-1592396055 = Express
    /// <br />-133711905 = Express with Advanced Services
    /// <br />-1534726760 = Standard
    /// <br />1293598313 = Web
    /// <br />1674378470 = SQL Database or [!INCLUDEssazuresynapse-md(../../includes/ssazuresynapse-md.md)]
    /// <br />-1461570097 = Azure SQL Edge Developer
    /// <br />1994083197 = Azure SQL Edge
    /// <br />Base data type: bigint
    /// </summary>
    public long EditionID => GetLong(nameof(EditionID));

    /// <summary>
    /// Database Engine edition of the instance of SQL Server installed on the server.
    /// <br />1 = Personal or Desktop Engine (Not available in SQL Server 2005 (9.x) and later versions.)
    /// <br />2 = Standard (For Standard, Web, and Business Intelligence.)
    /// <br />3 = Enterprise (For Evaluation, Developer, and Enterprise editions.)
    /// <br />4 = Express (For Express, Express with Tools, and Express with Advanced Services)
    /// <br />5 = SQL Database
    /// <br />6 = Azure Synapse Analytics
    /// <br />8 = Azure SQL Managed Instance
    /// <br />9 = Azure SQL Edge (For all editions of Azure SQL Edge)
    /// <br />11 = Azure Synapse serverless SQL pool
    /// <br />Base data type: int
    /// </summary>
    public int EngineEdition => GetInt(nameof(EngineEdition));

    /// <summary>
    /// The configured level of FILESTREAM access. For more information, see filestream access level.
    /// <br />Base data type: int
    /// </summary>
    public int FilestreamConfiguredLevel => GetInt(nameof(FilestreamConfiguredLevel));

    /// <summary>
    /// The effective level of FILESTREAM access. This value can be different than the FilestreamConfiguredLevel if the level
    /// has changed and either an instance restart or a computer restart is pending. For more information, see filestream
    /// access level.
    /// <br />Base data type: int
    /// </summary>
    public int FilestreamEffectiveLevel => GetInt(nameof(FilestreamEffectiveLevel));

    /// <summary>
    /// The name of the share used by FILESTREAM.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? FilestreamShareName => GetStringNullable(nameof(FilestreamShareName));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) and later.
    /// <br />Indicates whether the Always On availability groups manager has started.
    /// <br />0 = Not started, pending communication.
    /// <br />1 = Started and running.
    /// <br />2 = Not started and failed.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public int? HadrManagerStatus => GetInt(nameof(HadrManagerStatus));

    /// <summary>
    /// Applies to: SQL Server 2019 (15.x) and later.
    /// <br />Name of the default path to the instance backup files.
    /// </summary>
    public string? InstanceDefaultBackupPath => GetStringNullable(nameof(InstanceDefaultBackupPath));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) through current version in updates beginning in late 2015.
    /// <br />Name of the default path to the instance data files.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? InstanceDefaultDataPath => GetStringNullable(nameof(InstanceDefaultDataPath));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) through current version in updates beginning in late 2015.
    /// <br />Name of the default path to the instance log files.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? InstanceDefaultLogPath => GetStringNullable(nameof(InstanceDefaultLogPath));

    /// <summary>
    /// Name of the instance to which the user is connected.
    /// <br />Returns NULL if the instance name is the default instance, if the input isn't valid, or error.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? InstanceName => GetStringNullable(nameof(InstanceName));

    /// <summary>
    /// Returns 1 if the Advanced Analytics feature was installed during setup; 0 if Advanced Analytics wasn't installed.
    /// <br />Base data type: int
    /// </summary>
    public bool IsAdvancedAnalyticsInstalled => GetBool(nameof(IsAdvancedAnalyticsInstalled));

    /// <summary>
    /// Introduced in SQL Server 2019 (15.x) beginning with CU4.
    /// <br />Returns 1 if the instance is SQL Server Big Data Cluster; 0 if not.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsBigDataCluster => GetBoolNullable(nameof(IsBigDataCluster));

    /// <summary>
    /// Server instance is configured in a failover cluster.
    /// <br />1 = Clustered.
    /// <br />0 = Not Clustered.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsClustered => GetBoolNullable(nameof(IsClustered));

    /// <summary>
    /// Applies to: Azure SQL Database and Azure SQL Managed Instance.
    /// <br />Returns whether Azure AD-only authentication is enabled.
    /// <br />1 = Azure AD-only authentication is enabled.
    /// <br />0 = Azure AD-only authentication is disabled.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsExternalAuthenticationOnly => GetBoolNullable(nameof(IsExternalAuthenticationOnly));

    /// <summary>
    /// Applies to: SQL Server 2022 (16.x) and later.
    /// <br />Returns whether Microsoft Purview access policies are enabled.
    /// <br />1 = External governance is enabled.
    /// <br />0 = External governance is disabled.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsExternalGovernanceEnabled => GetBoolNullable(nameof(IsExternalGovernanceEnabled));

    /// <summary>
    /// The full-text and semantic indexing components are installed on the current instance of SQL Server.
    /// <br />1 = Full-text and semantic indexing components are installed.
    /// <br />0 = Full-text and semantic indexing components aren't installed.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsFullTextInstalled => GetBoolNullable(nameof(IsFullTextInstalled));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) and later.
    /// <br />Always On availability groups is enabled on this server instance.
    /// <br />0 = The Always On availability groups feature is disabled.
    /// <br />1 = The Always On availability groups feature is enabled.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// <br />For availability replicas to be created and run on an instance of SQL Server, Always On availability groups must
    /// be enabled on the server instance. For more information, see Enable and Disable Always On Availability Groups (SQL
    /// Server).
    /// <br />Note: The IsHadrEnabled property pertains only to Always On availability groups. Other high availability or
    /// disaster recovery features, such as database mirroring or log shipping, are unaffected by this server property.
    /// </summary>
    public bool? IsHadrEnabled => GetBoolNullable(nameof(IsHadrEnabled));

    /// <summary>
    /// Server is in integrated security mode.
    /// <br />1 = Integrated security (Windows Authentication)
    /// <br />0 = Not integrated security. (Both Windows Authentication and SQL Server Authentication.)
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsIntegratedSecurityOnly => GetBoolNullable(nameof(IsIntegratedSecurityOnly));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) and later.
    /// <br />Server is an instance of SQL Server Express LocalDB.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsLocalDB => GetBoolNullable(nameof(IsLocalDB));

    /// <summary>
    /// Applies to: SQL Server 2016 (13.x).
    /// <br />Returns whether the server instance has the PolyBase feature installed.
    /// <br />0 = PolyBase isn't installed.
    /// <br />1 = PolyBase is installed.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsPolyBaseInstalled => GetBoolNullable(nameof(IsPolyBaseInstalled));

    /// <summary>
    /// Server is in suspend mode and requires server level thaw.
    /// <br />1 = Suspended.
    /// <br />0 = Not suspended
    /// <br />Base data type: int
    /// </summary>
    public bool IsServerSuspendedForSnapshotBackup => GetBool(nameof(IsServerSuspendedForSnapshotBackup));

    /// <summary>
    /// Server is in single-user mode.
    /// <br />1 = Single user.
    /// <br />0 = Not single user
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsSingleUser => GetBoolNullable(nameof(IsSingleUser));

    /// <summary>
    /// Applies to: SQL Server 2019 (15.x) and later.
    /// <br />Returns 1 if tempdb has been enabled to use memory-optimized tables for metadata; 0 if tempdb is using regular,
    /// disk-based tables for metadata. For more information, see tempdb Database.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsTempDbMetadataMemoryOptimized => GetBoolNullable(nameof(IsTempDbMetadataMemoryOptimized));

    /// <summary>
    /// Applies to: SQL Server (SQL Server 2014 (12.x) and later), SQL Database.
    /// <br />Server supports In-Memory OLTP.
    /// <br />1 = Server supports In-Memory OLTP.
    /// <br />0 = Server doesn't supports In-Memory OLTP.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public bool? IsXTPSupported => GetBoolNullable(nameof(IsXTPSupported));

    /// <summary>
    /// Windows locale identifier (LCID) of the collation.
    /// <br />Base data type: int
    /// </summary>
    public int LCID => GetInt(nameof(LCID));

    /// <summary>
    /// Unused. License information isn't preserved or maintained by the SQL Server product. Always returns DISABLED.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string LicenseType => GetString(nameof(LicenseType));

    /// <summary>
    /// Windows computer name on which the server instance is running.
    /// <br />For a clustered instance, an instance of SQL Server running on a virtual server on Microsoft Cluster Service, it
    /// returns the name of the virtual server.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? MachineName => GetStringNullable(nameof(MachineName));

    /// <summary>
    /// Unused. License information isn't preserved or maintained by the SQL Server product. Always returns NULL.
    /// <br />Base data type: int
    /// </summary>
    public int? NumLicenses => GetIntNullable(nameof(NumLicenses));

    /// <summary>
    /// Applies to: SQL Server 2017 (14.x) and later.
    /// <br />Returns \ on Windows and / on Linux
    /// <br />Base data type: nvarchar
    /// </summary>
    public string? PathSeparator => GetStringNullable(nameof(PathSeparator));

    /// <summary>
    /// Process ID of the SQL Server service. ProcessID is useful in identifying which Sqlservr.exe belongs to this instance.
    /// <br />NULL = Input isn't valid, an error, or not applicable.
    /// <br />Base data type: int
    /// </summary>
    public int? ProcessID => GetInt(nameof(ProcessID));

    /// <summary>
    /// Applies to: SQL Server 2014 (12.x) beginning October 2015.
    /// <br />The build number.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ProductBuild => GetStringNullable(nameof(ProductBuild));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) through current version in updates beginning in late 2015.
    /// <br />Type of build of the current build.
    /// <br />Returns one of the following values:
    /// <br />OD = On Demand release a specific customer.
    /// <br />GDR = General Distribution Release released through Windows Update.
    /// <br />NULL = Not applicable.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ProductBuildType => GetStringNullable(nameof(ProductBuildType));

    /// <summary>
    /// Level of the version of the instance of SQL Server.
    /// <br />Returns one of the following values:
    /// <br />'RTM' = Original release version
    /// <br />'SPn' = Service pack version
    /// <br />'CTPn', = Community Technology Preview version
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string ProductLevel => GetString(nameof(ProductLevel));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) through current version in updates beginning in late 2015.
    /// <br />The major version.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ProductMajorVersion => GetStringNullable(nameof(ProductMajorVersion));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) through current version in updates beginning in late 2015.
    /// <br />The minor version.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ProductMinorVersion => GetStringNullable(nameof(ProductMinorVersion));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) through current version in updates beginning in late 2015.
    /// <br />Update level of the current build. CU indicates a cumulative update.
    /// <br />Returns one of the following values:
    /// <br />CUn = Cumulative Update
    /// <br />NULL = Not applicable.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ProductUpdateLevel => GetStringNullable(nameof(ProductUpdateLevel));

    /// <summary>
    /// Applies to: SQL Server 2012 (11.x) through current version in updates beginning in late 2015.
    /// <br />KB article for that release.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ProductUpdateReference => GetStringNullable(nameof(ProductUpdateReference));

    /// <summary>
    /// Version of the instance of SQL Server, in the form of major.minor.build.revision.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string ProductVersion => GetString(nameof(ProductVersion));

    /// <summary>
    /// Returns the date and time that the Resource database was last updated.
    /// <br />Base data type: datetime
    /// </summary>
    public DateTime ResourceLastUpdateDateTime => GetDateTime(nameof(ResourceLastUpdateDateTime));

    /// <summary>
    /// Returns the version Resource database.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string ResourceVersion => GetString(nameof(ResourceVersion));

    /// <summary>
    /// Both the Windows server and instance information associated with a specified instance of SQL Server.
    /// <br />NULL = Input isn't valid, or an error.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string? ServerName => GetStringNullable(nameof(ServerName));

    /// <summary>
    /// The SQL character set ID from the collation ID.
    /// <br />Base data type: tinyint
    /// </summary>
    public byte SqlCharSet => GetByte(nameof(SqlCharSet));

    /// <summary>
    /// The SQL character set name from the collation.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string SqlCharSetName => GetString(nameof(SqlCharSetName));

    /// <summary>
    /// The SQL sort order ID from the collation
    /// <br />Base data type: tinyint
    /// </summary>
    public byte SqlSortOrder => GetByte(nameof(SqlSortOrder));

    /// <summary>
    /// The SQL sort order name from the collation.
    /// <br />Base data type: nvarchar(128)
    /// </summary>
    public string SqlSortOrderName => GetString(nameof(SqlSortOrderName));

    /// <summary>
    /// The number of suspended databases on the server.
    /// <br />Base data type: int
    /// </summary>
    public int SuspendedDatabaseCount => GetInt(nameof(SuspendedDatabaseCount));
}
