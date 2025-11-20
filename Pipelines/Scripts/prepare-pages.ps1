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
    $packageFriendlyName = (Select-String -Pattern "`"displayName`": `"(.+)`"" -Path $_ | Select-Object -First 1).Matches.Groups[1].Value
    $packagePath = $_.Directory

    # Create README, add front matter, and copy content
    New-Item -Path "./$packageName.md" -Value @"
---
title: $packageFriendlyName
parent: Packages
---


"@
    $readmeContent = (Get-Content -Path (Join-Path $packagePath "README.md"))
    $readmeContent = $readmeContent -replace "> \[!(\w+)\]", { "{: .$("$($_.Groups[1])".ToLower()) }" } # Convert GitHub admonitions to just-the-docs syntax
    Add-Content -Path "./$packageName.md" -Value $readmeContent

    # Create CHANGELOG, add front matter, and copy content
    New-Item -Path "./$packageName.CHANGELOG.md" -Value @"
---
title: Changelog
parent: $packageFriendlyName
---


"@
    Add-Content -Path "./$packageName.CHANGELOG.md" -Value (Get-Content -Path (Join-Path $packagePath "CHANGELOG.md"))
}
