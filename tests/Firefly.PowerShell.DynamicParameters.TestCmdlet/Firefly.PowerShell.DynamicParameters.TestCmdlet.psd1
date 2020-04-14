@{
    # Script module or binary module file associated with this manifest.
    RootModule        = 'Firefly.PowerShell.DynamicParameters.TestCmdlet.psm1'

    # Version number of this module.
    ModuleVersion     = '0.3.2'

    # Supported PSEditions
    # CompatiblePSEditions = @()

    # ID used to uniquely identify this module
    GUID              = 'a38976be-ce69-48d4-8e23-20164bd7304b'

    # Author of this module
    Author            = 'Alistair Mackay'

    # Company or vendor of this module
    CompanyName       = 'Firefly Consulting Ltd.'

    # Copyright statement for this module
    Copyright         = '© 2020 Alistair Mackay. All rights reserved.'

    # Description of the functionality provided by this module
    Description       = 'Cmdlet for running unit tests against'

    # Minimum version of the Windows PowerShell engine required by this module
    PowerShellVersion = '5.1'

    # Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
    NestedModules     = @('Firefly.PowerShell.DynamicParameters.TestCmdlet.dll')

    # Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
    FunctionsToExport = @()

    # Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
    CmdletsToExport   = 'Show-DynamicParameter'

    # Variables to export from this module
    VariablesToExport = @()

    # Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
    AliasesToExport   = @()
<#
    # Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
    PrivateData       = @{

        PSData = @{

            # Tags applied to this module. These help with module discovery in online galleries.
            Tags       = @('SQL', 'MSSQL', 'SQLServer')

            # A URL to the license for this module.
            LicenseUri = 'https://github.com/fireflycons/Invoke-SqlExecute/blob/master/LICENSE'

            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/fireflycons/Invoke-SqlExecute'

        } # End of PSData hashtable

    } # End of PrivateData hashtable
#>
}

