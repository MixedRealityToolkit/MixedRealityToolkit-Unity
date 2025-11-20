# Copyright (c) Mixed Reality Toolkit Contributors
# Licensed under the BSD 3-Clause

<#
.PARAMETER ProjectRoot
    The root folder of the project.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectRoot
)

$ProjectRoot = Resolve-Path -Path $ProjectRoot

# Loop through package directories and copy documentation
Get-ChildItem -Path (Join-Path $ProjectRoot "*" "package.json") | ForEach-Object {
    $packageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*" -Path $_ | Select-Object -First 1

    if (-not $packageName) {
        return # this is not an MRTK package, so skip
    }

    $packageName = $packageName.Matches[0].Value
    $packagePath = $_.Directory

    Copy-Item -Path (Join-Path $packagePath "README.md") -Destination "./$packageName.md"
    Copy-Item -Path (Join-Path $packagePath "CHANGELOG.md") -Destination "./$packageName.CHANGELOG.md"
}
