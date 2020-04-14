namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests <see cref="ValidateScriptAttribute"/>
    /// <seealso cref="Constants.ValidateScript"/>
    /// </summary>
    public class ValidateScriptTests
    {
        /// <summary>
        /// Tests that when value is out of range then parameter binding exception is thrown.
        /// </summary>
        /// <param name="value">The value.</param>
        [Theory]
        [InlineData(3)]
        [InlineData(9)]
        public void Test_WhenValueIsOutOfRangeCheckedByScript_ThenParameterBindingExceptionIsThrown(int value)
        {
            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateScript, value);

            action.Should().Throw<ParameterBindingException>();
        }

        /// <summary>
        /// Tests that when value is within range it is returned.
        /// </summary>
        /// <param name="value">The value.</param>
        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void Test_WhenValueIsWithinRangeCheckedByScript_ItIsReturned(int value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.ValidateScript, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.Should().Be(value);
        }
    }
}