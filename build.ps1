param(
    [Alias("action", "baction", "build_action", "a")]
    [Parameter(Mandatory=$false, Position=0, ValueFromPipeline=$false, HelpMessage="Build Action to execute")]
    [string]$buildAction,

    [Alias("btype", "build_type", "bt")]
    [Parameter(Mandatory=$false, Position=1, ValueFromPipeline=$false, HelpMessage="Build Type to use (DEBUG or RELEASE)")]
    [string]$buildType
)
$VerbosePreference="Continue"



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

    $value = TrimOrNull($value)

    if (!$value) {
        if (!$defaultValue) {
            $value = $defaultValue
        }
    }

    if (!$value) {
        throw [System.ArgumentNullException] "No value provided for $name"
    }

    if ($validValues) {
        if ($value -notin $validValues) {
            throw [System.ArgumentException] "Invalid value provided for $name '$value'  ->  $validValues"
        }
    }

    
}

# Clear screen  https://superuser.com/a/1738611
Write-Output "$([char]27)[2J"  # ESC[2J clears the screen, but leaves the scrollback
Write-Output "$([char]27)[3J"  # ESC[3J clears the screen, including all scrollback

Write-Verbose "Build Action Before: $buildAction"
$buildAction = $buildAction.TrimOrNull()
if (!$buildAction) {
    $buildAction = "BUILD"
}
$buildAction = $buildAction.ToUpper()
$buildActionValids = @("BUILD", "CLEAN", "NUGET", "NUGETPUSH")
if ($buildAction -notin $buildActionValids) {
    throw [System.ArgumentException] "Invalid value provided for buildAction '$buildAction'  ->  $buildActionValids"
}
Write-Verbose "Build Action: $buildAction"


$buildType = ($buildType.TrimOrNull() ?? "DEBUG").ToUpper()
$buildTypeValids = @("DEBUG", "RELEASE")
if ($buildType -notin $buildTypeValids) {
    throw [System.ArgumentException] "Invalid value provided for buildType '$buildType'  ->  $buildTypeValids"
}
Write-Verbose "Build Type: $buildType"


$slnDir = (Resolve-Path $PSScriptRoot).Path
Write-Verbose "Solution Directory: $slnDir"

$slnFiles = Get-ChildItem -Path $slnDir -Filter *.sln | Select-Object # -First 1

if ($slnFiles.Count -lt 1) {
    throw [System.IO.FileNotFoundException] "No solution files found"
}

if ($slnFiles.Count -gt 1) {
    throw [System.IO.FileNotFoundException] "$($slnFiles.Count) solution files found but was expecting 1 -> $slnFiles"    
}

$slnFile = $slnFiles[0]
Write-Verbose "Solution File: $slnFile"





