namespace Firefly.PowerShell.DynamicParameters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Builder class with fluent syntax to generate a PowerShell dynamic parameter.
    /// </summary>
    public class RuntimeDefinedParameterBuilder
    {
        /// <summary>
        /// The additional attributes to apply to the parameter
        /// </summary>
        private readonly List<Attribute> attributesToApply = new List<Attribute>();

        /// <summary>
        /// The parameter name
        /// </summary>
        private readonly string parameterName;

        /// <summary>
        /// The parameter type
        /// </summary>
        private readonly Type parameterType;

        /// <summary>
        /// <c>true</c> if parameter will be mandatory; else <c>false</c>
        /// </summary>
        private bool mandatory;

        /// <summary>
        /// The parameter help message
        /// </summary>
        private string parameterHelpMessage;

        /// <summary>
        /// The parameter parameter set name
        /// </summary>
        private string[] parameterParameterSetNames;

        /// <summary>
        /// The parameter position
        /// </summary>
        private int? parameterPosition;

#if NETCOREAPP3_1

        /// <summary>
        /// The validation error message. Applied to both <see cref="ValidateSetAttribute"/> and <see cref="ValidatePatternAttribute"/>.
        /// The assumption is that you'd wouldn't set both attributes on a single parameter.
        /// </summary>
        private string validationErrorMessage;
#endif

        /// <summary>
        /// <c>true</c> if parameter will take value from pipeline; else <c>false</c>
        /// </summary>
        private bool valueFromPipeline;

        /// <summary>
        /// <c>true</c> if parameter will take value from pipeline by property name; else <c>false</c>
        /// </summary>
        private bool valueFromPipelineByPropertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeDefinedParameterBuilder"/> class.
        /// The parameter will have a default type of <see cref="object"/>
        /// </summary>
        /// <param name="name">The parameter name.</param>
        public RuntimeDefinedParameterBuilder(string name)
        {
            this.parameterName = name;
            this.parameterType = typeof(object);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeDefinedParameterBuilder"/> class.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="type">The parameter type.</param>
        public RuntimeDefinedParameterBuilder(string name, Type type)
        {
            this.parameterName = name;
            this.parameterType = type;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="RuntimeDefinedParameterBuilder"/> class from being created.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private RuntimeDefinedParameterBuilder()
        {
        }

        /// <summary>
        /// Builds the parameter.
        /// </summary>
        /// <returns>A new <see cref="RuntimeDefinedParameter"/></returns>
        public RuntimeDefinedParameter Build()
        {
            // Define [Parameter] attribute
            var paramAttribute = new ParameterAttribute
                                     {
                                         Mandatory = this.mandatory,
                                         ValueFromPipeline = this.valueFromPipeline,
                                         ValueFromPipelineByPropertyName = this.valueFromPipelineByPropertyName
                                     };

            if (this.parameterPosition.HasValue)
            {
                paramAttribute.Position = this.parameterPosition.Value;
            }

            if (this.parameterParameterSetNames != null && this.parameterParameterSetNames.Any())
            {
                paramAttribute.ParameterSetName = this.parameterParameterSetNames.First();
            }
            else
            {
                paramAttribute.ParameterSetName = "__AllParameterSets";
            }

            if (this.parameterHelpMessage != null)
            {
                paramAttribute.HelpMessage = this.parameterHelpMessage;
            }

            var attributeCollection = new Collection<Attribute> { paramAttribute };

            // Additional parameter sets
            if (this.parameterParameterSetNames != null && this.parameterParameterSetNames.Length > 1)
            {
                foreach (var parameterSetName in this.parameterParameterSetNames.Skip(1))
                {
                    attributeCollection.Add(new ParameterAttribute { ParameterSetName = parameterSetName });
                }
            }

            // Add in any other attributes we defined
            foreach (var attribute in this.attributesToApply)
            {
#if NETCOREAPP3_1

                // Custom error message in PS 7.x
                if (this.validationErrorMessage != null)
                {
                    switch (attribute)
                    {
                        case ValidateSetAttribute vsa:

                            vsa.ErrorMessage = this.validationErrorMessage;
                            break;

                        case ValidatePatternAttribute vpa:

                            vpa.ErrorMessage = this.validationErrorMessage;
                            break;
                    }
                }
#endif
                attributeCollection.Add(attribute);
            }

            return new RuntimeDefinedParameter(this.parameterName, this.parameterType, attributeCollection);
        }

        /// <summary>
        /// Declares parameter aliases.
        /// </summary>
        /// <param name="aliases">The additional aliases.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithAliases(params string[] aliases)
        {
            this.attributesToApply.Add(new AliasAttribute(aliases));
            return this;
        }

        /// <summary>
        /// <para>Applies <see cref="System.Management.Automation.AllowNullAttribute"/>.</para>
        /// <para>For parameters marked as mandatory, permits the value to be explicitly <c>$null</c></para>
        /// </summary>
        /// <returns>This builder.</returns>
        public RuntimeDefinedParameterBuilder WithAllowNull()
        {
            this.attributesToApply.Add(new AllowNullAttribute());
            return this;
        }

        /// <summary>
        /// Declares a help message describing the parameter usage.
        /// </summary>
        /// <param name="helpMessage">The help message.</param>
        /// <returns>This builder</returns>
        /// <returns></returns>
        public RuntimeDefinedParameterBuilder WithHelpMessage(string helpMessage)
        {
            this.parameterHelpMessage = helpMessage;
            return this;
        }

        /// <summary>
        /// Declares parameter mandatory.
        /// </summary>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithMandatory()
        {
            this.mandatory = true;
            return this;
        }

        /// <summary>
        /// Includes parameter in named parameter set(s).
        /// </summary>
        /// <param name="parameterSetNames">Names of the parameter sets in which to include this parameter.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithParameterSets(params string[] parameterSetNames)
        {
            this.parameterParameterSetNames = parameterSetNames;
            return this;
        }

        /// <summary>
        /// Sets the parameter as positional and what position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithPosition(int position)
        {
            this.parameterPosition = position;
            return this;
        }

        /// <summary>
        /// <para>Applies <see cref="ValidateCountAttribute"/></para>
        /// <para>Where the parameter accepts an array, defines min and max length of the array.</para>
        /// </summary>
        /// <param name="minLength">The minimum length.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateCount(int minLength, int maxLength)
        {
            this.attributesToApply.Add(new ValidateCountAttribute(minLength, maxLength));
            return this;
        }

        /// <summary>
        /// <para>Applies <see cref="ValidateLengthAttribute"/></para>
        /// <para>Where the parameter is a string, defines min and max length of the string.</para>
        /// </summary>
        /// <param name="minLength">The minimum length.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateLength(int minLength, int maxLength)
        {
            this.attributesToApply.Add(new ValidateLengthAttribute(minLength, maxLength));
            return this;
        }

        /// <summary>
        /// <para>Applies <see cref="ValidateNotNullAttribute"/></para>
        /// <para>Ensures parameter value is not null.</para>
        /// </summary>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateNotNull()
        {
            this.attributesToApply.Add(new ValidateNotNullAttribute());
            return this;
        }

        /// <summary>
        /// <para>Applies <see cref="ValidateNotNullOrEmptyAttribute"/></para>
        /// <para>Ensures parameter value is not null and if a string, it is not the empty string.</para>
        /// </summary>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateNotNullOrEmpty()
        {
            this.attributesToApply.Add(new ValidateNotNullOrEmptyAttribute());
            return this;
        }

        /// <summary>
        /// Applies <see cref="ValidatePatternAttribute"/> with given regex and options.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="options">Optional <see cref="RegexOptions"/> to apply to regex pattern.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidatePattern(
            string regexPattern,
            RegexOptions options = RegexOptions.None)
        {
            this.attributesToApply.Add(new ValidatePatternAttribute(regexPattern) { Options = options });
            return this;
        }

        /// <summary>
        /// Applies <see cref="ValidatePatternAttribute"/> with given <see cref="Regex"/>.
        /// </summary>
        /// <param name="regex">The regex to apply.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidatePattern(
            Regex regex)
        {
            this.attributesToApply.Add(new ValidatePatternAttribute(regex.ToString()) { Options = regex.Options });
            return this;
        }

        /// <summary>
        /// <para>Applies <see cref="ValidateRangeAttribute"/> with custom range</para>
        /// <para>Ensures the parameter value is not less than <paramref name="minRange"/> and not greater than <paramref name="maxRange"/></para>
        /// </summary>
        /// <param name="minRange">
        /// The minimum bound.
        /// </param>
        /// <param name="maxRange">
        /// The maximum bound.
        /// </param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateRange(object minRange, object maxRange)
        {
            this.attributesToApply.Add(new ValidateRangeAttribute(minRange, maxRange));
            return this;
        }

        /// <summary>
        /// Applies <see cref="ValidateScriptAttribute"/> with given script.
        /// </summary>
        /// <param name="validateScript">Validation script.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateScript(ScriptBlock validateScript)
        {
            this.attributesToApply.Add(new ValidateScriptAttribute(validateScript));
            return this;
        }

        /// <summary>
        /// Applies <see cref="ValidateSetAttribute"/> with given values.
        /// </summary>
        /// <param name="validValues">The valid values.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateSet(params string[] validValues)
        {
            this.attributesToApply.Add(new ValidateSetAttribute(validValues));
            return this;
        }

        /// <summary>
        /// Declares parameter as able to take value from pipeline.
        /// </summary>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValueFromPipeline()
        {
            this.valueFromPipeline = true;
            return this;
        }

        /// <summary>
        /// Declares parameter as able to take value from pipeline by property.
        /// </summary>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValueFromPipelineByPropertyName()
        {
            this.valueFromPipelineByPropertyName = true;
            return this;
        }

#if NETCOREAPP3_1

        /// <summary>
        /// <para><b>PowerShell 7 only.</b></para>
        /// <para>
        /// Sets the validation error message format string.
        /// Currently supported by <see cref="ValidateSetAttribute"/> and <see cref="ValidatePatternAttribute"/>.
        /// </para>
        /// <para>See also:</para>
        /// <para><seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.validatesetattribute.errormessage"/></para>
        /// <para><seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.validatepatternattribute.errormessage"/></para>
        /// </summary>
        /// <param name="errorMessageFormatString">The error message format string.</param>
        /// <returns>This builder</returns>
        /// <remarks>
        /// <para>PowerShell will prepend <c>Cannot validate argument on parameter '{0}'. </c> to any custom error message, where <c>{0}</c> is set to the parameter name.</para>
        /// </remarks>
        public RuntimeDefinedParameterBuilder WithValidationErrorMessage(string errorMessageFormatString)
        {
            this.validationErrorMessage = errorMessageFormatString;
            return this;
        }
#endif

#if NETCOREAPP

        /// <summary>
        /// <para><b>PowerShell Core only.</b></para>
        /// <para>Applies <see cref="ValidateRangeAttribute"/> with one of the built-in <see cref="ValidateRangeKind"/> checks.</para>
        /// </summary>
        /// <param name="rangeKind">Kind of the range.</param>
        /// <returns>This builder</returns>
        public RuntimeDefinedParameterBuilder WithValidateRange(ValidateRangeKind rangeKind)
        {
            this.attributesToApply.Add(new ValidateRangeAttribute(rangeKind));
            return this;
        }

#endif
    }
}