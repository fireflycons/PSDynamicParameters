namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System.Linq;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Test positional parameter
    /// </summary>
    public class PositionalParameterTests
    {
        /// <summary>
        /// Tests when POCO is given by position it is returned.
        /// </summary>
        [Fact]
        public void Test_WhenPocoIsGivenByPosition_ItIsReturned()
        {
            var expected = new TestPoco();
            var result = TestCmdletHost.RunTestHost(TestCases.PositionalArgument, expected);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");
            var actual = result.First().BaseObject;

            actual.GetType().Should().Be<TestPoco>();
            ReferenceEquals(expected, actual).Should().BeTrue(
                "the actual instance should pass through the PowerShell pipeline unfettered");
        }

        /// <summary>
        /// Tests when scalar value is given by position it is returned as original.
        /// </summary>
        /// <param name="value">Value for dynamic parameter</param>
        [Theory]
        [InlineData(1)]
        [InlineData(3.14)]
        [InlineData("a string")]
        public void Test_WhenScalarValueIsGivenByPosition_ItIsReturnedAsOriginalType(object value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.PositionalArgument, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.GetType().Should().Be(
                value.GetType(),
                "we passed the value boxed in an object which should be truthfully returned.");
            result.First().BaseObject.ToString().Should().Be(value.ToString());
        }
    }
}