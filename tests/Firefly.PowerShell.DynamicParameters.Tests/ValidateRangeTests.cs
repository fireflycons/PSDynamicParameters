namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests the <see cref="ValidateRangeAttribute"/>
    /// </summary>
    public class ValidateRangeTests
    {
        /// <summary>
        /// Tests that when value is out of range then parameter binding exception is thrown.
        /// </summary>
        /// <param name="value">The value.</param>
        [SkippableTheory]
        [InlineData(3)]
        [InlineData(9)]
        public void Test_WhenValueIsOutOfRange_ThenParameterBindingExceptionIsThrown(int value)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateRangeWithMinMax, value);

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
        public void Test_WhenValueIsWithinRange_ItIsReturned(int value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.ValidateRangeWithMinMax, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.Should().Be(value);
        }

#if NETCOREAPP

        /// <summary>
        /// Tests that when value is within range it is returned.
        /// </summary>
        /// <param name="value">The value.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Test_WhenValueIsWithinRangeKind_ItIsReturned(int value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.ValidateRangeWithRangeKindNonNegative, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.Should().Be(value);
        }

        /// <summary>
        /// Tests that when value is out of range then parameter binding exception is thrown.
        /// </summary>
        /// <param name="value">The value.</param>
        [SkippableTheory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void Test_WhenValueIsOutOfRangeKind_ThenParameterBindingExceptionIsThrown(int value)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateRangeWithRangeKindNonNegative, value);

            action.Should().Throw<ParameterBindingException>();
        }

#endif
    }
}