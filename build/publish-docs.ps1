if ($PSEdition -eq 'Core')
{
    Write-Host "Publish docs step not in scope in this environment"
    return
}

$isAppVeyor = $null -ne (Get-ChildItem env: | Where-Object { $_.Name -ilike 'APPVEYOR*' } | Select-Object -First 1)

# TODO - Only if master and repo_tag
if (-not $isAppVeyor)
{
    Write-Host "Publish docs step not in scope in this environment"
    return
}

# Dot-source git helper
. (Join-Path $PSScriptRoot Invoke-Git.ps1)

Push-Location (Join-Path $env:APPVEYOR_BUILD_FOLDER "../fireflycons.github.io")

try
{
    # Stage any changes
    Invoke-Git -SuppressWarnings add --all

    # Check status
    $stat = Invoke-Git -OutputToPipeline status --short

    if ($null -eq $stat)
    {
        Write-Host "No changes to documentation detected"
        return
    }

    $stat |
    ForEach-Object {
        Write-Host $_
    }

    # Commit
    Invoke-Git commit -m "AppVeyor Build ${env:APPVEYOR_BUILD_NUMBER}"

    # and push
    Invoke-Git push

    Write-Host
    Write-Host "Documentation changes pushed."
}
finally
{
    Pop-Location
}