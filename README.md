# Firefly.PowerShell.DynamicParameters

A library for the creation of runtime [dynamic parameters](https://docs.microsoft.com/en-gb/powershell/module/microsoft.powershell.core/about/about_functions_advanced_parameters) for binary cmdlets.

When creating a binary cmdlet, there are two ways that you can declare dynamic parameters:

1. When you know all the parameters you'll ever need at compile time, you can declare each parameter type in its own class. An example of this can be found in [this PowerShell Magazine article](https://www.powershellmagazine.com/2014/06/23/dynamic-parameters-in-c-cmdlets/)
1. When you don't necessarily know what parameters you'll need and you will create them on the fly in response to the values passed to the cmdlet's fixed arguments, then we need to create a [RuntimeDefinedParameterDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.runtimedefinedparameterdictionary).

This library handles the second case above. By implementing a builder pattern to create new dynamic paramaters, this makes it easy to manage the myriad attributes that may be applied to a cmdlet parameter.

## Supported PowerShell Versions

The nuget package contains the following builds

* .NET 4.0 - Windows PowerShell 5.1
* NETCORE 2.1 - PowerShell Core 6
* NETCORE 3.1 - PowerShell Core 7

Currently this can only be verified with tests on Windows due to [this issue](https://github.com/PowerShell/PowerShell/issues/12383) I found while running the tests on Ubuntu. However, the test cmdlet does execute and produce results and thus hand-rolled binary cmdlets built with this library _should_ still work on Linux. It is only the collection of exceptions from the embedded PowerShell host that doesn't work.

As a result, only tests that are not expected to throw exceptions are executed on the Linux build agent.

## Library Documentation

API documentation and example usage can be found [here](https://fireflycons.github.io/PSDynamicParameters).