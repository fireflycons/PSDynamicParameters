#addin nuget:?package=Newtonsoft.Json&version=12.0.3

using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

var target = Argument("target", "Default");
var serveDocs = Argument<bool>("serveDocs", false);
var isAppveyor = Environment.GetEnvironmentVariables().Keys.Cast<string>().Any(k => k.StartsWith("APPVEYOR_", StringComparison.OrdinalIgnoreCase));
var isReleasePublication = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" && Environment.GetEnvironmentVariable("APPVEYOR_REPO_TAG ") == "true";

var docFxConfig = File("../docfx/docfx.json");
DirectoryPath docFxSite;

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

Task("InitDocumentation")
    .Does(() => {
        using (System.IO.StreamReader reader = System.IO.File.OpenText(docFxConfig))
        {
            JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

            var site = o["build"]["dest"];

            docFxSite = ((FilePath)docFxConfig).GetDirectory().Combine(Directory(site.ToString()));
        }
    });

Task("CompileDocumentation")
    .Does(() => {

        if (DirectoryExists(docFxSite))
        {
            DeleteDirectory(docFxSite.ToString(), new DeleteDirectorySettings {
                Force = true,
                Recursive = true
            });
        }

        RunDocFX(docFxConfig, isAppveyor ? false : serveDocs);
    });

Task("CopyDocumentationToRepo")
    .WithCriteria(isAppveyor)
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
    .IsDependentOn("InitDocumentation")
    .IsDependentOn("CompileDocumentation")
    .IsDependentOn("CopyDocumentationToRepo");

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

void RunDocFX(FilePath config, bool serve)
{
    var sb = new StringBuilder();

    sb.Append($"\"{config}\" --force");

    if (serve)
    {
        sb.Append(" --serve");
    }

    using (var process = new Process {
        StartInfo = new ProcessStartInfo {

            FileName = "docfx.exe",
            Arguments = sb.ToString(),
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