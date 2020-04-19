namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    public class MandatoryParameterTests
    {
        /// <summary>
        /// Tests the when mandatory value not supplied then parameter binding exception is thrown.
        /// </summary>
        [SkippableFact]
        public void Test_WhenMandatoryValueNotSupplied_ThenParameterBindingExceptionIsThrown()
        {
            Skip.IfNot(Constants.IsWindows);

            var expectedMessage =
                "Cannot process command because of one or more missing mandatory parameters: TestParameter.";

            Action action = () => TestCmdletHost.RunTestHost(
                TestCases.MandatoryArgument,
                null);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }

        /// <summary>
        /// Tests the when mandatory value not supplied then parameter binding exception is thrown.
        /// </summary>
        [Fact]
        public void Test_WhenMandatoryValueIsExplicitlyNullAndAllowNullAttributeIsSet_ThenNoExceptionIsThrown()
        {
            Action action = () => TestCmdletHost.RunTestHost(TestCases.MandatoryWithAllowNull, null);

            action.Should().NotThrow();
        }

    }
}