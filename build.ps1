param(
    [Alias("action", "baction", "build_action", "a")]
    [Parameter(Mandatory=$false, Position=0, ValueFromPipeline=$false, HelpMessage="Build Action to execute")]
    [string]$buildAction,

    [Alias("btype", "build_type", "bt")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Build Type to use (DEBUG or RELEASE)")]
    [string]$buildType,

    [Alias("solution_file")]
    [Parameter(Mandatory=$false, ValueFromPipeline=$false, HelpMessage="Solution File")]
    [string]$slnFile
)
$VerbosePreference="Continue"
$MAGICSTRING = (New-Guid).ToString()
$SCRIPTDIR = (Resolve-Path $PSScriptRoot).Path

# https://stackoverflow.com/questions/25915450/how-to-use-extension-methods-in-powershell
# https://stackoverflow.com/a/77682793

# https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters?view=powershell-7.4
# https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_return?view=powershell-7.4



function TrimOrNull {
    [OutputType([string])]
    param (
        [Parameter(Position=0)] [object]$str
    )
    if (!$str) {
        return $null
    }
    $s = $str.ToString()
    if (!$s) {
        return $null
    }
    $s = $s.Trim()
    if ($s.Length -eq 0) {
        return $null
    }
    return $s
}

function ParseArg {
    [OutputType([string])]
    param (
        [Parameter(Position=0)] [string]$name,
        [Parameter(Position=1)] [object]$value,
        [Parameter(Position=2)] [string]$defaultValue,
        [Parameter(Position=3)] [string[]]$validValues
        # [Parameter(ValueFromRemainingArguments=$true)][String[]]$Hosts
    )

#    Write-Verbose ("$name" + ': $name=' + "$name")
#    Write-Verbose ("$name" + ': $value=' + "$value")
#    Write-Verbose ("$name" + ': $defaultValue=' + "$defaultValue")
#    Write-Verbose ("$name" + ': $validValues=' + "$validValues")

#    Write-Verbose "$name 1: $value"
    $value = TrimOrNull $value
#    Write-Verbose "$name 2: $value"

    if (!$value) {
        if ($defaultValue) {
            $value = $defaultValue
        }
    }
#    Write-Verbose "$name 3: $value"

    if (!$value) {
        throw [System.ArgumentNullException] "No value provided for $name"
    }

#    Write-Verbose "$name 4: $value"
    if ($validValues) {
        if ($value -notin $validValues) {
            throw [System.ArgumentException] "Invalid value provided for $name '$value'  ->  $validValues"
        }
    }
#    Write-Verbose "$name 5: $value"

    return $value
}

# Clear screen  https://superuser.com/a/1738611
Write-Output "$([char]27)[2J"  # ESC[2J clears the screen, but leaves the scrollback
Write-Output "$([char]27)[3J"  # ESC[3J clears the screen, including all scrollback


Write-Verbose "Script Directory: $SCRIPTDIR"
Write-Verbose "Magic String: $MAGICSTRING"


$buildAction = ParseArg -name "buildAction" -value $buildAction -defaultValue "BUILD" -validValues @("BUILD", "CLEAN", "NUGET", "NUGETPUSH")
$buildAction = $buildAction.ToUpper()
Write-Verbose "buildAction: $buildAction"

$buildType = ParseArg "buildType" $buildType "DEBUG" @("DEBUG", "RELEASE")
$buildType = $buildType.ToUpper()
Write-Verbose "buildType: $buildType"

$slnFile = ParseArg "slnFile" $slnFile $MAGICSTRING $null
$slnFile = TrimOrNull $slnFile
if (!$slnFile -or $slnFile -eq $MAGICSTRING) {
    $slnFile = $null
    $slnFiles = Get-ChildItem -Path $SCRIPTDIR -Filter *.sln | Select-Object # -First 1
    if ($slnFiles.Count -lt 1) {
        throw [System.IO.FileNotFoundException] "No solution files found"
    } elseif ($slnFiles.Count -gt 1) {
        throw [System.IO.FileNotFoundException] "$($slnFiles.Count) solution files found but was expecting 1 -> $slnFiles"    
    } else {
        $slnFile = $slnFiles[0]
    }
}

$slnFileInfo = [System.IO.FileInfo] $slnFile
if(!$slnFileInfo.Exists){
    throw [System.IO.FileNotFoundException] "Solution file .sln not found -> $slnFile"    
}
$slnFile = $slnFileInfo.FullName
Write-Verbose "slnFile: $slnFile"

$slnDirInfo = $slnFileInfo.Directory
$slnDir = $slnDirInfo.FullName
Write-Verbose "slnDir: $slnDir"

Set-Location -Path $slnDir

[System.IO.DirectoryInfo] ([IO.Path]::Combine($slnDir, 'Foo', 'Bar'))

$gitId = git -C $slnDir rev-parse HEAD | Out-String
$gitId = TrimOrNull $gitId
Write-Verbose "gitId: $gitId"

$gitBranch = git -C $slnDir branch --show-current | Out-String
$gitBranch = TrimOrNull $gitBranch
Write-Verbose "gitBranch: $gitBranch"

$gitUrl = git -C $slnDir config --get remote.origin.url | Out-String
$gitUrl = TrimOrNull $gitUrl
Write-Verbose "gitUrl: $gitUrl"

#$cmdOutput = <command> | Out-String

