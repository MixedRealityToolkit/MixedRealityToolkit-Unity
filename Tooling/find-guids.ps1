param (
    [string[]]$guids
)

Write-Host "Checking:"
$guids | ForEach-Object {
    Write-Host $_
}

$regex = "("
$guids | ForEach-Object {
    $regex += "$_|"
}
$regex = $regex.TrimEnd("|") + ")"

$count = 0

Get-ChildItem . -File -Recurse -Include *.unity, *.prefab, *.asset, *.mat, *.meta | ForEach-Object {
    if ($_ | Select-String -Pattern $regex) {
        Write-Host "`nFound the following GUIDs in ${_}:"

        $file = $_
        $guids | ForEach-Object {
            if ($file | Select-String $_) {
                Write-Host $_
            }
        }

        $count++
    }
}

if ($count -eq 1) {
    Write-Host "`nFound GUIDs in $count file"
} else {
    Write-Host "`nFound GUIDs in $count files"
}

if ($count -gt 0) {
    exit 1
} else {
    exit 0
}