#addin nuget:?package=Newtonsoft.Json&version=12.0.3

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var serveDocs = Argument<bool>("serveDocs", false);
var isAppveyor = EnvironmentVariable("APPVEYOR") != null;
var isReleasePublication = isAppveyor && EnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" && EnvironmentVariable("APPVEYOR_REPO_TAG ") == "true";
var canPublishDocs = isAppveyor && EnvironmentVariable<string>("APPVEYOR_REPO_NAME", "none").StartsWith("fireflycons/");
var docFxConfig = File("../docfx/docfx.json");
var testResultsDir = Directory(EnvironmentVariable<string>("APPVEYOR_BUILD_FOLDER", "..")) + Directory("test-output");

DirectoryPath docFxSite;

Task("BuildOnWindows")
    .WithCriteria(IsRunningOnWindows())
    .Does(() => {

        DotNetCoreBuild("../Firefly.PowerShell.DynamicParameters.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration
        });
    });

Task("BuildOnLinux")
    .WithCriteria(IsRunningOnUnix())
    .Does(() => {

        foreach (var framework in new [] { "netcoreapp2.1", "netcoreapp3.1"})
        {
            Information($"Building {framework}");

            DotNetCoreBuild("../Firefly.PowerShell.DynamicParameters.sln", new DotNetCoreBuildSettings
            {
                Configuration = configuration,
                Framework = framework
            });
        }
    });

Task("Build")
    .IsDependentOn("BuildOnWindows")
    .IsDependentOn("BuildOnLinux");


Task("TestOnWindows")
    .WithCriteria(IsRunningOnWindows())
    .Does(() => {

        try
        {
            DotNetCoreTest("../tests/Firefly.PowerShell.DynamicParameters.Tests/Firefly.PowerShell.DynamicParameters.Tests.csproj", new DotNetCoreTestSettings
            {
                Configuration = configuration,
                NoBuild = true,
                Logger = "trx",
                ResultsDirectory = testResultsDir
            });
        }
        finally
        {
            UploadTestResults();
        }
    });

Task("TestOnLinux")
    .WithCriteria(IsRunningOnUnix())
    .Does(() => {

        try
        {
            foreach (var framework in new [] { "netcoreapp2.1", "netcoreapp3.1"})
            {
                Information($"Testing {framework}");

                DotNetCoreTest("../tests/Firefly.PowerShell.DynamicParameters.Tests/Firefly.PowerShell.DynamicParameters.Tests.csproj", new DotNetCoreTestSettings
                {
                    Configuration = configuration,
                    NoBuild = true,
                    Framework = framework,
                    Logger = "trx",
                    ResultsDirectory = testResultsDir
                });
            }
        }
        finally
        {
            UploadTestResults();
        }
    });

Task("Test")
    .IsDependentOn("TestOnWindows")
    .IsDependentOn("TestOnLinux");

Task("CompileDocumentation")
    .WithCriteria(IsRunningOnWindows())
    .Does(() => {

        using (System.IO.StreamReader reader = System.IO.File.OpenText(docFxConfig))
        {
            var jsonConfig = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            var site = jsonConfig["build"]["dest"].Value<string>();

            docFxSite = ((FilePath)docFxConfig).GetDirectory().Combine(Directory(site));
        }

        if (DirectoryExists(docFxSite))
        {
            DeleteDirectory(docFxSite, new DeleteDirectorySettings {
                Force = true,
                Recursive = true
            });
        }

        RunDocFX(docFxConfig, isAppveyor ? false : serveDocs);
    });

Task("CopyDocumentationTo-github.io-clone")
    .WithCriteria(IsRunningOnWindows())
    .WithCriteria(canPublishDocs)
    .Does(() => {

        var outputDir = MakeAbsolute(Directory(System.IO.Path.Combine(EnvironmentVariableStrict("APPVEYOR_BUILD_FOLDER"), "..", "fireflycons.github.io", "PSDynamicParameters")));

        Information($"Updating documentation in {outputDir}");

        if (DirectoryExists(outputDir))
        {
            DeleteDirectory(outputDir.ToString(), new DeleteDirectorySettings {
                Force = true,
                Recursive = true
            });
        }

        CopyDirectory(docFxSite, outputDir);
    });

Task("BuildDocumentation")
    .IsDependentOn("CompileDocumentation")
    .IsDependentOn("CopyDocumentationTo-github.io-clone");

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("BuildDocumentation");

RunTarget(target);


#region Helper Functions

void UploadTestResults()
{
    if (!isAppveyor)
    {
        return;
    }

    using (var wc = new WebClient())
    {
        foreach(var result in GetFiles(testResultsDir + File("*.trx")))
        {
            wc.UploadFile($"https://ci.appveyor.com/api/testresults/mstest/{EnvironmentVariableStrict("APPVEYOR_JOB_ID")}", result.ToString());
        }
    }
}

string EnvironmentVariableStrict(string name)
{
    var val = EnvironmentVariable(name);

    if (string.IsNullOrEmpty(val))
    {
        throw new CakeException($"Required environment variable '{name}' not set.");
    }

    return val;
}

void RunDocFX(FilePath config, bool serve)
{
    var sb = new StringBuilder();

    sb.Append($"\"{config}\" --force");

    if (serve)
    {
        sb.Append(" --serve");
    }

    RunExternalProcess("docfx.exe", sb.ToString());
}

void RunExternalProcess(string executable, string arguments)
{
    using (var process = new Process {
        StartInfo = new ProcessStartInfo {

            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    })
    {
        process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                Information(e.Data);
            }
        });

        process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                Error(e.Data);
            }
        });

        process.Start();

        // Asynchronously read the standard output of the spawned process.
        // This raises OutputDataReceived events for each line of output.
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"docfx exited with code {process.ExitCode}");
        }
    }
}

#endregion