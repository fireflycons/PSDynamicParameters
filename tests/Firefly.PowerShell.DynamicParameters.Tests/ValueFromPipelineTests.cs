namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System.Linq;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests passing dynamic parameter values through the pipeline
    /// </summary>
    public class ValueFromPipelineTests
    {
        /// <summary>
        /// Tests that a POCO object is passed through as a dynamic parameter and the same instance is returned
        /// </summary>
        [Fact]
        public void Test_WhenPocoIsPassedWithValueFromPipeline_ItIsReturned()
        {
            var expected = new TestPoco();
            var result = TestCmdletHost.RunTestHost(TestCases.ValueFromPipeline, expected);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");
            var actual = result.First().BaseObject;

            actual.GetType().Should().Be<TestPoco>();
            ReferenceEquals(expected, actual).Should().BeTrue(
                "the actual instance should pass through the PowerShell pipeline unfettered");
        }

        /// <summary>
        /// Tests that a POCO object is passed through as a dynamic parameter and the same instance is returned
        /// </summary>
        [Fact]
        public void Test_WhenPocoIsPassedWithValueFromPipelineByPropertyName_ThePropertyValueIsReturned()
        {
            var expected = new TestPoco();
            var result = TestCmdletHost.RunTestHost(TestCases.ValueFromPipelineByPropertyName, expected);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");
            var actual = result.First().BaseObject;

            actual.GetType().Should().Be<string>();
            actual.ToString().Should().Be(Constants.TestPocoPropertyValue);
        }

        /// <summary>
        /// When the dynamic parameter is a simple scalar with a default type of object
        /// </summary>
        /// <param name="value">Value to pipe to cmdlet</param>
        [Theory]
        [InlineData(1)]
        [InlineData(3.14)]
        [InlineData("a string")]
        public void Test_WhenScalarValueIsPipedWithNoTypeInfomation_ItIsReturnedAsOriginalType(object value)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.ValueFromPipeline, value);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.GetType().Should().Be(
                value.GetType(),
                "we passed the value boxed in an object which should be truthfully returned.");
            result.First().BaseObject.ToString().Should().Be(value.ToString());
        }
    }
}