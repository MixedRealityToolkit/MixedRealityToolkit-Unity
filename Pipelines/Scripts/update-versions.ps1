# Copyright (c) Mixed Reality Toolkit Contributors
# Licensed under the BSD 3-Clause

<#
.SYNOPSIS
    Updates the version of the UPM packages in the project.
.DESCRIPTION
    Updates the version of the UPM packages in the project.
.PARAMETER PackagesRoot
    The root folder containing the packages.
.PARAMETER BuildNumber
    The fourth digit for the full version number for assembly versioning. This is the build number.
.PARAMETER ReleaseLabel
    The tag to append after the version (e.g. "internal" or "prerelease"). Leave blank for a release build.
.PARAMETER ExperimentLabel
    An additional tag to append after the version, to append after the release label (e.g. "pre.1"). Historically used for the MRTK3 packages that are still experimental.
.PARAMETER Revision
    The revision number for the build, to append after the release labal and suffix.
.PARAMETER ReleasePackages
    An array of the package names that are no longer  If the package isn't in this array, it will get labeled with the ExperimentLabel.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagesRoot,
    [ValidatePattern("\d+")]
    [string]$BuildNumber,
    [ValidatePattern("[A-Za-z]*")]
    [string]$ReleaseLabel = "",
    [ValidatePattern("([A-Za-z]+\.\d+)?")]
    [string]$ExperimentLabel = "",
    [ValidatePattern("(\d(\.\d+)*)?")]
    [string]$Revision = "",
    [string]$ReleasePackages = ""
)

$releasePkgs = $ReleasePackages.Split(",")
$PackagesRoot = Resolve-Path -Path $PackagesRoot

if (-not [string]::IsNullOrEmpty($BuildNumber)) {
    $BuildNumber = $BuildNumber.Trim('.')
    $BuildNumber = ".$BuildNumber"
}

if (-not [string]::IsNullOrEmpty($ReleaseLabel)) {
    $ReleaseLabel = $ReleaseLabel.Trim('.')
}

if (-not [string]::IsNullOrEmpty($ExperimentLabel)) {
    $ExperimentLabel = $ExperimentLabel.Trim('.')
}

if (-not [string]::IsNullOrEmpty($Revision)) {
    $Revision = $Revision.Trim('.')
}

Write-Host ""
Write-Host -ForegroundColor Green "=======================================" 
Write-Host -ForegroundColor Green "Updating All Package Versions"
Write-Host -ForegroundColor Green "======================================="
Write-Output "Project root: $PackagesRoot"
Write-Output "Release packages: $releasePkgs"

$year = "{0:D4}" -f (Get-Date).Year
$month = "{0:D2}" -f (Get-Date).Month
$day = "{0:D2}" -f (Get-Date).Day

# loop through package directories, update package version, assembly version, and build version hash for updating dependencies
Get-ChildItem -Path $PackagesRoot -Filter "package.json" -Recurse | ForEach-Object {
    $packageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*" -Path $_ | Select-Object -First 1

    if (-not $packageName) {
        return # this is not an MRTK package, so skip
    }

    $packageName = $packageName.Matches[0].Value
    $packagePath = $_.Directory

    Write-Host ""
    Write-Host -ForegroundColor Green "======================================="  
    Write-Host -ForegroundColor Green "Updating Package Version"
    Write-Host -ForegroundColor Green "=======================================" 
    Write-Output "Package name: $packageName"

    $inlineVersion = Select-String '"version" *: *"([0-9.]+)(-?[a-zA-Z0-9.]*)' -InputObject (Get-Content -Path $_)
    $version = $inlineVersion.Matches.Groups[1].Value
    
    $labelParts = @()
    
    if (-not [string]::IsNullOrEmpty($ReleaseLabel)) {
        $labelParts += $ReleaseLabel
    }

    if ((-not [string]::IsNullOrEmpty($ExperimentLabel)) -and 
        (-not $releasePkgs.Contains($packageName))) {
        $labelParts += $ExperimentLabel
    }

    if (-not [string]::IsNullOrEmpty($Revision)) {
        $labelParts += $Revision
    }

    $label = $labelParts -join "."
    if (-not [string]::IsNullOrEmpty($label)) {
        $label = "-" + $label
    }

    Write-Output "Patching package version to $version$label"
    ((Get-Content -Path $_ -Raw) -Replace '("version": )"(?:[0-9.]+|%version%)-?[a-zA-Z0-9.]*', "`$1`"$version$label") | Set-Content -Path $_ -NoNewline

    Get-ChildItem -Path $packagePath/AssemblyInfo.cs -Recurse | ForEach-Object {
        $assemblyInfo = Get-Content -Path $_ -Raw

        Write-Output "Patching assembly version to $version.0"
        $assemblyInfo = $assemblyInfo -Replace "\[assembly:.AssemblyVersion\(\`".*\`"\)\]", "[assembly: AssemblyVersion(`"$version.0`")]"

        Write-Output "Patching assembly file version to $version$BuildNumber"
        if ($assemblyInfo -Match "\[assembly: AssemblyFileVersion\`(\`".*\`"\)\]") {
            $assemblyInfo = $assemblyInfo -Replace "\[assembly: AssemblyFileVersion\`(\`".*\`"\)\]", "[assembly: AssemblyFileVersion(`"$version$BuildNumber`")]"
        } else {
            $assemblyInfo += "[assembly: AssemblyFileVersion(`"$version$BuildNumber`")]`r`n"
        }

        Write-Output "Patching assembly information version to $version$label"
        if ($assemblyInfo -Match "\[assembly: AssemblyInformationalVersion\`(\`".*\`"\)\]") {
            $assemblyInfo = $assemblyInfo -Replace "\[assembly: AssemblyInformationalVersion\`(\`".*\`"\)\]", "[assembly: AssemblyInformationalVersion(`"$version$label`")]"
        } else {
            $assemblyInfo += "[assembly: AssemblyInformationalVersion(`"$version$label`")]`r`n"
        }

        Set-Content -Path $_ -Value $assemblyInfo -NoNewline 
    }

    Write-Output "Patching CHANGELOG.md version to [$version$label] - $year-$month-$day"
    Get-ChildItem -Path $packagePath/CHANGELOG.md -Recurse | ForEach-Object {            
        (Get-Content -Path $_ -Raw) -Replace "## \[$version(-[a-zA-Z0-9.]+)?\] - \b\d{4}\b-\b(0[1-9]|1[0-2])\b-\b(0[1-9]|[12][0-9]|3[01])\b", "## [$version$label] - $year-$month-$day" | Set-Content -Path $_ -NoNewline
    }
}

Write-Host ""
Write-Host -ForegroundColor Green "=======================================" 
Write-Host -ForegroundColor Green "Successfully Updated Package Versions"
Write-Host -ForegroundColor Green "======================================="
Write-Host ""