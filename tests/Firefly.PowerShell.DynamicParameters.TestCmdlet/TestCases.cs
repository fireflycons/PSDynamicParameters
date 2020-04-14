namespace Firefly.PowerShell.DynamicParameters.TestCmdlet
{
    using System.Management.Automation;

    /// <summary>
    /// Describes the tests we are running. Used to influence how <see cref="ShowDynamicParameterCommand"/> builds the dynamic parameter.
    /// </summary>
    public enum TestCases
    {
        /// <summary>
        /// Test a simple scalar with no type information - the input will be cast as string.
        /// </summary>
        UndecoratedArgument = 1,

        /// <summary>
        /// The scalar argument is expected to be a <see cref="double"/>
        /// </summary>
        NumericArgumentDouble,

        /// <summary>
        /// Test a POCO object, with its type such that the generated parameter respects the object's type.
        /// </summary>
        PocoArgument,

        /// <summary>
        /// Tests that if the dynamic parameter is marked mandatory, an exception will be thrown if the parameter is not presented.
        /// </summary>
        MandatoryArgument,

        /// <summary>
        /// Test we can pass <c>$null</c> to a mandatory parameter decorated with <see cref="AllowNullAttribute"/>
        /// </summary>
        MandatoryWithAllowNull,

        /// <summary>
        /// Tests passing scalars and objects via pipeline
        /// </summary>
        ValueFromPipeline,

        /// <summary>
        /// Tests passing a POCO via pipeline by property name
        /// </summary>
        ValueFromPipelineByPropertyName,

        /// <summary>
        /// Tests <see cref="ValidateSetAttribute "/> passing value as an argument
        /// </summary>
        ValidateSetViaArguments,

        /// <summary>
        /// Tests <see cref="ValidateSetAttribute "/> passing value in pipeline
        /// </summary>
        ValidateSetFromPipeline,

        /// <summary>
        /// Tests <see cref="ValidateSetAttribute "/> passing value from POCO property via pipeline
        /// </summary>
        ValidateSetFromPipelineByPropertyName,

        /// <summary>
        /// Tests <see cref="ValidateSetAttribute"/> with a custom error message (PowerShell 7 only)
        /// </summary>
        ValidateSetWithCustomMessage,

        /// <summary>
        /// Tests <see cref="ValidatePatternAttribute"/> passing value as argument.
        /// </summary>
        ValidatePatternViaArguments,

        /// <summary>
        /// Tests <see cref="ValidatePatternAttribute"/> with regex options case sensitive
        /// </summary>
        ValidatePatterWithOptionsCaseSensitive,

        /// <summary>
        /// Tests <see cref="ValidatePatternAttribute"/> with regex options case insensitive
        /// </summary>
        ValidatePatterWithOptionsCaseInsensitive,

        /// <summary>
        /// Tests <see cref="ValidatePatternAttribute"/> with argument from pipeline
        /// </summary>
        ValidatePatternFromPipeline,

        /// <summary>
        /// Tests <see cref="ValidatePatternAttribute"/> with a custom error message (PowerShell 7 only)
        /// </summary>
        ValidatePatternWithCustomMessage,

        /// <summary>
        /// Tests <see cref="ValidateRangeAttribute"/> with a min/max value
        /// </summary>
        ValidateRangeWithMinMax,

        /// <summary>
        /// Tests <see cref="ValidateRangeAttribute"/> with a <see cref="ValidateRangeKind"/>
        /// </summary>
        ValidateRangeWithRangeKindNonNegative,

        /// <summary>
        /// Tests <see cref="ValidateScriptAttribute"/>
        /// </summary>
        ValidateScript,

        /// <summary>
        /// Tests <see cref="ValidateNotNullAttribute"/>
        /// </summary>
        ValidateNotNull,

        /// <summary>
        /// Tests <see cref="ValidateNotNullOrEmptyAttribute"/>
        /// </summary>
        ValidateNotNullOrEmpty,

        /// <summary>
        /// Tests <see cref="ValidateCountAttribute"/>
        /// </summary>
        ValidateCount,

        /// <summary>
        /// Tests <see cref="ValidateLengthAttribute"/>
        /// </summary>
        ValidateLength,

        /// <summary>
        /// Tests a positional argument
        /// </summary>
        PositionalArgument,

        /// <summary>
        /// Tests parameter aliases
        /// </summary>
        AliasArgument,

        /// <summary>
        /// Tests parameter sets
        /// </summary>
        ParameterSetsFirstParameterInSetASecondParameterNotInSet,

        /// <summary>
        /// Tests invalid combination - params for both sets present.
        /// </summary>
        ParameterSetInvalidCombination
    }
}