param(
    [Alias("action", "baction", "build_action", "a")]
    [Parameter(Mandatory=$false, Position=0, ValueFromPipeline=$false, HelpMessage="Build Action to execute")]
    [string]$buildAction,

    [Alias("btype", "build_type", "bt", "t")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Build Type to use (DEBUG or RELEASE)")]
    [string]$buildType,

    [Alias("solution_file", "sln")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Solution File")]
    [string]$slnFile,

    [Alias("v")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Enable extra log information")]
    [switch]$logVerbose = $false,

    [Alias("d", "vv")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Enable lots of log information")]
    [switch]$logDebug = $false
)

if ($logDebug) { $DebugPreference = "Continue" }
if ($logVerbose) { $VerbosePreference = "Continue" }
$InformationPreference = "Continue"
$WarningPreference = "Continue"
$ErrorActionPreference = "Stop"

#$buildAction = "nugetpush"

$SCRIPTDIR = (Resolve-Path $PSScriptRoot).Path
Write-Verbose "Script Directory: $SCRIPTDIR"
Write-Verbose "Working Directory: $PWD"

Write-Verbose "START"

# https://stackoverflow.com/questions/25915450/how-to-use-extension-methods-in-powershell
# https://stackoverflow.com/a/77682793
# https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters?view=powershell-7.4
# https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_return?view=powershell-7.4
# https://stackoverflow.com/questions/24868273/run-a-c-sharp-cs-file-from-a-powershell-script

$buildCsScriptFilePath = [IO.Path]::Combine($SCRIPTDIR, 'builder', 'Builder.cs')
$buildCsScript = Get-Content -Raw -Path $buildCsScriptFilePath

$CLASS_ID_SUFFIX = (New-Guid).ToString().Replace("-", "")
$buildCsScript = $buildCsScript.Replace("class Builder", "class Builder_$CLASS_ID_SUFFIX")
$buildCsScript = $buildCsScript.Replace("class Extensions", "class Extensions_$CLASS_ID_SUFFIX")
Add-Type -TypeDefinition $buildCsScript

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

#$buiderLog = [System.Action[string, object, System.Exception]]$buiderLogImpl
#$builderLog = $builderLogImpl

$parameters = @{
    TypeName = "MaxRunSoftware.Builder_$CLASS_ID_SUFFIX"
    # ArgumentList = ($buiderLog, $builderScriptDir)
}
$b = New-Object @parameters



# Clear screen  https://superuser.com/a/1738611
#Write-Output "$([char]27)[2J"  # ESC[2J clears the screen, but leaves the scrollback
#Write-Output "$([char]27)[3J"  # ESC[3J clears the screen, including all scrollback

if (!$b.TrimOrNull($buildAction)) {
    $title    = "Action"
    $question = 'No build action specified' 
    $choices  = '&clean', '&build', '&nuget', 'nuget&push'

    $decision = $Host.UI.PromptForChoice($title, $question, $choices, 1)
    $buildAction = $choices[$decision].Replace('&', '')
}

$b.ScriptDir = $SCRIPTDIR
$b.Log = $builderLog
$b.BuildType = $buildType
$b.BuildAction = $buildAction
$b.BuildVersionType = "beta"
$b.GitKeyPath = "~/Keys/MaxRunSoftware_NuGet.txt"

$b.DebugDoNotDelete = $false

$b.Init()
Write-Debug $b

function RunCmdString{
    param(
        [Parameter(Mandatory=$false, Position=0)] [string] $cmd, 
        [Parameter(Mandatory=$false, Position=1)] [string[]] $cmdParams
    )
    $r = & $cmd $cmdParams | Out-String
    Write-Verbose "$cmd $($cmdParams -join ' ')    -->   $r"   
    return $r
}

#$b.GitId = git -C $($b.SlnDir) rev-parse HEAD | Out-String
#$b.GitBranch = git -C $b.SlnDir branch --show-current | Out-String
#$b.GitUrl = git -C $b.SlnDir config --get remote.origin.url | Out-String

$b.GitId = RunCmdString 'git' @('-C', "$($b.SlnDir)", 'rev-parse', 'HEAD') 
$b.GitBranch = RunCmdString 'git' @('-C', "$($b.SlnDir)/", 'branch', '--show-current') 
$b.GitUrl = RunCmdString 'git' @('-C', "$($b.SlnDir)/", 'config', '--get', 'remote.origin.url') 





function ActionClean {
    $b.CleanProjectDirs()
}

function ActionBuild {
    $b.LogI("Building Projects")

    # $dotnetProps = "`"/p:RepositoryCommit=$($b.GitId);RepositoryBranch=$($b.GitBranch);RepositoryUrl=$($b.GitUrl)`""  # https://stackoverflow.com/a/51485481    
    # dotnet build "$($b.SlnFile)" --configuration "$($b.BuildType)" --nologo --version-suffix "$($b.VersionSuffix)" "$($b.GetDotNetProperties())"
    # dotnet build "$($b.SlnFile)" --configuration "$($b.BuildType)" --nologo --version-suffix "$($b.VersionSuffix)" "`"/p:RepositoryCommit=$($b.GitId);RepositoryBranch=$($b.GitBranch);RepositoryUrl=$($b.GitUrl)`""

    [string]$cmd = 'dotnet'
    [string[]]$cmdParams = @(
        'build', "`"$($b.SlnFile)`""
    ,   '--configuration', "`"$($b.BuildType)`""
    ,   '--nologo'
    ,   '--force'
    ,   '--version-suffix', "`"$($b.BuildVersionSuffix)`""
    ,   "`"/p:RepositoryCommit=$($b.GitId);RepositoryBranch=$($b.GitBranch);RepositoryUrl=$($b.GitUrl);SymbolPackageFormat=snupkg`""
    )

    & $cmd $cmdParams
}

function ActionNuget {
    $b.CopyNugetFiles("scheduler", "microsoft")
}

function ActionNugetPush {
    # dotnet nuget push "$filename" --api-key $keyNuGet --source https://api.nuget.org/v3/index.json --skip-duplicate

    foreach($file in $b.BuildDirNugetFiles) {
        [string]$cmd = 'dotnet'
        [string[]]$cmdParams = @(
            'nuget', 
        ,   'push', "`"$file`""
        ,   '--source', 'https://api.nuget.org/v3/index.json'
        ,   '--api-key', "$($b.ReadGitKey())"
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


if ($buildAction -eq "clean") {
    ActionClean
}
elseif ($buildAction -eq "build") {
    ActionClean
    ActionBuild
}
elseif ($buildAction -eq "nuget") {
    ActionClean
    ActionBuild
    ActionNuget
}
elseif ($buildAction -eq "nugetpush") {
    ActionNugetPush
}


#$cmdOutput = <command> | Out-String


