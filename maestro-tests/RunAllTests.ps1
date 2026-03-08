# PowerShell script to run all .yaml test files in the current directory using Maestro
# Command: maestro test .\filename.yaml -p android

$yamlFiles = Get-ChildItem -Path . -Filter *.yaml
$successCount = 0
$totalCount = $yamlFiles.Count
$failures = @()

foreach ($file in $yamlFiles) 
{
    Write-Host "Running test for $($file.Name)..."
    $output = & maestro test .\$($file.Name) -p android 2>&1
    if ($LASTEXITCODE -eq 0) 
    {
        Write-Host "$($file.Name) succeeded"
        $successCount++
    } else 
    {
        $errorMessage = $output -join "`n"  # Join output lines if multiple
        Write-Host "$($file.Name) failed: $errorMessage"
        $failures += @{File = $file.Name; Reason = $errorMessage}
    }
}

Write-Host "`nSummary: $successCount/$totalCount tests succeeded"

if ($failures.Count -gt 0) 
{
    Write-Host "Failures:"
    foreach ($f in $failures) {
        Write-Host "$($f.File): $($f.Reason)"
    }
}