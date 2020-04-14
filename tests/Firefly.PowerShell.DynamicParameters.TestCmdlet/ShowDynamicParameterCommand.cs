namespace Firefly.PowerShell.DynamicParameters.TestCmdlet
{
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A binary cmdlet that outputs the value passed to its dynamic parameter.
    /// Dynamic parameter is built based on the test to run.
    /// </summary>
    [Cmdlet(VerbsCommon.Show, "DynamicParameter")]
    public class ShowDynamicParameterCommand : PSCmdlet, IDynamicParameters
    {
        /// <summary>
        /// Gets or sets the test to run
        /// This is a mandatory fixed parameter to the cmdlet.
        /// </summary>
        [Parameter(Mandatory = true)]
        public int TestNumber { get; set; }

        /// <summary>
        /// Gets the dynamic parameters.
        /// The parameter is created based on the value of <see cref="Constants.DynamicParameterName"/>
        /// </summary>
        /// <returns>A <see cref="RuntimeDefinedParameterDictionary"/></returns>
        public object GetDynamicParameters()
        {
            var dynamicParams = new RuntimeDefinedParameterDictionaryHelper();

            switch ((TestCases)this.TestNumber)
            {
                case TestCases.UndecoratedArgument:

                    // All input values will be treated as object
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName));
                    break;

                case TestCases.NumericArgumentDouble:

                    // Input values will be cast to double
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName, typeof(double)));
                    break;

                case TestCases.PocoArgument:

                    // The input value is explicitly an instance of TestPoco
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName, typeof(TestPoco)));
                    break;

                case TestCases.MandatoryArgument:

                    // All input values will be treated as object - param is mandatory
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithMandatory());
                    break;

                case TestCases.MandatoryWithAllowNull:

                    dynamicParams.Add(
                        new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithMandatory().WithAllowNull());
                    break;

                case TestCases.ValueFromPipeline:

                    // All input values will be treated as object
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValueFromPipeline());
                    break;

                case TestCases.ValueFromPipelineByPropertyName:

                    // All input values will be treated as object
                    dynamicParams.Add(
                        new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName)
                            .WithValueFromPipelineByPropertyName());
                    break;

                case TestCases.ValidateSetViaArguments:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidateSet(Constants.ValidStrings));
                    break;

                case TestCases.ValidateSetFromPipeline:

                    dynamicParams.Add(
                        new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName)
                            .WithValidateSet(Constants.ValidStrings).WithValueFromPipeline());
                    break;

                case TestCases.ValidateSetFromPipelineByPropertyName:

                    dynamicParams.Add(
                        new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName)
                            .WithValidateSet(Constants.ValidStrings).WithValueFromPipelineByPropertyName());
                    break;

                case TestCases.PositionalArgument:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithPosition(0));
                    break;

                case TestCases.AliasArgument:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithAliases(
                        Constants.ParameterAliasOne,
                        Constants.ParameterAliasTwo));
                    break;

                case TestCases.ValidatePatternViaArguments:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidatePattern(Constants.IpAddressRegex));
                    break;

                case TestCases.ValidatePatternWithRegexObject:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidatePattern(new Regex(Constants.IpAddressRegex)));
                    break;

                case TestCases.ValidatePatterWithOptionsCaseSensitive:

                    // Uses default RegexOptions.None - which is case sensitive
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidatePattern(Constants.CaseSensitivityRegex));
                    break;

                case TestCases.ValidatePatterWithRegexObjectOptionsCaseSensitive:

                    // Uses default RegexOptions.None - which is case sensitive
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidatePattern(new Regex(Constants.CaseSensitivityRegex)));
                    break;

                case TestCases.ValidatePatterWithOptionsCaseInsensitive:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidatePattern(Constants.CaseSensitivityRegex, RegexOptions.IgnoreCase));
                    break;

                case TestCases.ValidatePatternWithRegexObjectOptionsCaseInsensitive:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidatePattern(new Regex(Constants.CaseSensitivityRegex, RegexOptions.IgnoreCase)));
                    break;

                case TestCases.ValidateRangeWithMinMax:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName, typeof(int)).WithValidateRange(Constants.ValidRange[0], Constants.ValidRange[1]));
                    break;

                case TestCases.ValidateScript:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName, typeof(int)).WithValidateScript(Constants.ValidateScript));
                    break;

                case TestCases.ValidateNotNull:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidateNotNull());
                    break;

                case TestCases.ValidateNotNullOrEmpty:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidateNotNullOrEmpty());
                    break;

                case TestCases.ValidateCount:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidateCount(Constants.ValidCounts[0], Constants.ValidCounts[1]));
                    break;

                case TestCases.ValidateLength:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName).WithValidateLength(Constants.ValidLengths[0], Constants.ValidLengths[1]));
                    break;

                case TestCases.ParameterSetsFirstParameterInSetASecondParameterNotInSet:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterSetFirstParameter).WithParameterSets(Constants.DynamicParameterSetsSetA));
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterSetSecondParameter));
                    break;

                case TestCases.ParameterSetInvalidCombination:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterSetFirstParameter).WithParameterSets(Constants.DynamicParameterSetsSetA));
                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterSetSecondParameter).WithParameterSets(Constants.DynamicParameterSetsSetB));
                    break;
#if NETCOREAPP
                // PowerShell Core only
                case TestCases.ValidateRangeWithRangeKindNonNegative:

                    dynamicParams.Add(new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName, typeof(int)).WithValidateRange(Constants.ValidRangeKindNonNegative));
                    break;

#endif

#if NETCOREAPP3_1

                // PowerShell 7 only
                case TestCases.ValidatePatternWithCustomMessage:

                    dynamicParams.Add(
                        new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName)
                            .WithValidatePattern(Constants.IpAddressRegex)
                            .WithValidationErrorMessage(Constants.InvalidIpAddressCustomMessage));
                    break;

                case TestCases.ValidateSetWithCustomMessage:

                    dynamicParams.Add(
                        new RuntimeDefinedParameterBuilder(Constants.DynamicParameterName)
                            .WithValidateSet(Constants.ValidStrings)
                            .WithValidationErrorMessage(Constants.InvalidParameterValueCustomMessage));
                    break;
#endif
            }

            // Return the RuntimeDefinedParameterDictionaryHelper.
            return (RuntimeDefinedParameterDictionary)dynamicParams;
        }

        /// <summary>
        /// Body of the cmdlet.
        /// If a dynamic parameter is present, emit its value to the pipeline
        /// </summary>
        protected override void ProcessRecord()
        {
            switch ((TestCases)this.TestNumber)
            {
                // For these cases, emit the parameter set name
                case TestCases.ParameterSetsFirstParameterInSetASecondParameterNotInSet:
                case TestCases.ParameterSetInvalidCombination:

                    this.WriteObject(this.ParameterSetName);
                    break;

                default:

                    // Emit the value passed to the dynamic parameter, if it is bound
                    // This is $PSBoundParameters
                    var boundParameters = this.MyInvocation.BoundParameters;

                    if (boundParameters.ContainsKey(Constants.DynamicParameterName))
                    {
                        this.WriteObject(boundParameters[Constants.DynamicParameterName]);
                    }

                    break;
            }
        }
    }
}