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
$docs = Join-Path $ProjectRoot "docs"

# Copy root README images to docs
Copy-Item -Path (Join-Path $ProjectRoot "images" "*") -Destination (Join-Path $docs "images") -Recurse

$indexDestination = Join-Path $docs "index.md"
# Create home page, add front matter, and copy content
New-Item -Path $indexDestination -Value @"
---
title: Home
---


"@
Add-Content -Path $indexDestination -Value (Get-Content -Path (Join-Path $ProjectRoot "README.md"))

# Loop through package directories and copy documentation
Get-ChildItem -Path (Join-Path $ProjectRoot "*" "package.json") | ForEach-Object {
    $packageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*" -Path $_ | Select-Object -First 1

    if (-not $packageName) {
        return # this is not an MRTK package, so skip
    }

    $packageName = $packageName.Matches[0].Value
    $packageFriendlyName = (Select-String -Pattern "`"displayName`": `"(.+)`"" -Path $_ | Select-Object -First 1).Matches.Groups[1].Value
    $packagePath = $_.Directory

    # Create README, add front matter, and copy content
    $packageReadmeDestination = Join-Path $docs "$packageName.md"
    New-Item -Path $packageReadmeDestination -Value @"
---
title: $packageFriendlyName
parent: Packages
---


"@
    $readmeContent = (Get-Content -Path (Join-Path $packagePath "README.md"))
    $readmeContent = $readmeContent -replace "> \[!(\w+)\]", { "{: .$("$($_.Groups[1])".ToLower()) }" } # Convert GitHub admonitions to just-the-docs syntax
    Add-Content -Path $packageReadmeDestination -Value $readmeContent

    # Create CHANGELOG, add front matter, and copy content
    $packageChangelogDestination = Join-Path $docs "$packageName.CHANGELOG.md"
    New-Item -Path $packageChangelogDestination -Value @"
---
title: Changelog
parent: $packageFriendlyName
---


"@
    Add-Content -Path $packageChangelogDestination -Value (Get-Content -Path (Join-Path $packagePath "CHANGELOG.md"))
}
