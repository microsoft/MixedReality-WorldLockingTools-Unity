<#
.SYNOPSIS
    Builds .unitypackage artifacts for MixedReality-WorldLockingTools
.DESCRIPTION
    This script builds the following set of .unitypackages:

    - CoreEngine
    
    This contains the WorldLocking.Core and WorldLocking.Engine
    content.

    - Examples_wMRTKV2.2

    This contains all of the content under WorldLocking.Examples
    as well as MRTK V2.2 on which it depends.

    - Tools
    
    This contains the WorldLocking.Tools content.

    Defaults to assuming that the current working directory of the script is in the root
    directory of the repo.
.PARAMETER OutputDirectory
    Where should we place the output? Defaults to ".\artifacts"
.PARAMETER RepoDirectory
    The root location of the repo. Defaults to "." which assumes that the script
    is running in the root folder of the repo.
.PARAMETER LogDirectory
    The location where Unity logs will be stored. Defaults to the current
    working directory.
.PARAMETER UnityDirectory
    The Unity install directory that will be used to build packages.
.PARAMETER Clean
    If true, the OutputDirectory will be recursively deleted prior to package
    generation.
.PARAMETER Verbose
    If true, verbose messages will be displayed.
.EXAMPLE
    .\unitypackage.ps1

    This will generate packages that look like:
    artifacts\Microsoft.WorldLockingTools.Unity.CoreEngine.unitypackage
    artifacts\Microsoft.WorldLockingTools.Unity.Examples_wMRTKV2.2.unitypackage
    artifacts\Microsoft.WorldLockingTools.Unity.Tools.unitypackage
.EXAMPLE
    .\build.ps1 -OutputDirectory .\out -Clean

    This will generate packages that look like:
    out\Microsoft.WorldLockingTools.Unity.CoreEngine.unitypackage
    out\Microsoft.WorldLockingTools.Unity.Examples_wMRTKV2.2.unitypackage
    out\Microsoft.WorldLockingTools.Unity.Tools.unitypackage
#>
param(
    [string]$OutputDirectory = ".\artifacts",
    [string]$RepoDirectory = ".",
    [string]$LogDirectory,
    [string]$UnityDirectory,
    [switch]$Clean,
    [switch]$Verbose
)

if ( $Verbose ) { $VerbosePreference = 'Continue' }

# This hashtable contains mapping of the packages (by name) to the set
# of top level folders that should be included in that package.
# The keys of this hashtable will contribute to the final naming of the package
# (for example, in Microsoft.WorldLockingTools.Unity.CoreEngine, the CoreEngine
# section comes from the key below).
#
# Note that capitalization below in the key itself is significant. Capitalization
# in the values is not significant.
#
# These paths are project-root relative.
$packages = @{
    "CoreEngine" = @(
        "Assets\WorldLocking.Core",
        "Assets\WorldLocking.Engine"
    );
    "Examples_wMRTKV2.2" = @(
        "Assets\WorldLocking.Examples",
        "Assets\MRTK"
    );
    "Tools" = @(
        "Assets\WorldLocking.Tools"
    );
}

# Beginning of the .unitypackage script main section
# The overall structure of this script looks like:
#
# 1) Checking that UnityVersion value is present and valid.
# 2) Ensures that the output directory and log directory exists.
# 3) Uses the Unity editor's ExportPackages functionality (using the -exportPackages)
#    to build the .unitypackage files.

Write-Verbose ".unitypackage generation beginning"

Write-Verbose "Reconciling Unity binary:"
if (-not $UnityDirectory) {
    throw "-UnityDirectory is a required flag"
}

$unityEditor = Get-ChildItem $UnityDirectory -Filter 'Unity.exe' -Recurse | Select-Object -First 1 -ExpandProperty FullName
if (-not $unityEditor) {
    throw "Unable to find the unity editor executable in $UnityDirectory"
}
Write-Verbose $unityEditor;

if ($Clean) {
    Write-Verbose "Recursively deleting output directory: $OutputDirectory"
    Remove-Item -ErrorAction SilentlyContinue $OutputDirectory -Recurse 
}

if (-not (Test-Path $OutputDirectory -PathType Container)) {
    New-Item $OutputDirectory -ItemType Directory | Out-Null
}

if (-not $LogDirectory) {
    $LogDirectory = "."
} else {
    New-Item $LogDirectory -ItemType Directory | Out-Null
}

$OutputDirectory = Resolve-Path $OutputDirectory
$LogDirectory = Resolve-Path $LogDirectory
$RepoDirectory = Resolve-Path $RepoDirectory

foreach ($entry in $packages.GetEnumerator()) {
    $packageName = $entry.Name;
    $folders = $entry.Value

    $logFileName = "$LogDirectory\Build-UnityPackage-$packageName.log"
    $unityPackagePath = "$OutputDirectory\Microsoft.WorldLockingTools.Unity.${packageName}.unitypackage";
    
    # The exportPackages flag expects the last value in the array
    # to be the final output destination.
    $exportPackages = $folders + $unityPackagePath
    $exportPackages = $exportPackages -join " "

    Write-Verbose "Generating .unitypackage: $unityPackagePath"
    Write-Verbose "Log location: $logFileName"

    # Assumes that unity package building has failed, unless we
    # succeed down below after running the Unity packaging step.
    $exitCode = 1

    try {
        $unityArgs = "-BatchMode -Quit -Wait " +
            "-projectPath $RepoDirectory " +
            "-exportPackage $exportPackages " +
            "-logFile $logFileName"

        # Starts the Unity process, and the waits (and shows output from the editor in the console
        # while the process is still running.)
        $proc = Start-Process -FilePath "$unityEditor" -ArgumentList "$unityArgs" -PassThru
        $ljob = Start-Job -ScriptBlock { param($log) Get-Content "$log" -Wait } -ArgumentList $logFileName

        while (-not $proc.HasExited -and $ljob.HasMoreData)
        {
            Receive-Job $ljob
            Start-Sleep -Milliseconds 200
        }
        Receive-Job $ljob
        Stop-Job $ljob
        Remove-Job $ljob

        Stop-Process $proc

        $exitCode = $proc.ExitCode
        if (($proc.ExitCode -eq 0) -and (Test-Path $unityPackagePath)) {
            Write-Verbose "Successfully created $unityPackagePath"
        }
        else {
            # It's possible that $exitCode could have been set to a zero value
            # despite the package not being there - in that case this should still return
            # failure (i.e. a non-zero exit code)
            $exitCode = 1
            Write-Error "Failed to create $unityPackagePath"
        }
    }
    catch { Write-Error $_ }

    if ($exitCode -ne 0) {
        exit $exitCode
    }
}