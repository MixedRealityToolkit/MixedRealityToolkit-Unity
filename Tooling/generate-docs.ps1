# Copyright (c) Mixed Reality Toolkit Contributors
# Licensed under the BSD 3-Clause

<#
.SYNOPSIS
    Generates XML documentation files and validates CS1591 across MRTK production assemblies locally.

.DESCRIPTION
    Runs Unity in batch mode to compile production assemblies with /doc and /warnaserror:1591,
    surfacing any missing XML documentation on public APIs without requiring external CI tools.

.PARAMETER ProjectPath
    Path to the Unity project (defaults to UnityProjects/MRTKDevTemplate).

.PARAMETER OutputDirectory
    Directory where generated DLLs and XML files will be placed (defaults to artifacts/docs).

.PARAMETER AssemblyFilter
    Optional filter string to build only specific assemblies (e.g. "Input", "Core", "SpatialManipulation").

.PARAMETER WarnAsError
    Treat CS1591 missing XML documentation warnings as build errors (default: $true).

.PARAMETER IncludeEditor
    Include Editor assemblies in documentation generation.

.PARAMETER VerboseLog
    Enable verbose compiler output.

.EXAMPLE
    .\Tooling\generate-docs.ps1
    # Generates and validates docs for all production assemblies

.EXAMPLE
    .\Tooling\generate-docs.ps1 -AssemblyFilter Input
    # Validates only MixedReality.Toolkit.Input
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$ProjectPath = "$PSScriptRoot/../UnityProjects/MRTKDevTemplate",

    [Parameter(Position = 1)]
    [string]$OutputDirectory = "$PSScriptRoot/../artifacts/docs",

    [string]$AssemblyFilter = "",

    [bool]$WarnAsError = $true,

    [switch]$IncludeEditor,

    [switch]$VerboseLog
)

$ErrorActionPreference = "Stop"

$resolvedProject = Resolve-Path $ProjectPath
$resolvedOutput = [System.IO.Path]::GetFullPath($OutputDirectory)

if (-not (Test-Path $resolvedOutput)) {
    New-Item -ItemType Directory -Path $resolvedOutput -Force | Out-Null
}

$commonArgs = @(
    "-executeMethod", "MixedReality.Toolkit.Editor.DocGen.GenerateDocsBinariesBatchMode",
    "-docOutput:$resolvedOutput"
)
if ($AssemblyFilter) { $commonArgs += "-docFilter:$AssemblyFilter" }
if ($WarnAsError) { $commonArgs += "-docWarnAsError:true" } else { $commonArgs += "-docWarnAsError:false" }
if ($IncludeEditor) { $commonArgs += "-docIncludeEditor" }
if ($VerboseLog) { $commonArgs += "-docVerbose" }

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " MRTK Local Documentation Generator & CS1591 Validator" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Project:   $resolvedProject"
Write-Host "Output:    $resolvedOutput"
if ($AssemblyFilter) { Write-Host "Filter:    $AssemblyFilter" }
Write-Host "WarnAsErr: $WarnAsError"
Write-Host ""

# Check for 'unity' CLI in PATH
$unityCli = Get-Command "unity" -ErrorAction SilentlyContinue

if ($null -ne $unityCli) {
    Write-Host "Using Unity CLI: $($unityCli.Source)" -ForegroundColor Green
    $logFile = Join-Path $resolvedOutput "docgen.log"
    $cliArgs = @("run", "$resolvedProject", "--no-banner", "--no-tail", "-l", "$logFile", "--") + $commonArgs
    & unity @cliArgs
    $exitCode = $LASTEXITCODE

    if (Test-Path $logFile) {
        $docGenLines = Get-Content $logFile | Where-Object { $_ -match "\[DocGen\]" }
        if ($docGenLines) {
            Write-Host "`n--- DocGen Output Summary ---" -ForegroundColor Cyan
            foreach ($line in $docGenLines) {
                if ($line -match "FAILED|error CS") {
                    Write-Host $line -ForegroundColor Red
                } elseif ($line -match "warning CS") {
                    Write-Host $line -ForegroundColor Yellow
                } elseif ($line -match "PASSED|SUCCEEDED|compiled successfully") {
                    Write-Host $line -ForegroundColor Green
                } else {
                    Write-Host $line -ForegroundColor Gray
                }
            }
        }
    }
} else {
    # Fallback to direct Unity.exe
    $projectVersionPath = Join-Path $resolvedProject "ProjectSettings/ProjectVersion.txt"
    $unityVersion = "2021.3.45f2"
    if (Test-Path $projectVersionPath) {
        $versionLine = Get-Content $projectVersionPath | Where-Object { $_ -match "^m_EditorVersion:\s*(.+)$" } | Select-Object -First 1
        if ($versionLine -match "^m_EditorVersion:\s*(.+)$") {
            $unityVersion = $Matches[1].Trim()
        }
    }

    $unityExePath = "C:\Program Files\Unity\Hub\Editor\$unityVersion\Editor\Unity.exe"
    if (-not (Test-Path $unityExePath)) {
        Write-Error "Unity editor executable not found at '$unityExePath' and 'unity' CLI is not in PATH."
        exit 1
    }

    Write-Host "Using Unity executable: $unityExePath" -ForegroundColor Green
    $processArgs = @("-projectPath", "$resolvedProject", "-batchmode") + $commonArgs + @("-quit", "-logFile", "$logFile")
    
    $proc = Start-Process -FilePath $unityExePath -ArgumentList $processArgs -PassThru -Wait -NoNewWindow
    $exitCode = $proc.ExitCode
    
    if (Test-Path $logFile) {
        Get-Content $logFile | Select-String "\[DocGen\]" | ForEach-Object { Write-Host $_.Line }
    }
}

if ($exitCode -eq 0) {
    Write-Host "`nDocumentation generation and CS1591 validation SUCCEEDED." -ForegroundColor Green
} else {
    Write-Host "`nDocumentation generation or CS1591 validation FAILED (Exit code: $exitCode)." -ForegroundColor Red
}

exit $exitCode
