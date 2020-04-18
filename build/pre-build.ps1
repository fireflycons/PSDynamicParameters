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

Write-Host "Cloning documention site"

# Dot-source git helper
. (Join-Path $PSScriptRoot Invoke-Git.ps1)

# https://www.appveyor.com/docs/how-to/git-push/

$cloneFolder = Join-Path $env:APPVEYOR_BUILD_FOLDER '..'

Invoke-Git config --global credential.helper store
Add-Content "$HOME\.git-credentials" "https://${env:GITHUB_PAT}:x-oauth-basic@github.com`n"
Invoke-Git config --global user.email "${env:GITHUB_EMAIL}"
Invoke-Git config --global user.name "AppVeyor"
Push-Location $cloneFolder
Invoke-Git clone https://github.com/fireflycons/fireflycons.github.io.git
Pop-Location

