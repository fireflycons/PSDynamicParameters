namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.Linq;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using Microsoft.Win32;

    /// <summary>
    /// Hosts a PowerShell run space in which to run <see cref="ShowDynamicParameterCommand"/> with various tests.
    /// </summary>
    internal static class TestCmdletHost
    {
        /// <summary>
        /// Script for non pipeline invocations.
        /// </summary>
        private const string ValueAsArgumentScript = @"

            param
            (
                $TestValue
            )

            Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
            Import-Module '{0}'

            if ($null -ne $TestValue)
            {{
                Show-DynamicParameter -TestNumber {1} -TestParameter $TestValue
            }}
            else
            {{
                Show-DynamicParameter -TestNumber {1}
            }}
        ";

        /// <summary>
        /// Script for non pipeline invocations.
        /// </summary>
        private const string ValueInPipelineScript = @"

            param
            (
                $TestValue
            )

            Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
            Import-Module '{0}'
            $TestValue | Show-DynamicParameter -TestNumber {1}
        ";

        /// <summary>
        /// Script for positional parameter.
        /// </summary>
        private const string PositionalArgumentScript = @"

            param
            (
                $TestValue
            )

            Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
            Import-Module '{0}'
            Show-DynamicParameter -TestNumber {1} $TestValue
        ";

        /// <summary>
        /// Script for positional parameter.
        /// </summary>
        private const string AliasArgumentScript = @"

            param
            (
                $TestValue
            )

            Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
            Import-Module '{0}'
            Show-DynamicParameter -TestNumber {1} -{2} $TestValue
        ";

        /// <summary>
        /// Script for positional parameter.
        /// </summary>
        private const string ExplicitNullScript = @"

            param
            (
                $TestValue
            )

            Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
            Import-Module '{0}'
            Show-DynamicParameter -TestNumber {1} -TestParameter $null
        ";

        /// <summary>
        /// Script for parameter set tests.
        /// </summary>
        private const string ParameterSetScript = @"

            Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
            Import-Module '{0}'
            Show-DynamicParameter -TestNumber {1} {2} {3}
        ";

        /// <summary>
        /// Path to PSD1 file describing the <see cref="ShowDynamicParameterCommand"/> cmdlet
        /// </summary>
        private static readonly string CmdletModulePath = typeof(ShowDynamicParameterCommand).Assembly.Location;

        /// <summary>
        /// Runs the <see cref="ShowDynamicParameterCommand"/> with arguments for test and collects results
        /// </summary>
        /// <param name="testNumber">Test to run</param>
        /// <param name="dynamicParameterValue">Value to pass to the dynamic parameter. If <c>null</c>, then omit parameter. If it is the type <see cref="DBNull"/>, pass <c>null</c> explicitly as the value</param>
        /// <param name="alias">Name of parameter alias for alias tests.</param>
        /// <returns>Pipeline output</returns>
        public static Collection<PSObject> RunTestHost(
            TestCases testNumber,
            object dynamicParameterValue,
            string alias = null)
        {
            Collection<PSObject> result;

            using (var powershell = PowerShell.Create())
            {
                string testScript;

                switch (testNumber)
                {
                    case TestCases.ValueFromPipeline:
                    case TestCases.ValueFromPipelineByPropertyName:
                    case TestCases.ValidatePatternFromPipeline:
                    case TestCases.ValidateSetFromPipeline:
                    case TestCases.ValidateSetFromPipelineByPropertyName:

                        testScript = string.Format(ValueInPipelineScript, CmdletModulePath, (int)testNumber);
                        break;

                    case TestCases.AliasArgument:

                        testScript = string.Format(AliasArgumentScript, CmdletModulePath, (int)testNumber, alias);
                        break;

                    case TestCases.PositionalArgument:

                        testScript = string.Format(PositionalArgumentScript, CmdletModulePath, (int)testNumber);
                        break;

                    case TestCases.MandatoryWithAllowNull:
                    case TestCases.ValidateNotNull:
                    case TestCases.ValidateNotNullOrEmpty:

                        testScript = string.Format(ExplicitNullScript, CmdletModulePath, (int)testNumber);
                        break;

                    case TestCases.ParameterSetsFirstParameterInSetASecondParameterNotInSet:
                    case TestCases.ParameterSetInvalidCombination:

                        testScript = string.Format(
                            ParameterSetScript,
                            CmdletModulePath,
                            (int)testNumber,
                            $"-{Constants.DynamicParameterSetFirstParameter} A",
                            $"-{Constants.DynamicParameterSetSecondParameter} B");
                        break;

                    default:

                        testScript = string.Format(ValueAsArgumentScript, CmdletModulePath, (int)testNumber);
                        break;
                }

                powershell.AddScript(testScript);

                if (testNumber != TestCases.MandatoryWithAllowNull)
                {
                    powershell.AddParameter("TestValue", dynamicParameterValue);
                }

                // All objects emitted to pipeline by executing PowerShell code are collected
                result = powershell.Invoke();

                if (powershell.HadErrors)
                {
                    // Get first exception from script, if any
                    var errorRecord = powershell.Streams.Error.FirstOrDefault();

                    if (errorRecord != null)
                    {
                        throw errorRecord.Exception;
                    }
                    
                    if (powershell.InvocationStateInfo.Reason != null)
                    {
                        throw powershell.InvocationStateInfo.Reason;
                    }
                    
                    throw new Exception("Unknown exception in PowerShell host");
                }
            }

            return result;
        }
    }
}