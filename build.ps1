param(
    [Alias("action", "actions", "baction", "bactions", "build_action", "build_actions", "a")]
    [Parameter(Mandatory=$false, Position=0, ValueFromPipeline=$false, HelpMessage="Build Action to execute [ CleanProjectDirs | CleanBuildDirs | Build | NugetCopy | NugetPush ]")]
    [string]$buildActions,

    [Alias("btype", "build_type", "bt", "t")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Build Type to use (DEBUG or RELEASE)")]
    [string]$buildType,

    [Alias("solution_file", "sln", "slnx")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Solution File")]
    [string]$solutionFile,

    [Alias("v")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Enable extra log information")]
    [switch]$logVerbose = $false,

    [Alias("d", "vv")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Enable lots of log information")]
    [switch]$logDebug = $false
)

$logDebug = $true
$logVerbose = $true

if ($logDebug) { $DebugPreference = "Continue" }
if ($logVerbose) { $VerbosePreference = "Continue" }
$InformationPreference = "Continue"
$WarningPreference = "Continue"
$ErrorActionPreference = "Stop"

$builderLog = [System.Action[string, object, System.Exception]]{
    param(
        [Parameter(Mandatory=$false, Position=0)] [string] $level, 
        [Parameter(Mandatory=$false, Position=1)] [object] $msg, 
        [Parameter(Mandatory=$false, Position=2)] [System.Exception] $e
    )
    $ee = ""
    if ($e) { $ee = "  --> [$($e.GetType().Name)] $($e.Message)" }
    $msge = "$($msg)$($ee)"

    switch ($level) {
        "Debug" { Write-Debug $msge }
        "Verbose" { Write-Verbose $msge }
        "Information" { Write-Information $msge }
        "Warning" { Write-Warning $msge }
        "Error" { Write-Error -Message $msg -Exception $e }
        default { throw [System.ApplicationException] "builderLog received invalid level '$level'" }
    }
}

function RunCmdString{
    param(
        [Parameter(Mandatory=$false, Position=0)] [string] $cmd, 
        [Parameter(Mandatory=$false, Position=1)] [string[]] $cmdParams
    )
    $r = & $cmd $cmdParams | Out-String
    Write-Verbose "$cmd $($cmdParams -join ' ')    -->   $r"   
    return $r
}

function Choice{
    param(
        [Parameter(Mandatory=$true, Position=0)] [string] $label,
        [Parameter(Mandatory=$true, Position=1)] [string] $helpMessage 
    )
    return New-Object System.Management.Automation.Host.ChoiceDescription $label, $helpMessage
}

#region Actions

function Action_CleanProjectDirs { param ($b)
    $b.CleanProjectDirs()
}

function Action_CleanBuildDirs { param ($b)
    $b.CleanBuildDirs()
}

function Action_Build { param ($b)
    $b.LogI("Building Projects")

    # $dotnetProps = "`"/p:RepositoryCommit=$($b.GitId);RepositoryBranch=$($b.GitBranch);RepositoryUrl=$($b.GitUrl)`""  # https://stackoverflow.com/a/51485481    
    # dotnet build "$($b.SolutionFile)" --configuration "$($b.BuildType)" --nologo --version-suffix "$($b.VersionSuffix)" "$($b.GetDotNetProperties())"
    # dotnet build "$($b.SolutionFile)" --configuration "$($b.BuildType)" --nologo --version-suffix "$($b.VersionSuffix)" "`"/p:RepositoryCommit=$($b.GitId);RepositoryBranch=$($b.GitBranch);RepositoryUrl=$($b.GitUrl)`""

    [string]$cmd = 'dotnet'
    [string[]]$cmdParams = @(
        'build', "`"$($b.SolutionFile)`""
    ,   '--configuration', "`"$($b.BuildType)`""
    ,   '--nologo'
    ,   '--force'
    ,   '--version-suffix', "`"$($b.BuildVersionSuffix)`""
    ,   "`"/p:RepositoryCommit=$($b.GitId);RepositoryBranch=$($b.GitBranch);RepositoryUrl=$($b.GitUrl);SymbolPackageFormat=snupkg`""
    )

    & $cmd $cmdParams
}

function Action_NugetCopy { param ($b)
    $b.CopyNugetFiles()
}

function Action_NugetPush { param ($b)
    # dotnet nuget push "$filename" --api-key $keyNuGet --source https://api.nuget.org/v3/index.json --skip-duplicate

    foreach($file in $b.BuildDirNugetFiles) {
        [string]$cmd = 'dotnet'
        [string[]]$cmdParams = @(
            'nuget', 
        ,   'push', "$file"
        ,   '--source', 'https://api.nuget.org/v3/index.json'
        ,   '--api-key', "$($b.GetGitKey())"
        ,   '--skip-duplicate'
        )

        $filename = [IO.Path]::GetFileName($file)
        $title    = "Nuget.org:  $file"
        $question = "Confirm upload to Nuget?  $filename" 
        $choices  = '&Yes', '&No'

        $decision = $Host.UI.PromptForChoice($title, $question, $choices, 1)
        if ($decision -eq 0) {
            & $cmd $cmdParams
        } else {
            Write-Information "Skipping: $filename"
        }
        Write-Information ''
        #Write-Information "$cmd $($cmdParams -join ' ')"
    }
}

#endregion Actions

$SCRIPTDIR = (Resolve-Path $PSScriptRoot).Path
Write-Verbose "Script Directory: $SCRIPTDIR"
Write-Verbose "Working Directory: $PWD"

Write-Verbose "START"

# https://stackoverflow.com/questions/25915450/how-to-use-extension-methods-in-powershell
# https://stackoverflow.com/a/77682793
# https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters?view=powershell-7.4
# https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_return?view=powershell-7.4
# https://stackoverflow.com/questions/24868273/run-a-c-sharp-cs-file-from-a-powershell-script

$CLASS_NAME = "Builder"
$buildCsScriptFilePath = [IO.Path]::Combine($SCRIPTDIR, "builder", "$CLASS_NAME.cs")
$buildCsScript = Get-Content -Raw -Path $buildCsScriptFilePath

#$CLASS_ID_SUFFIX = (New-Guid).ToString().Replace("-", "")
$CLASS_NAME_NEW = $CLASS_NAME + '_' + ([long]([System.DateTime]::UtcNow.Ticks / 10000)).ToString()
$buildCsScript = $buildCsScript.Replace("class $CLASS_NAME", "class $CLASS_NAME_NEW")
Add-Type -TypeDefinition $buildCsScript
$CLASS_NAME = $CLASS_NAME_NEW



#$buiderLog = [System.Action[string, object, System.Exception]]$buiderLogImpl
#$builderLog = $builderLogImpl

$parameters = @{
    TypeName = "MaxRunSoftware.$CLASS_NAME"
    # ArgumentList = ($buiderLog, $builderScriptDir)
}
$b = New-Object @parameters



# Clear screen  https://superuser.com/a/1738611
#Write-Output "$([char]27)[2J"  # ESC[2J clears the screen, but leaves the scrollback
#Write-Output "$([char]27)[3J"  # ESC[3J clears the screen, including all scrollback



$b.ScriptDir = $SCRIPTDIR
$b.Log = $builderLog
$b.BuildType = $buildType
$b.BuildVersionType = "beta"
$b.GitKeyPath = "~/Keys/MaxRunSoftware_NuGet.txt"

#$b.GitId = git -C $($b.SlnDir) rev-parse HEAD | Out-String
$b.GitId = RunCmdString 'git' @('-C', "$($b.SolutionDir)", 'rev-parse', 'HEAD') 

#$b.GitBranch = git -C $b.SlnDir branch --show-current | Out-String
$b.GitBranch = RunCmdString 'git' @('-C', "$($b.SolutionDir)/", 'branch', '--show-current') 

#$b.GitUrl = git -C $b.SlnDir config --get remote.origin.url | Out-String
$b.GitUrl = RunCmdString 'git' @('-C', "$($b.SolutionDir)/", 'config', '--get', 'remote.origin.url') 

$b.BuildDateTime = [System.DateTime]::UtcNow
$b.ProjectsToIncludeNuget = "Common Data Ftp Microsoft Windows"
$b.FileExtensionsToCleanInBuildDirs = "exe dll xml zip txt nupkg snupkg nuspec"

#$b.DebugDoNotDelete = $true


Write-Debug $b


#$buildActionsSet = New-Object System.Collections.Generic.HashSet[string]
$buildActionsSet = [System.Collections.Generic.HashSet[String]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($action in $b.SplitAndTrim($buildActions)) {
    $buildActionsSet.Add($action);
}

function Menu { param ($b, $bas)
    $title    = "Action"
    $question = 'No build action specified'
    [System.Management.Automation.Host.ChoiceDescription[]] $choices = @()
    $choices += Choice 'CleanBuild&Dirs' 'Cleans build directories'
    $choices += Choice '&CleanProjectDirs' 'Cleans project directories'
    $choices += Choice '&Build' 'Builds all projects'
    $choices += Choice '&NugetCopy' 'Copies project .nupkg files to build directory'
    $choices += Choice 'Nuget&Push' 'Pushes .nupkg files to nuget.org'
    $choices += Choice '&Quit' 'Pushes .nupkg files to nuget.org'


    #$choices  = '&clean', '&build', '&nuget', 'nuget&push'

    $decisions = $Host.UI.PromptForChoice($title, $question, $choices, [int[]](0))
    Write-Verbose "Decisions: $decision"
    foreach($d in $b.SplitAndTrim($decisions)) {
        $c = $choices[$d].Label.Replace('&', '')
        $bas.Add($c);
    }
    #$buildActions = $choices[$decisions].Label.Replace('&', '')
}

while ($buildActionsSet.Count -lt 1) {
    Menu $b $buildActionsSet
}

Write-Information "Actions: $buildActionsSet"

if ($buildActionsSet.Count -lt 1) {
    Write-Error "No build action specified"
}
if ($buildActionsSet.Contains("CleanProjectDirs")) {
    Action_CleanProjectDirs $b
}
if ($buildActionsSet.Contains("CleanBuildDirs")) {
    Action_CleanBuildDirs $b
}
if ($buildActionsSet.Contains("Build")) {
    Action_Build $b 
}
if ($buildActionsSet.Contains("NugetCopy")) {
    Action_NugetCopy $b
}
if ($buildActionsSet.Contains("NugetPush")) {
    Action_NugetPush $b
}
if ($buildActionsSet.Contains("Quit")) {
    Write-Information "Quiting..."
}


#$cmdOutput = <command> | Out-String


