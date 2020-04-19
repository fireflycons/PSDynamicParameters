namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Test parameter with <see cref="ValidateSetAttribute"/>
    /// </summary>
    public class ValidateSetTests
    {
        /// <summary>
        /// Tests that when invalid values are given on command line they throw <see cref="ParameterBindingException"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [SkippableTheory]
        [InlineData("Four")]
        [InlineData(1)]
        public void Test_WhenInvalidValuesAreGivenOnCommandLine_ThenParameterBindingExceptionIsThrown(object value)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateSetViaArguments, value);

            action.Should().Throw<ParameterBindingException>();
        }

#if NETCOREAPP3_1

        [SkippableFact]
        public void Test_WhenInvalidValuesAreGivenOnCommandLineAndCustomErrorMessage_ThenParameterBindingExceptionIsThrownWithCustomMessage()
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            const string Value = "Four";
            var customError = string.Format(Constants.InvalidParameterValueCustomMessage, Value);
            var expectedMessage =
                $"Cannot validate argument on parameter '{Constants.DynamicParameterName}'. {customError}";

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateSetWithCustomMessage, Value);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }

#endif

        /// <summary>
        /// Tests that when invalid values are piped they throw <see cref="ParameterBindingException"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        [SkippableTheory]
        [InlineData("Four")]
        [InlineData(1)]
        public void Test_WhenInvalidValuesArePiped_ThenParameterBindingExceptionIsThrown(object value)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateSetFromPipeline, value);

            action.Should().Throw<ParameterBindingException>();
        }

        /// <summary>
        /// Simple scalar tests with <see cref="ValidateSetAttribute"/>
        /// </summary>
        /// <param name="value">Value to pass to test</param>
        [Theory]
        [InlineData("One")]
        [InlineData("Two")]
        [InlineData("Three")]
        public void Test_WhenValidValuesAreGivenOnCommandLine_TheyAreReturned(object value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.ValidateSetViaArguments, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;

            actual.Should().Be(value);
        }

        /// <summary>
        /// Simple scalar tests with <see cref="ValidateSetAttribute"/>
        /// </summary>
        /// <param name="value">Value to pass to test</param>
        [Theory]
        [InlineData("One")]
        [InlineData("Two")]
        [InlineData("Three")]
        public void Test_WhenValidValuesArePiped_TheyAreReturned(object value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.ValidateSetFromPipeline, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;

            actual.Should().Be(value);
        }

        /// <summary>
        /// Tests that when POCO with valid property value is piped the value is returned.
        /// </summary>
        /// <param name="value">The value.</param>
        [Theory]
        [InlineData("One")]
        [InlineData("Two")]
        [InlineData("Three")]
        public void Test_WhenPocoWithValidPropertyValueIsPiped_TheValueIsReturned(string value)
        {
            var poco = new TestPoco(value);

            var result = TestCmdletHost.RunTestHost(TestCases.ValidateSetFromPipelineByPropertyName, poco);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;

            actual.Should().Be(value);
        }

        /// <summary>
        /// Tests that when poco with invalid property value is piped it throws <see cref="ParameterBindingException"/>.
        /// </summary>
        [SkippableFact]
        public void Test_WhenPocoWithInvalidPropertyValueIsPiped_ThenParameterBindingExceptionIsThrown()
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            const string InvalidValue = "Four";

            var poco = new TestPoco(InvalidValue);

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidateSetFromPipelineByPropertyName, poco);

            action.Should().Throw<ParameterBindingException>();
        }

    }
}