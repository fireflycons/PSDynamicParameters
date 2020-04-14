namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System.Linq;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests referring to dynamic parameter using an alias
    /// </summary>
    public class AliasTests
    {
        /// <summary>
        /// When the dynamic parameter is a simple scalar with a default type of object
        /// </summary>
        /// <param name="value">Value for dynamic parameter</param>
        /// <param name="parameterAlias">Parameter alias to use</param>
        [Theory]
        [InlineData(1, Constants.ParameterAliasOne)]
        [InlineData(1, Constants.ParameterAliasTwo)]
        [InlineData(3.14, Constants.ParameterAliasOne)]
        [InlineData(3.14, Constants.ParameterAliasTwo)]
        [InlineData("a string", Constants.ParameterAliasOne)]
        [InlineData("a string", Constants.ParameterAliasTwo)]
        public void Test_WhenScalarValueIsGivenByAlias_ItIsReturnedAsOriginalType(
            object value,
            string parameterAlias)
        {
            var result = TestCmdletHost.RunTestHost(TestCases.AliasArgument, value, parameterAlias);

            result.Count.Should().Be(1, "a single value was passed to the dynamic parameter");

            var actual = result.First().BaseObject;
            actual.GetType().Should().Be(
                value.GetType(),
                "we passed the value boxed in an object which should be truthfully returned.");
            result.First().BaseObject.Should().Be(value);
        }
    }
}