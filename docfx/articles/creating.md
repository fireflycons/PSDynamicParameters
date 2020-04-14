## Creating a Cmdlet with a Dynamic Parameter

In this simplified example based on the tests within this repo, we create a parameter based on the value of the fixed argument to the cmdlet.

```csharp
[Cmdlet(VerbsCommon.Add, "DynamicParameter")]
public class AddDynamicParameterCommand : PSCmdlet, IDynamicParameters
{
    /// <summary>
    /// Gets or sets the test to run
    /// This is a mandatory fixed parameter to the cmdlet.
    /// </summary>
    [Parameter(Mandatory = true)]
    public int TestNumber { get; set; }

    /// <summary>
    /// Gets the dynamic parameters.
    /// The parameter is created based on the value of
    /// <see cref="Constants.DynamicParameterName"/>
    /// </summary>
    /// <returns>A <see cref="RuntimeDefinedParameterDictionary"/></returns>
    public object GetDynamicParameters()
    {
        var dynamicParams = new RuntimeDefinedParameterDictionaryHelper();

        if (this.TestNumber == 1)
        {
            // Add a param "-MyNewParameter" which will be mandatory
            // with allowed values of "Yes" or "No"
            dynamicParams.Add(
                new DynamicParameterBuilder("MyNewParameter")
                    .WithMandatory()
                    .WithValidateSet("Yes", "No")
                );
        }

        // This must always be returned whether or not parameters were added.
        return (RuntimeDefinedParameterDictionary)dynamicParams;
    }

    /// <summary>
    /// Body of the cmdlet.
    /// </summary>
    protected override void ProcessRecord()
    {
        // This is $PSBoundParameters
        var boundParameters = this.MyInvocation.BoundParameters;

        if (boundParameters.ContainsKey("MyNewParameter"))
        {
            // The dynamic parameter is present - retrieve its value
            var myNewParameter = boundParameters["MyNewParameter"];

            // Do something with the parameter...
        }

        break;
    }
}

```