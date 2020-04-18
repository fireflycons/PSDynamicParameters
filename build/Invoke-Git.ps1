$script:git = Get-Command -Name Git

function Invoke-Git
{
    param
    (
        [switch]$OutputToPipeline,

        [Parameter(ValueFromRemainingArguments)]
        [string[]]$GitArgs
    )

    $backupErrorActionPreference = $script:ErrorActionPreference
    $script:ErrorActionPreference = "Continue"

    try
    {
        Write-Host -ForegroundColor Cyan ("git " + ($GitArgs -join ' ').Replace($env:GITHUB_PAT, "*****")).Replace($env:GITHUB_EMAIL, "*****")

        & $git $GitArgs 2>&1 |
        ForEach-Object {
            if ($_ -is [System.Management.Automation.ErrorRecord])
            {
                Write-Host $_.ErrorDetails.Message
            }
            else
            {
                if (-not ([string]::IsNullOrEmpty($_)))
                {
                    if ($OutputToPipeline)
                    {
                        $_
                    }
                    else
                    {
                        Write-Host $_
                    }
                }
            }
        }

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

