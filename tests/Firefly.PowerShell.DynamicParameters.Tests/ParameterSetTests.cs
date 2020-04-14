namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests parameter sets
    /// </summary>
    public class ParameterSetTests
    {
        /// <summary>
        /// Tests the when first parameter is in set A and second parameter in all sets then set A is returned.
        /// </summary>
        [Fact]
        public void Test_WhenFirstParameterIsInSetA_AndSecondParameterInAllSets_ThenSetAIsReturned()
        {
            var result = TestCmdletHost.RunTestHost(
                TestCases.ParameterSetsFirstParameterInSetASecondParameterNotInSet,
                null);

            result.First().BaseObject.Should().Be(Constants.DynamicParameterSetsSetA);
        }

        /// <summary>
        /// Tests the when first parameter is in set A and second parameter in set B and both parameters present then parameter binding exception is thrown.
        /// </summary>
        [Fact]
        public void Test_WhenFirstParameterIsInSetA_AndSecondParameterInSetB_AndBothParametersPresent_ThenParameterBindingExceptionIsThrown()
        {
#if NETCOREAPP
            const string ExpectedMessage = "Parameter set cannot be resolved using the specified named parameters. One or more parameters issued cannot be used together or an insufficient number of parameters were provided.";
#else
            const string ExpectedMessage = "Parameter set cannot be resolved using the specified named parameters.";
#endif
            Action action = () => TestCmdletHost.RunTestHost(
                TestCases.ParameterSetInvalidCombination,
                null);

            action.Should().Throw<ParameterBindingException>().WithMessage(ExpectedMessage);
        }
    }
}