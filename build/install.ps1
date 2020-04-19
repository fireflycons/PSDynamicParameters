# Dot-source vars describing environment
. (Join-Path $PSScriptRoot build-environment.ps1)

if ($PSEdition -eq 'Core' -and $IsLinux)
{
    & dotnet-core-uninstall --all-previews
}

$cinst = Get-Command -Name cinst -ErrorAction SilentlyContinue
if (-not $cinst)
{
    Write-Host "Chocolatey not present on this platform. DocFX install skipped"
    return
}

& $cinst docfx --yes --limit-output |
Foreach-Object {
    if ($_ -inotlike 'Progress*Saving*')
    {
        Write-Host $_
    }
}
exit $LASTEXITCODE
