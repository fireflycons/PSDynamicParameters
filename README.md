# Firefly.PowerShell.DynamicParameters

A library for the creation of runtime [dynamic parameters](https://docs.microsoft.com/en-gb/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters) for binary cmdlets.

When creating a binary cmdlet, there are two ways that you can declare dynamic parameters:

1. When you know all the parameters you'll ever need at compile time, you can declare each parameter type in its own class. An example of this can be found in [this PowerShell Magazine article](https://www.powershellmagazine.com/2014/06/23/dynamic-parameters-in-c-cmdlets/)
1. When you don't necessarily know what parameters you'll need and you will create them on the fly in response to the values passed to the cmdlet's fixed arguments, then we need to create a [RuntimeDefinedParameterDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.runtimedefinedparameterdictionary).

This library handles the second case above. By implementing a builder pattern to create new dynamic paramaters, this makes it easy to manage the myriad attributes that may be applied to a cmdlet parameter.

## Supported PowerShell Versions

The nuget package contains the following builds

* .NET Framework 4.0 - Windows PowerShell 5.1
* .NET Core 2.1 - PowerShell Core 6 (Windows and Linux)
* .NET Core 3.1 - PowerShell Core 7 (Windows and Linux)

## Library Documentation

API documentation and example usage can be found [here](https://fireflycons.github.io/PSDynamicParameters).