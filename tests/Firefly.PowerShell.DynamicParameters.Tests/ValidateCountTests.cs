namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests <see cref="ValidateCountAttribute"/>
    /// </summary>
    public class ValidateCountTests
    {
        /// <summary>
        /// Tests the when array length is within count validation then no exception is thrown.
        /// <seealso cref="Constants.ValidCounts"/>
        /// </summary>
        /// <param name="arrayLength">Length of the array.</param>
        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void Test_WhenArrayLengthIsWithinCountValidation_ThenNoExceptionIsThrown(int arrayLength)
        {
            var testArray = new string[arrayLength];

            for (var i = 0; i < arrayLength; ++i)
            {
                testArray[i] = $"{i}";
            }

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateCount, testArray);

            action.Should().NotThrow();
        }

        /// <summary>
        /// Tests the when array length is outside count validation then parameter binding exception is thrown.
        /// </summary>
        /// <param name="arrayLength">Length of the array.</param>
        /// <seealso cref="Constants.ValidCounts"/>
        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        public void Test_WhenArrayLengthIsOutsideCountValidation_ThenParameterBindingExceptionIsThrown(int arrayLength)
        {
            var testArray = new string[arrayLength];

            for (var i = 0; i < arrayLength; ++i)
            {
                testArray[i] = $"{i}";
            }

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateCount, testArray);

            action.Should().Throw<ParameterBindingException>();
        }
    }
}