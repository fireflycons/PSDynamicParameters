namespace Firefly.PowerShell.DynamicParameters.Tests
{
    using System.Linq;

    using Firefly.PowerShell.DynamicParameters.TestCmdlet;

    using FluentAssertions;

    using Xunit;

    /// <summary>
    /// Tests <c>ValueFromRemainingArguments</c>
    /// </summary>
    public class ValueFromRemainingArgumentsTest
    {
        /// <summary>
        /// Tests that when value from remaining arguments is set then array of values passed in is returned.
        /// </summary>
        [Fact]
        public void Test_WhenValueFromRemainingArgumentsIsSet_ThenArrayPassedInIsReturned()
        {
            var result = TestCmdletHost.RunTestHost(
                TestCases.ValueFromRemainingArguments,
                Constants.RemainingArguments);

            // ReSharper disable once StyleCop.SA1119 - Stylecop gets it wrong here.
            ((string[])(result.First().BaseObject)).SequenceEqual(Constants.RemainingArguments).Should().BeTrue();
        }
    }
}