# Copyright (c) Mixed Reality Toolkit Contributors
# Licensed under the BSD 3-Clause

<#
.SYNOPSIS
    Updates the version of the UPM packages in the project with a release label, revision, and build number.
.DESCRIPTION
    The script will update the version of the package.json file with the new version label and revision number. This 
    script will also update the AssemblyInfo.cs file with the new version number and build number. Finally, this
    script will update the CHANGELOG.md file with the new version number and release date.
.PARAMETER PackagesRoot
    The root folder containing the packages.
.PARAMETER PrereleaseTag
    The tag to append after the version (e.g. "build", "internal" or "prerelease"). Leave blank for a release build.
.PARAMETER Revision
    The revision number for the build, to append after the release labal.
.PARAMETER BuildNumber
    The fourth digit for the full version number for assembly versioning. This is the build number.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagesRoot,
    [ValidatePattern("[A-Za-z]*")]
    [string]$PrereleaseTag = "",
    [ValidatePattern("(\d(\.\d+)*)?")]
    [string]$Revision = "",
    [ValidatePattern("\d+")]
    [string]$BuildNumber
)

$PackagesRoot = Resolve-Path -Path $PackagesRoot

if (-not [string]::IsNullOrEmpty($BuildNumber)) {
    $BuildNumber = $BuildNumber.Trim('.')
    $BuildNumber = ".$BuildNumber"
}

if (-not [string]::IsNullOrEmpty($PrereleaseTag)) {
    $PrereleaseTag = $PrereleaseTag.Trim('.')
}

if (-not [string]::IsNullOrEmpty($Revision)) {
    $Revision = $Revision.Trim('.')
}

Write-Host ""
Write-Host -ForegroundColor Green "=======================================" 
Write-Host -ForegroundColor Green "Updating All Package Versions"
Write-Host -ForegroundColor Green "======================================="
Write-Output "Project root: $PackagesRoot"

$year = "{0:D4}" -f (Get-Date).Year
$month = "{0:D2}" -f (Get-Date).Month
$day = "{0:D2}" -f (Get-Date).Day

# loop through package directories, update package version, assembly version, and build version hash for updating dependencies
Get-ChildItem -Path $PackagesRoot -Filter "package.json" -Recurse | ForEach-Object {
    # Get the content of the package.json file
    $jsonContent = Get-Content $_.FullName | Out-String | ConvertFrom-Json

    # Read the package name
    $packageName = $jsonContent.name

    # Test is package name starts with org.mixedrealitytoolkit to verify it's an MRTK package
    if ($packageName -notmatch "^org\.mixedrealitytoolkit\.\w+(\.\w+)*") {
        return 
    }

    # Read the version value
    $version = $jsonContent.version

    # Get the package path
    $packagePath = $_.Directory

    Write-Host ""
    Write-Host -ForegroundColor Green "======================================="  
    Write-Host -ForegroundColor Green "Updating Package Version"
    Write-Host -ForegroundColor Green "=======================================" 
    Write-Output "Package name: $packageName"

    # This regex will match the a valid version is the package.json file. Some examples of valid versions are:
    #
    #   1.0.0
    #   1.0.0-pre.1
    #   1.0.0-development.pre.1
    #   1.0.0-development  
    #
    # In these example "development" is the prerelease tag and "pre.1" is the meta tag.    
    $validVersion = $version -match '(?<version>[0-9.]+)(-((?<prereleaseTag>[a-zA-Z0-9]*)?(?=(|\.[a-zA-Z]*\.[0-9]*)(\.[0-9]{6}\.[0-9]|$))|)((?(?<=-)|\.)(?<metaTag>[a-zA-Z][a-zA-Z0-9]*)\.(?<metaTagVersion>[1-9][0-9]*))?)?'
    if (-not $validVersion) {
        throw "Failed to parse version out of the package.json file at $($_.FullName)"
    }
    
    # Get the version parts from the $Matches variable, and verify that the version key exists.
    $versionParts = $Matches
    if (-not $versionParts.ContainsKey('version')) {
        throw "Failed to parse version out of the package.json file at $($_.FullName)"
    }

    # Get the version
    $version = $versionParts['version']

    # Get all tag parts to append to the version
    $tagParts = @()
    
    # Add the new version label if it's not empty
    if (-not [string]::IsNullOrEmpty($PrereleaseTag)) {
        $tagParts += $PrereleaseTag
    }

    # Add the optional metatag tag and version if found in match
    if ($versionParts.ContainsKey('metaTag') -and $versionParts.ContainsKey('metaTagVersion')) {        
        $tagParts += $versionParts['metaTag']
        $tagParts += $versionParts['metaTagVersion']
    }

    # Add the revision number if it's not empty
    if (-not [string]::IsNullOrEmpty($Revision)) {
        $tagParts += $Revision
    }

    # Create a full tag string with prerelease tag and the meta tag
    $tag = $tagParts -join "."
    if (-not [string]::IsNullOrEmpty($tag)) {
        $tag = "-" + $tag
    }

    # Update the version with the new tag    
    $jsonContent.version = "$version$tag"

    # Write json content back to the file
    Write-Output "Patching package version to $version$tag"
    $jsonContent | ConvertTo-Json -Depth 10 | Set-Content -Path $_.FullName

    # Update the assembly version in the AssemblyInfo.cs file
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

        Write-Output "Patching assembly information version to $version$tag"
        if ($assemblyInfo -Match "\[assembly: AssemblyInformationalVersion\`(\`".*\`"\)\]") {
            $assemblyInfo = $assemblyInfo -Replace "\[assembly: AssemblyInformationalVersion\`(\`".*\`"\)\]", "[assembly: AssemblyInformationalVersion(`"$version$tag`")]"
        } else {
            $assemblyInfo += "[assembly: AssemblyInformationalVersion(`"$version$tag`")]`r`n"
        }

        Set-Content -Path $_ -Value $assemblyInfo -NoNewline 
    }

    # Update the CHANGELOG.md file with the new version and release date
    Write-Output "Patching CHANGELOG.md version to [$version$tag] - $year-$month-$day"
    Get-ChildItem -Path $packagePath/CHANGELOG.md -Recurse | ForEach-Object {            
        (Get-Content -Path $_ -Raw) -Replace "## \[$version(-[a-zA-Z0-9.]+)?\] - \b\d{4}\b-\b(0[1-9]|1[0-2])\b-\b(0[1-9]|[12][0-9]|3[01])\b", "## [$version$tag] - $year-$month-$day" | Set-Content -Path $_ -NoNewline
    }
}

Write-Host ""
Write-Host -ForegroundColor Green "=======================================" 
Write-Host -ForegroundColor Green "Successfully Updated Package Versions"
Write-Host -ForegroundColor Green "======================================="
Write-Host ""