namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests use of <see cref="ValidatePatternAttribute"/>
    /// </summary>
    public class ValidatePatternTests
    {
        /// <summary>
        /// Tests the when invalid IP addresses are passed to parameter with IP validation regex then parameter binding exception is thrown.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        [SkippableTheory]
        [InlineData("256.0.0.0")]
        [InlineData("192.168.0.256")]
        [InlineData("8.8.256.8")]
        public void
            Test_WhenInvalidIPAddressesArePassedToParameterWithIPValidationRegexObject_ThenParameterBindingExceptionIsThrown(
                string ipAddress)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            var expectedMessage =
                $"Cannot validate argument on parameter 'TestParameter'. The argument \"{ipAddress}\" does not match the \"{Constants.IpAddressRegex}\" pattern. Supply an argument that matches \"{Constants.IpAddressRegex}\" and try the command again.";
            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidatePatternWithRegexObject, ipAddress);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }

        /// <summary>
        /// Tests the when invalid IP addresses are passed to parameter with IP validation regex then parameter binding exception is thrown.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        [SkippableTheory]
        [InlineData("256.0.0.0")]
        [InlineData("192.168.0.256")]
        [InlineData("8.8.256.8")]
        public void
            Test_WhenInvalidIPAddressesArePassedToParameterWithIPValidationRegex_ThenParameterBindingExceptionIsThrown(
                string ipAddress)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            var expectedMessage =
                $"Cannot validate argument on parameter 'TestParameter'. The argument \"{ipAddress}\" does not match the \"{Constants.IpAddressRegex}\" pattern. Supply an argument that matches \"{Constants.IpAddressRegex}\" and try the command again.";
            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidatePatternViaArguments, ipAddress);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }

        /// <summary>
        /// Tests the when valid IP addresses are passed to parameter with IP validation regex they are returned.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        [SkippableTheory]
        [InlineData("10.0.0.0")]
        [InlineData("192.168.0.23")]
        [InlineData("8.8.8.8")]
        public void Test_WhenValidIPAddressesArePassedToParameterWithIPValidationRegex_TheyAreReturned(string ipAddress)
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            var result = TestCmdletHost.RunTestHost(TestCases.ValidatePatternViaArguments, ipAddress);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.Should().Be(ipAddress);
        }

        /// <summary>
        /// Tests the when valid IP addresses are passed to parameter with IP validation regex they are returned.
        /// </summary>
        /// <param name="ipAddress">The IP address.</param>
        [Theory]
        [InlineData("10.0.0.0")]
        [InlineData("192.168.0.23")]
        [InlineData("8.8.8.8")]
        public void Test_WhenValidIPAddressesArePassedToParameterWithIPValidationRegexObject_TheyAreReturned(string ipAddress)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.ValidatePatternWithRegexObject, ipAddress);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.Should().Be(ipAddress);
        }

        /// <summary>
        /// Tests the when value does not match case sensitive regex then parameter binding exception is thrown.
        /// </summary>
        [SkippableFact]
        public void
            Test_WhenValueDoesNotMatchCaseSensitiveRegex_ThenParameterBindingExceptionIsThrown()
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            const string TestValue = "ABC";

            var expectedMessage =
                $"Cannot validate argument on parameter 'TestParameter'. The argument \"{TestValue}\" does not match the \"{Constants.CaseSensitivityRegex}\" pattern. Supply an argument that matches \"{Constants.CaseSensitivityRegex}\" and try the command again.";

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidatePatterWithOptionsCaseSensitive, TestValue);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }

        /// <summary>
        /// Tests the when value does not match case sensitive regex then parameter binding exception is thrown.
        /// </summary>
        [SkippableFact]
        public void
            Test_WhenValueDoesNotMatchCaseSensitiveRegexObject_ThenParameterBindingExceptionIsThrown()
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            const string TestValue = "ABC";

            var expectedMessage =
                $"Cannot validate argument on parameter 'TestParameter'. The argument \"{TestValue}\" does not match the \"{Constants.CaseSensitivityRegex}\" pattern. Supply an argument that matches \"{Constants.CaseSensitivityRegex}\" and try the command again.";

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidatePatterWithRegexObjectOptionsCaseSensitive, TestValue);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }

        /// <summary>
        /// Tests the when value does not match case sensitive regex then no exception is thrown.
        /// </summary>
        [Fact]
        public void
            Test_WhenValueDoesNotMatchCaseSensitiveRegex_ThenNoExceptionIsThrown()
        {

            const string TestValue = "ABC";

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidatePatterWithOptionsCaseInsensitive, TestValue);

            action.Should().NotThrow();
        }

        /// <summary>
        /// Tests the when value does not match case sensitive regex then no exception is thrown.
        /// </summary>
        [Fact]
        public void
            Test_WhenValueDoesNotMatchCaseSensitiveRegexObject_ThenNoExceptionIsThrown()
        {
            const string TestValue = "ABC";

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidatePatternWithRegexObjectOptionsCaseInsensitive, TestValue);

            action.Should().NotThrow();
        }

#if NETCOREAPP3_1

        /// <summary>
        /// Tests that when invalid IP addresses are passed to parameter with IP validation regex and a custom error message is set,
        /// then parameter binding exception is thrown with the custom message.
        /// </summary>
        [SkippableFact]
        public void
            Test_WhenInvalidIPAddressesArePassedToParameterWithIPValidationRegexAndCustomErrorMessage_ThenParameterBindingExceptionIsThrownWithCustomMessage()
        {
            Skip.IfNot(Constants.IsWindows, Constants.SkipReason);

            const string IpAddress = "256.0.0.0";
            var customError = string.Format(Constants.InvalidIpAddressCustomMessage, IpAddress);
            var expectedMessage =
                $"Cannot validate argument on parameter '{Constants.DynamicParameterName}'. {customError}";

            Action action = () => TestCmdletHost.RunTestHost(TestCases.ValidatePatternWithCustomMessage, IpAddress);

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);

        }

#endif
    }
}