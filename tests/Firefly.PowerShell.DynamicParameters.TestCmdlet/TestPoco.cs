namespace Firefly.PowerShell.DynamicParameters.TestCmdlet
{
    /// <summary>
    /// A POCO to pass as an argument value to the test cmdlet to check it remains the same object though the pipeline
    /// </summary>
    public class TestPoco
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPoco"/> class.
        /// </summary>
        public TestPoco()
        {
            this.TestParameter = Constants.TestPocoPropertyValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestPoco"/> class.
        /// </summary>
        /// <param name="testParameterValue">The test parameter value.</param>
        public TestPoco(string testParameterValue)
        {
            this.TestParameter = testParameterValue;
        }

        /// <summary>
        /// Gets the description of this object
        /// </summary>
        /// <remarks>
        /// This property name must match the name of the dynamic parameter in <see cref="ShowDynamicParameterCommand"/> for value from pipeline by property name to work
        /// </remarks>
        public string TestParameter { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return this.TestParameter;
        }
    }
}