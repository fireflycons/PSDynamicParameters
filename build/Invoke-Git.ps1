$script:git = Get-Command -Name Git

function Invoke-Git
{
    param
    (
        [Parameter(ValueFromRemainingArguments)]
        [string[]]$GitArgs
    )

    $backupErrorActionPreference = $script:ErrorActionPreference
    $script:ErrorActionPreference = "Continue"

    try
    {
        & $git $GitArgs 2>&1
        $exitcode = $LASTEXITCODE
    }
    finally
    {
        $script:ErrorActionPreference = $backupErrorActionPreference
    }
    if ($exitcode -ne 0)
    {
        throw "GIT finished with exit code $exitcode"
    }
}
