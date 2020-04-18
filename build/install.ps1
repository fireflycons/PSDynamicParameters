$cinst = Get-Command -Name cinst -ErrorAction SilentlyContinue
if (-not $cinst)
{
    Write-Host "Chocolatey not present on this platform. DocFX install skipped"
    return
}

& $cinst docfx --yes --limit-output
exit $LASTEXITCODE
