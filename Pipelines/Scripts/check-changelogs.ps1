# Copyright (c) Mixed Reality Toolkit Contributors
# Licensed under the BSD 3-Clause

<#
.SYNOPSIS
    Validates that changelogs have been properly updated for changed files.
.DESCRIPTION
    Validates that changelogs have been properly updated for changed files.
.EXAMPLE
    .\check-changelogs.ps1 -ChangesFile c:\path\to\changes\file.txt
#>
param(
    # The filename containing the list of files to scope the code validation
    # to. This is useful in pull request validation when there isn't a need
    # to check every single file in the repo for changes (i.e. only the list
    # of changed files)
    [Parameter(Mandatory = $true)]
    [string]$ChangesFile
)

$changelogUpdated = @{ }

# If the file containing the list of changes was provided and actually exists,
# this validation should scope to only those changed files.
if ($ChangesFile -and (Test-Path $ChangesFile -PathType leaf)) {
    Get-Content $ChangesFile | ForEach-Object {
        Write-Host "Checking file: $_"
        $packageName = $_ | Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*" | Select-Object -First 1

        if (-not $packageName) {
            return # this is not an MRTK package, so skip
        }

        $packageName = $packageName.Matches[0].Value
        
        $isChangelog = $_ -match "CHANGELOG.md"
        if ($changelogUpdated.ContainsKey($packageName)) {
            if ($isChangelog) {
                $changelogUpdated[$packageName] = $true
            }
        }
        else {
            $changelogUpdated[$packageName] = $isChangelog
        }
    }
}

$containsIssue = $false
$changelogUpdated.GetEnumerator() | ForEach-Object {
    if (-not $_.Value) {
        Write-Warning "Package '$($_.Key)' has changes, but its CHANGELOG.md was not updated. This is not always an issue but usually is"
        $containsIssue = $true
    }
}

if ($containsIssue) {
    Write-Output "Potential issues found, please see above for details"
    exit 1;
}
else {
    Write-Output "No issues found"
    exit 0;
}
