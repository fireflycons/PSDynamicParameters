namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    public class ValidateNotNullTests
    {
        [Fact]
        public void
            Test_WhenParameterValueIsNull_AndValidateNotNullAttributeIsPresent_ThenParameterBindingExceptionIsThrown()
        {
            var expectedMessage =
                $"Cannot validate argument on parameter '{Constants.DynamicParameterName}'. The argument is null. Provide a valid value for the argument, and then try running the command again.";
            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateNotNull, null);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void
            Test_WhenParameterValueIsNull_AndValidateNotNullOrEmptyAttributeIsPresent_ThenParameterBindingExceptionIsThrown()
        {
            var expectedMessage =
                $"Cannot validate argument on parameter '{Constants.DynamicParameterName}'. The argument is null or empty. Provide an argument that is not null or empty, and then try the command again.";
            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateNotNullOrEmpty, null);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }
    }
}