namespace Firefly.PowerShell.DynamicParameters.TestCmdlet
{
    using System.Management.Automation;

    public static class Constants
    {
        /// <summary>
        /// Parameter name of the dynamic parameter we will create in <see cref="ShowDynamicParameterCommand"/>
        /// </summary>
        public const string DynamicParameterName = "TestParameter";

        /// <summary>
        /// Value that will be assigned to instance of <see cref="TestPoco"/>
        /// </summary>
        public const string TestPocoPropertyValue = "I am a test object";

        /// <summary>
        /// The parameter name of first alias for parameter alias tests
        /// </summary>
        public const string ParameterAliasOne = "AliasOne";

        /// <summary>
        /// The parameter name of second alias for parameter alias tests
        /// </summary>
        public const string ParameterAliasTwo = "AliasTwo";

        /// <summary>
        /// Invalid IP address custom message for validate pattern with custom error message.
        /// </summary>
        public const string InvalidIpAddressCustomMessage = "'{0}' is not a valid IP address.";

        /// <summary>
        /// The invalid parameter value custom message for validate set with custom error message.
        /// </summary>
        public const string InvalidParameterValueCustomMessage = "'{0}' is not a permitted value.";

        /// <summary>
        /// IP address regex
        /// </summary>
        public const string IpAddressRegex = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        /// <summary>
        /// The case sensitivity regex
        /// </summary>
        public const string CaseSensitivityRegex = "[a-z]+";

        /// <summary>
        /// First parameter for parameter set tests
        /// </summary>
        public const string DynamicParameterSetFirstParameter = "FirstParameter";

        /// <summary>
        /// Second parameter for parameter set tests
        /// </summary>
        public const string DynamicParameterSetSecondParameter = "SecondParameter";

        /// <summary>
        /// Name of a parameter set
        /// </summary>
        public const string DynamicParameterSetsSetA = "SetA";

        /// <summary>
        /// Name of a parameter set
        /// </summary>
        public const string DynamicParameterSetsSetB = "SetB";

        /// <summary>
        /// Validate Set valid values
        /// </summary>
        public static readonly string[] ValidStrings = { "One", "Two", "Three" };

        /// <summary>
        /// A min/max range for Validate Range test
        /// </summary>
        public static readonly int[] ValidRange = { 4, 8 };

        /// <summary>
        /// A count range for <see cref="ValidateCountAttribute"/>
        /// </summary>
        public static readonly int[] ValidCounts = { 3, 4 };

        /// <summary>
        /// A length range for <see cref="ValidateLengthAttribute"/>
        /// </summary>
        public static readonly int[] ValidLengths = { 3, 4 };

        /// <summary>
        /// A script block for validate script test. Does the same as the above valid range
        /// </summary>
        public static readonly ScriptBlock ValidateScript = ScriptBlock.Create("$_ -ge 4 -and $_ -le 8");

#if NETCOREAPP

        /// <summary>
        /// A range kind for Validate Range tests
        /// </summary>
        public const ValidateRangeKind ValidRangeKindNonNegative = ValidateRangeKind.NonNegative;
#endif
    }
}