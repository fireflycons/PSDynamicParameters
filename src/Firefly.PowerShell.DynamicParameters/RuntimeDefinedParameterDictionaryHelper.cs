namespace Firefly.PowerShell.DynamicParameters
{
    using System.Management.Automation;

    /// <summary>
    /// <para>
    /// Helper class to accumulate a <see cref="RuntimeDefinedParameterDictionary"/> which is required in the <c>DynamicParameters</c> declaration of a Cmdlet
    /// </para>
    /// <para>See also the following documentation</para>
    /// <para>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.idynamicparameters.getdynamicparameters"/>
    /// </para>
    /// <para>
    /// <seealso href="https://www.powershellmagazine.com/2014/06/23/dynamic-parameters-in-c-cmdlets/"/>
    /// </para>
    /// </summary>
    public class RuntimeDefinedParameterDictionaryHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeDefinedParameterDictionaryHelper"/> class with an empty parameter dictionary
        /// </summary>
        public RuntimeDefinedParameterDictionaryHelper()
        {
            this.DynamicParameters = new RuntimeDefinedParameterDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeDefinedParameterDictionaryHelper"/> class from an existing <see cref="RuntimeDefinedParameterDictionary"/>
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        public RuntimeDefinedParameterDictionaryHelper(RuntimeDefinedParameterDictionary dict)
        {
            this.DynamicParameters = dict;
        }

        /// <summary>
        /// Gets the current state of the contained <see cref="RuntimeDefinedParameterDictionary"/>.
        /// </summary>
        /// <value>
        /// The set of dynamic parameters that have been defined.
        /// </value>
        public RuntimeDefinedParameterDictionary DynamicParameters { get; }

        /// <summary>
        /// <para>
        /// Performs an explicit conversion from <see cref="RuntimeDefinedParameterDictionaryHelper"/> to <see cref="RuntimeDefinedParameterDictionary"/>.
        /// </para>
        /// </summary>
        /// <param name="dictionaryHelper">The dictionary helper instance.</param>
        /// <returns>
        /// <see cref="RuntimeDefinedParameterDictionary"/> which can be returned from <see cref="IDynamicParameters.GetDynamicParameters"/>. An explicit cast is needed since the return type is <see cref="object"/>
        /// </returns>
        /// <example>
        /// <code>
        /// return (RuntimeDefinedParameterDictionary)dictionaryHelper;
        /// </code>
        /// </example>
        public static explicit operator RuntimeDefinedParameterDictionary(
            RuntimeDefinedParameterDictionaryHelper dictionaryHelper)
        {
            return dictionaryHelper.DynamicParameters;
        }

        /// <summary>
        /// Adds a new dynamic parameter using the specified parameter builder.
        /// </summary>
        /// <param name="parameterBuilder">The parameter builder which will be built to yield the parameter to add.</param>
        /// <example>
        /// <description>Create a mandatory dynamic parameter with a validate set and return it to the cmdlet</description>
        /// <code>
        /// object GetDynamicParameters()
        /// {
        ///     var dictionaryHelper = new RuntimeDefinedParameterDictionaryHelper();
        ///
        ///     if (some_condition_based_on_fixed_parameters)
        ///     {
        ///         dictionaryHelper.Add(
        ///             new DynamicParameterBuilder("MyMandatoryOneTwoParameter")
        ///                .WithMandatory()
        ///                .WithValidateSet("One", "Two")
        ///             );
        ///     }
        ///
        ///     return (RuntimeDefinedParameterDictionary)dictionaryHelper;
        /// }
        /// </code>
        /// </example>
        public void Add(RuntimeDefinedParameterBuilder parameterBuilder)
        {
            this.Add(parameterBuilder.Build());
        }

        /// <summary>
        /// Adds an existing dynamic parameter to the dictionary.
        /// </summary>
        /// <param name="parameter">The parameter to add.</param>
        public void Add(RuntimeDefinedParameter parameter)
        {
            this.DynamicParameters.Add(parameter.Name, parameter);
        }
    }
}