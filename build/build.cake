#addin nuget:?package=Cake.DocFx&version=0.13.1

using System.Net;

var target = Argument("target", "Default");
var isAppveyor = Environment.GetEnvironmentVariables().Keys.Cast<string>().Any(k => k.StartsWith("APPVEYOR_", StringComparison.OrdinalIgnoreCase));
var isReleasePublication = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" && Environment.GetEnvironmentVariable("APPVEYOR_REPO_TAG ") == "true";

Task("Build")
    .Does(() => {

        DotNetCoreBuild("../Firefly.PowerShell.DynamicParameters.sln", new DotNetCoreBuildSettings
        {
            Configuration = "Release"
        });

    });


Task("Test")
    .Does(() => {

        var resultsDir = Directory(System.IO.Path.Combine(EnvironmentVariableOrDefault("APPVEYOR_BUILD_FOLDER", ".."), "test-output"));
        try
        {
            // ArgumentCustomization = args=>args.Append("-StorePasswordInClearText")
            foreach (var fw in new [] { "net452", "netcoreapp2.1", "netcoreapp3.1"})
            {
                var settings = new DotNetCoreTestSettings
                {
                    Configuration = "Release",
                    NoBuild = true,
                    Framework = fw,
                    Logger = "trx",
                    ResultsDirectory = resultsDir
                };

                DotNetCoreTest("../tests/Firefly.PowerShell.DynamicParameters.Tests/Firefly.PowerShell.DynamicParameters.Tests.csproj", settings);
            }
        }
        finally
        {
            if (isAppveyor)
            {
                using (var wc = new WebClient())
                {
                    foreach(var result in GetFiles(resultsDir + File("*.trx")))
                    {
                        wc.UploadFile($"https://ci.appveyor.com/api/testresults/mstest/{EnvironmentVariableStrict("APPVEYOR_JOB_ID")}", result.ToString());
                    }
                }
            }
        }
    });

Task("BuildDocumentation")
    .Does(() => {

        DocFxBuild("../docfx/docfx.json", new DocFxBuildSettings {
            Serve = false,
            Force = true
        });
    });

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("BuildDocumentation");

RunTarget(target);


string EnvironmentVariableStrict(string name)
{
    var val = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrEmpty(val))
    {
        throw new CakeException($"Variable '{name}' not found");
    }

    return val;
}

string EnvironmentVariableOrDefault(string name, string defaultValue)
{
    return Environment.GetEnvironmentVariable(name) ?? defaultValue;
}