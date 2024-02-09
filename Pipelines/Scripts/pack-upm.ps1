# Copyright (c) Mixed Reality Toolkit Contributors
# Licensed under the BSD 3-Clause

<#
.SYNOPSIS
    Builds the Mixed Reality Toolkit Unity Package Manager (UPM) packages.
.DESCRIPTION
    Builds UPM packages for the Mixed Reality Toolkit.
.PARAMETER ProjectRoot
    The root folder of the project.
.PARAMETER OutputDirectory
    Where should we place the output? Defaults to ".\artifacts"
.PARAMETER BuildNumber
    The fourth digit for the full version number for assembly versioning. This is the build number.
.PARAMETER ReleaseLabel
    The tag to append after the version (e.g. "internal" or "prerelease"). Leave blank for a release build.
.PARAMETER ExperimentLabel
    An additional tag to append after the version, to append after the release label (e.g. "pre.1"). Historically used for the MRTK3 packages that are still experimental.
.PARAMETER Revision
    The revision number for the build, to append after the release labal and suffix.
.PARAMETER ReleasePackages
    An array of the package names that have been released, and no longer in experimentation. If the package isn't in this array, it will get labeled with the ExperimentLabel.

#>
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectRoot,
    [string]$OutputDirectory = "./artifacts/upm",
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

$ProjectRoot = Resolve-Path -Path $ProjectRoot

if (-not (Test-Path $OutputDirectory -PathType Container)) {
    New-Item $OutputDirectory -ItemType Directory | Out-Null
}

$OutputDirectory = Resolve-Path -Path $OutputDirectory

Write-Host ""
Write-Host -ForegroundColor Blue "======================================="  
Write-Host -ForegroundColor Blue "Packing All Packages"
Write-Host -ForegroundColor Blue "=======================================" 
Write-Host "OutputDirectory: $OutputDirectory"

try {
    Push-Location $OutputDirectory

    # Update package versions
    . $PSScriptRoot\update-versions.ps1 -PackagesRoot $ProjectRoot -BuildNumber $BuildNumber -ReleaseLabel $ReleaseLabel -ExperimentLabel $ExperimentLabel -Revision $Revision -ReleasePackages $ReleasePackages

    # Loop through package directories and copy documentation
    Get-ChildItem -Path $ProjectRoot/*/package.json | ForEach-Object {
        $packageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*" -Path $_ | Select-Object -First 1

        if (-not $packageName) {
            return # this is not an MRTK package, so skip
        }

        $packageName = $packageName.Matches[0].Value
        $packagePath = $_.Directory
        $docFolder = "$packagePath/Documentation~"

        Write-Host ""
        Write-Host -ForegroundColor Green "======================================="  
        Write-Host -ForegroundColor Green "Copying Documentation~"
        Write-Host -ForegroundColor Green "=======================================" 
        Write-Host "Package name: $packageName"

        if (Test-Path -Path $docFolder) {
            Copy-Item -Path "$ProjectRoot/Pipelines/UPM/Documentation~/*" -Destination $docFolder -Recurse
        }
        else {
            Copy-Item -Path "$ProjectRoot/Pipelines/UPM/Documentation~" -Destination $docFolder -Recurse
        }  
    }

    # Package the package directories
    Get-ChildItem -Path $ProjectRoot/*/package.json | ForEach-Object {
        $currentPackageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*" -Path $_ | Select-Object -First 1
        
        if (-not $currentPackageName) {
            return # this is not an MRTK package, so skip
        }

        Write-Host ""
        Write-Host -ForegroundColor Green "======================================="  
        Write-Host -ForegroundColor Green "Packing Package"
        Write-Host -ForegroundColor Green "======================================="
        Write-Host "Package name: $currentPackageName" 

        $currentPackageName = $currentPackageName.Matches[0].Value
        $packageFriendlyName = (Select-String -Pattern "`"displayName`": `"(.+)`"" -Path $_ | Select-Object -First 1).Matches.Groups[1].Value

        $packagePath = $_.Directory
        $docFolder = "$packagePath/Documentation~"

        # build the package
        npm pack $packagePath

        # clean up
        if (Test-Path -Path $docFolder) {
            Write-Host "Cleaning up Documentation~ from $packageFriendlyName"
            # A documentation folder was created. Remove it.            
            Remove-Item -Path $docFolder -Recurse -Force

            # But restore anything that's checked-in.
            if (git -C $packagePath ls-files $docFolder) {
                git -C $packagePath checkout $docFolder
            }
        }

        git -C $packagePath checkout $_
        Get-ChildItem -Path $packagePath/AssemblyInfo.cs -Recurse | ForEach-Object {
            git -C $packagePath checkout $_
        }
    }

    Write-Host ""
    Write-Host -ForegroundColor Blue "======================================="  
    Write-Host -ForegroundColor Blue "Successfully Packed All Pacakges"
    Write-Host -ForegroundColor Blue "======================================="
    Write-Host ""
}
finally {
    Pop-Location
}
