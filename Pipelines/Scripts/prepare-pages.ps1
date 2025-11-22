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
nav_order: 1
---


"@
Add-Content -Path $indexDestination -Value (Get-Content -Path (Join-Path $ProjectRoot "README.md"))

# Convert GitHub admonitions to just-the-docs syntax for in-place docs files
# We leave them checked-in so they render correctly in the GitHub repo, but we convert them for Pages
Get-ChildItem -Path (Join-Path $docs "*" "*.md") -Recurse | ForEach-Object {
    $fileContent = Get-Content -Path $_
    Set-Content -Path $_ -Value ($fileContent -replace "> \[!(\w+)\]", { "{: .$("$($_.Groups[1])".ToLower()) }" })
}

# Loop through package directories and copy documentation
Get-ChildItem -Path (Join-Path $ProjectRoot "*" "package.json") | ForEach-Object {
    $packageName = Select-String -Pattern "org\.mixedrealitytoolkit\.\w+(\.\w+)*" -Path $_ | Select-Object -First 1

    if (-not $packageName) {
        return # this is not an MRTK package, so skip
    }

    $packageName = $packageName.Matches[0].Value
    $packageFriendlyName = (Select-String -Pattern "`"displayName`": `"(.+)`"" -Path $_ | Select-Object -First 1).Matches.Groups[1].Value
    $packagePath = $_.DirectoryName
    $packageDocsPath = Join-Path $docs $packageName

    New-Item -Path $packageDocsPath -ItemType Directory

    # Create README, add front matter, and copy content
    $packageReadmeDestination = Join-Path $packageDocsPath "index.md"
    New-Item -Path $packageReadmeDestination -ItemType File -Value @"
---
title: $packageFriendlyName
parent: Packages
---


"@
    $readmeContent = Get-Content -Path (Join-Path $packagePath "README.md")
    # Convert GitHub admonitions to just-the-docs syntax
    Add-Content -Path $packageReadmeDestination -Value ($readmeContent -replace "> \[!(\w+)\]", { "{: .$("$($_.Groups[1])".ToLower()) }" })

    # Create CHANGELOG, add front matter, and copy content
    $packageChangelogDestination = Join-Path $packageDocsPath "CHANGELOG.md"
    New-Item -Path $packageChangelogDestination -ItemType File -Value @"
---
title: Changelog
parent: $packageFriendlyName
---


"@
    Add-Content -Path $packageChangelogDestination -Value (Get-Content -Path (Join-Path $packagePath "CHANGELOG.md"))

    Get-ChildItem -Path (Get-ChildItem -Path $packagePath -Directory) -Recurse -File -Include "*.md" | ForEach-Object {
        if ($_.BaseName -in @("LICENSE", "CHANGELOG")) {
            return # skip specific files that we don't need to publish through this path
        }

        # Create file, add front matter, and copy content
        # Remove the ~ from the Documentation~ folder name
        $fileFolder = (Join-Path $packageDocsPath ($_.DirectoryName | Split-Path -Leaf)).Replace('~', '')
        New-Item -Path $fileFolder -ItemType Directory
        $fileDestination = Join-Path $fileFolder $_.Name
        $fileTitle = Select-String -Pattern "# (.+)" -Path $_ | Select-Object -First 1
        New-Item -Path $fileDestination -ItemType File -Value @"
---
title: $($fileTitle.Matches ? $fileTitle.Matches[0].Groups[1] : $_.BaseName)
parent: $packageFriendlyName
---


"@
        $fileContent = Get-Content -Path $_
        # Convert GitHub admonitions to just-the-docs syntax
        Add-Content -Path $fileDestination -Value ($fileContent -replace "> \[!(\w+)\]", { "{: .$("$($_.Groups[1])".ToLower()) }" })
    }
}
