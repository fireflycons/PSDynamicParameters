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

Write-Host "Cloning documantion site"

$script:git = Get-Command -Name Git

function Invoke-Git
{
    param
    (
        [Parameter(ValueFromRemainingArguments)]
        [string[]]$GitArgs
    )

    & $git $GitArgs

    if ($LASTEXITCODE -ne 0)
    {
        throw "GIT finished with exit code $LASTEXITCODE"
    }
}

# https://www.appveyor.com/docs/how-to/git-push/

$cloneFolder = Join-Path $env:APPVEYOR_BUILD_FOLDER '..'

Invoke-Git config --global credential.helper store
Add-Content "$HOME\.git-credentials" "https://${env:GITHUB_PAT}:x-oauth-basic@github.com`n"
Invoke-Git config --global user.email "${env:GITHUB_EMAIL}"
Invoke-Git config --global user.name "AppVeyor"
Push-Location $cloneFolder
Invoke-Git clone https://github.com/fireflycons/fireflycons.github.io.git
Pop-Location

