#!/usr/bin/pwsh

##########################################################################
# This is the Cake bootstrapper script for PowerShell.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

<#

.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER Experimental
Tells Cake to use the latest Roslyn release.
.PARAMETER WhatIf
Performs a dry run of the build script.
No tasks will be executed.
.PARAMETER SkipToolPackageRestore
Skips restoring of packages.
.PARAMETER ForceCoreClr
Force use of Cake.CoreCLR on Windows.
.PARAMETER ScriptArgs
Remaining arguments are added here.

.LINK
http://cakebuild.net

#>

[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target = "Default",
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Normal",
    [switch]$Experimental,
    [Alias("DryRun", "Noop")]
    [switch]$WhatIf,
    [switch]$SkipToolPackageRestore,
    [switch]$ForceCoreClr,
    [Parameter(Position = 0, Mandatory = $false, ValueFromRemainingArguments = $true)]
    [string[]]$ScriptArgs
)

[Reflection.Assembly]::LoadWithPartialName("System.Security") | Out-Null
function MD5HashFile([string] $filePath)
{
    if ([string]::IsNullOrEmpty($filePath) -or !(Test-Path $filePath -PathType Leaf))
    {
        return $null
    }

    [System.IO.Stream] $file = $null;
    [System.Security.Cryptography.MD5] $md5 = $null;
    try
    {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $file = [System.IO.File]::OpenRead($filePath)
        return [System.BitConverter]::ToString($md5.ComputeHash($file))
    }
    finally
    {
        if ($null -ne $file)
        {
            $file.Dispose()
        }
    }
}

try
{
    Write-Host "Preparing to run build script..."

    Write-Host "Checking operating system..."
    $IsWindowsOS = (-not (Get-Variable -Name IsWindows -ErrorAction Ignore)) -or $IsWindows

    if ($IsWindowsOS)
    {
        Write-Host " - Windows"
    }
    elseif ($IsLinux)
    {
        Write-Host " - Linux"
        $ForceCoreClr = $true
    }
    elseif ($IsMacOS)
    {
        Write-Host "- MacOS"
        $ForceCoreClr = $true
    }
    else
    {
        Write-Host " - Unknown: Cannot continue!"
        exit 1
    }

    if (!$PSScriptRoot)
    {
        $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
    }

    $TOOLS_DIR = Join-Path $PSScriptRoot "tools"
    $PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
    $PACKAGES_CONFIG_MD5 = Join-Path $TOOLS_DIR "packages.config.md5sum"

    # Should we use the new Roslyn?
    $UseExperimental = [string]::Empty;
    if ($Experimental.IsPresent)
    {
        Write-Verbose -Message "Using experimental version of Roslyn."
        $UseExperimental = "-experimental"
    }

    # Is this a dry run?
    $UseDryRun = [string]::Empty;
    if ($WhatIf.IsPresent)
    {
        $UseDryRun = "-dryrun"
    }

    # Will we use dotnet to invoke Cake?
    $DotNet = [string]::Empty
    $DotNetExpression = [string]::Empty
    if ($ForceCoreClr)
    {
        try
        {
            $DotNet = (Get-Command dotnet -ErrorAction Stop).Source
            $DotNetExpression = "`"$DotNet`""
        }
        catch
        {
            throw "Unable to locate dotnet executable on this system!"
        }
    }

    # Make sure tools folder exists
    if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR))
    {
        Write-Verbose -Message "Creating tools directory..."
        New-Item -Path $TOOLS_DIR -Type directory | out-null
    }

    # Make sure that packages.config exist.
    if (!(Test-Path $PACKAGES_CONFIG))
    {
        Write-Verbose -Message "Downloading packages.config..."
        try
        {
            (New-Object System.Net.WebClient).DownloadFile("http://cakebuild.net/download/bootstrapper/packages", $PACKAGES_CONFIG)
        }
        catch
        {
            Throw "Could not download packages.config."
        }
    }

    if ($IsWindowsOS -and -not $ForceCoreClr)
    {
        # Windows - Acquire Nuget and use to download Cake (.NET Framework)

        $NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
        $CAKE = Join-Path $TOOLS_DIR "Cake/Cake.exe"
        $NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

        # Try find NuGet.exe in path if not exists
        if (!(Test-Path $NUGET_EXE))
        {
            Write-Verbose -Message "Trying to find nuget.exe in PATH..."
            $existingPaths = $Env:Path -Split ';' |
            Where-Object {

                if ([string]::IsNullOrEmpty($_))
                {
                    $false
                }

                try
                {
                    # Some paths may throw Access Denied exceptions which write a message to STDERR, causing Bamboo to fail the build
                    Test-Path $_ -PathType Container
                }
                catch
                {
                    $false
                }
            }

            $NUGET_EXE_IN_PATH = Get-ChildItem -Path $existingPaths -Filter "nuget.exe" | Select-Object -First 1
            if ($null -ne $NUGET_EXE_IN_PATH -and (Test-Path $NUGET_EXE_IN_PATH.FullName))
            {
                Write-Verbose -Message "Found in PATH at $($NUGET_EXE_IN_PATH.FullName)."
                $NUGET_EXE = $NUGET_EXE_IN_PATH.FullName
            }
        }

        # Try download NuGet.exe if not exists
        if (!(Test-Path $NUGET_EXE))
        {
            Write-Verbose -Message "Downloading NuGet.exe..."
            try
            {
                (New-Object System.Net.WebClient).DownloadFile($NUGET_URL, $NUGET_EXE)
            }
            catch
            {
                Throw "Could not download NuGet.exe."
            }
        }

        # Save nuget.exe path to environment to be available to child processed
        $ENV:NUGET_EXE = $NUGET_EXE

        # Restore tools from NuGet?
        if (-Not $SkipToolPackageRestore.IsPresent)
        {
            Push-Location
            Set-Location $TOOLS_DIR

            # Check for changes in packages.config and remove installed tools if true.
            [string] $md5Hash = MD5HashFile($PACKAGES_CONFIG)
            if ((!(Test-Path $PACKAGES_CONFIG_MD5)) -Or
                ($md5Hash -ne (Get-Content $PACKAGES_CONFIG_MD5 )))
            {
                Write-Verbose -Message "Missing or changed package.config hash..."
                Remove-Item * -Recurse -Exclude packages.config, nuget.exe
            }

            Write-Verbose -Message "Restoring tools from NuGet..."
            $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$TOOLS_DIR`""

            if ($LASTEXITCODE -ne 0)
            {
                Throw "An error occured while restoring NuGet tools."
            }
            else
            {
                $md5Hash | Out-File $PACKAGES_CONFIG_MD5 -Encoding "ASCII"
            }
            Write-Verbose -Message ($NuGetOutput | out-string)
            Pop-Location
        }

        # Make sure that Cake has been installed.
        if (!(Test-Path $CAKE))
        {
            Throw "Could not find Cake.exe at $CAKE"
        }
    }
    else
    {
        # Linux/CoreCLR
        # Use dotnet restore to get Cake CoreCLR since we do not have NuGet.exe
        # Now transform packages.config to a csproj file so that dotnet restore accepts it.

        $xsl = @'
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:template match="/">
        <Project Sdk="Microsoft.NET.Sdk">
            <PropertyGroup>
                <TargetFramework>netstandard2.0</TargetFramework>
            </PropertyGroup>
            <ItemGroup>
                <xsl:for-each select="packages/package">
                    <PackageReference>
                        <xsl:attribute name="Include">
                            <xsl:choose>
                                <xsl:when test="@id = 'Cake'">Cake.CoreCLR</xsl:when>
                                <xsl:otherwise><xsl:value-of select="@id" /></xsl:otherwise>
                            </xsl:choose>
                        </xsl:attribute>
                        <xsl:attribute name="Version">
                            <xsl:value-of select="@version" />
                        </xsl:attribute>
                    </PackageReference>
                </xsl:for-each>
            </ItemGroup>
        </Project>
    </xsl:template>
</xsl:stylesheet>
'@
        $cakeCsproj = Join-Path $PSScriptRoot "cake-package-$([Guid]::NewGuid()).csproj"
        try
        {
            # XSLT transform packages.config to a .csproj file
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($xsl)
            $stream = New-Object System.IO.MemoryStream (, $bytes)
            $reader = [System.Xml.XmlReader]::Create($stream)

            $xsltSettings = New-Object System.Xml.Xsl.XsltSettings;
            $xmlUrlResolver = New-Object System.Xml.xmlUrlResolver;
            $xsltSettings.EnableScript = 1;

            $xslt = New-Object System.Xml.Xsl.XslCompiledTransform;
            $xslt.Load($reader, $xsltSettings, $xmlUrlResolver);
            $xslt.Transform($PACKAGES_CONFIG, $cakeCsproj);

            # Run dotnet restore
            & $DotNet restore --packages $TOOLS_DIR $cakeCsproj

            # Now locate cake.dll
            $CAKE = Get-ChildItem -Path $TOOLS_DIR -File -Filter Cake.dll -Recurse |
            Select-Object -First 1 |
            Select-Object -ExpandProperty FullName

            if (-not $CAKE)
            {
                throw "Could not find Cake.dll (Cake CoreCLR application)"
            }
        }
        catch
        {
            $errorMessage = $_.Exception.Message
            $failedItem = $_.Exception.ItemName
            Write-Host  'Error'$errorMessage':'$failedItem':' $_.Exception;
            exit 1
        }
        finally
        {
            $reader, $stream |
            Where-Object {
                $null -ne $_
            } |
            Foreach-Object {
                $_.Dispose()
            }

            if (Test-Path -Path $cakeCsproj -PathType Leaf)
            {
                Remove-Item $cakeCsproj
            }
        }
    }

    Invoke-Command -NoNewScope {
        # Check for a prebuild script and include if present.
        Get-ChildItem -Path $PSScriptRoot -Filter *.cake |
        Where-Object {
            $_.Name -ieq 'prebuild.cake'
        } |
        Select-Object -First 1 |
        Select-Object -ExpandProperty FullName

        $Script
    } |
    Foreach-Object {

        $scriptToExecute = $_

        if (-not ($scriptToExecute.Contains('/') -or $scriptToExecute.Contains('\') -or (Test-Path -Path $scriptToExecute -PathType Leaf)))
        {
            # Assume build script is in same folder as this file
            $scriptToExecute = Join-Path $PSScriptRoot $scriptToExecute
        }

        Write-Host "`nExecuting $(Split-Path -Leaf $scriptToExecute) ..."

        # First load any Cake modules speficied by #module
        Invoke-Expression "& $DotNetExpression `"$CAKE`"  `"$scriptToExecute`" --bootstrap"

        # Now execute script
        Invoke-Expression "& $DotNetExpression `"$CAKE`" `"$scriptToExecute`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" $UseMono $UseDryRun $UseExperimental $ScriptArgs"

        if ($LASTEXITCODE -ne 0)
        {
            throw "$(Split-Path -Leaf $scriptToExecute) - Execution failed."
        }
    }
}
catch
{
    Write-Host "Exception Thrown: $($_.Exception.Message)"
    Write-Host $_.ScriptStackTrace
    exit 1
}
finally
{
    Write-Host
    Write-Host "Cake downloaded $([Math]::Round((Get-ChildItem (Join-Path $PSSCriptRoot tools) -File -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB, 2)) MB of tools and addins"
    Write-Host
}