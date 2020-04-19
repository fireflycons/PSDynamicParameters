namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Test some basic stuff like scalar values, argument with a specific type and mandatory argument.
    /// </summary>
    public class BasicTests
    {
        /// <summary>
        /// Tests that a POCO object is passed through as a dynamic parameter and the same instance is returned
        /// </summary>
        [Fact]
        public void Test_WhenPocoIsGivenWithTypeInfomation_ItIsReturned()
        {
            var expected = new TestPoco();
            var result = TestCmdletHost.RunTestHost(TestCases.PocoArgument, expected);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");
            var actual = result.First().BaseObject;

            actual.GetType().Should().Be<TestPoco>();
            ReferenceEquals(expected, actual).Should().BeTrue(
                "the actual instance should pass through the PowerShell pipeline unfettered");
        }

        /// <summary>
        /// When the dynamic parameter is a simple scalar with a default type of object
        /// </summary>
        /// <param name="value">Value for dynamic parameter</param>
        [Theory]
        [InlineData(1)]
        [InlineData(3.14)]
        [InlineData("a string")]
        public void Test_WhenScalarValueIsGivenWithNoTypeInfomation_ItIsReturnedAsOriginalType(object value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.UndecoratedArgument, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.GetType().Should().Be(value.GetType(), "we passed the value boxed in an object which should be truthfully returned.");
            result.First().BaseObject.Should().Be(value);
        }

        /// <summary>
        /// Tests the when scalar value is given and double is the type it is returned as double.
        /// </summary>
        /// <param name="value">The value.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(3.14)]
        public void Test_WhenScalarValueIsGivenAndDoubleIsTheType_ItIsReturnedAsDouble(object value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.NumericArgumentDouble, value);

            var expected = value is int i ? i : (double)value;

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");
            var actual = result.First().BaseObject;

            actual.GetType().Should().Be<double>();
            actual.Should().Be(expected);
        }

        /// <summary>
        /// Tests the when scalar value is given and double is the type and non numeric passed then parameter binding exception is thrown.
        /// </summary>
        [SkippableFact]
        public void Test_WhenScalarValueIsGivenAndDoubleIsTheTypeAndNonNumericPassed_ThenParameterBindingExceptionIsThrown()
        {
            Skip.IfNot(Constants.IsWindows);

            var expectedMessage =
                "Cannot bind parameter 'TestParameter'. Cannot convert value \"string\" to type \"System.Double\". Error: \"Input string was not in a correct format.\"";

            Action action = () => TestCmdletHost.RunTestHost(TestCases.NumericArgumentDouble, "string");

            action.Should().Throw<ParameterBindingException>().WithMessage(expectedMessage);
        }
    }
}