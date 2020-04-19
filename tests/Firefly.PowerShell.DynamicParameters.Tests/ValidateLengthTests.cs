namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests <see cref="ValidateLengthAttribute"/>
    /// </summary>
    public class ValidateLengthTests
    {
        /// <summary>
        /// Tests the when string length is outside length validation then parameter binding exception is thrown.
        /// <seealso cref="Constants.ValidLengths"/>
        /// </summary>
        /// <param name="stringLength">Length of the string.</param>
        [SkippableTheory]
        [InlineData(2)]
        [InlineData(5)]
        public void Test_WhenStringLengthIsOutsideLengthValidation_ThenParameterBindingExceptionIsThrown(
            int stringLength)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            var testString = string.Empty.PadLeft(stringLength);

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateLength, testString);

            action.Should().Throw<ParameterBindingException>();
        }

        /// <summary>
        /// Tests the when string length is within length validation then no exception is thrown.
        /// <seealso cref="Constants.ValidLengths"/>
        /// </summary>
        /// <param name="stringLength">Length of the string.</param>
        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void Test_WhenStringLengthIsWithinLengthValidation_ThenNoExceptionIsThrown(int stringLength)
        {
            var testString = string.Empty.PadLeft(stringLength);

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateLength, testString);

            action.Should().NotThrow();
        }
    }
}