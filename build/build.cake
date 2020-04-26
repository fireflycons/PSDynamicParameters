#addin nuget:?package=Newtonsoft.Json&version=12.0.3

using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var serveDocs = Argument<bool>("serveDocs", false);

var isAppveyor = EnvironmentVariable<bool>("APPVEYOR", false);
var projectRoot = Directory(EnvironmentVariable<string>("APPVEYOR_BUILD_FOLDER", ".."));
var isReleasePublication = isAppveyor && EnvironmentVariable("APPVEYOR_REPO_BRANCH") == "master" && EnvironmentVariable<bool>("APPVEYOR_REPO_TAG", false);
var canPublishDocs = isAppveyor && EnvironmentVariable<string>("APPVEYOR_REPO_NAME", "none").StartsWith("fireflycons/");
var docFxConfig = projectRoot + File("docfx/docfx.json");
var testResultsDir = Directory(EnvironmentVariable<string>("APPVEYOR_BUILD_FOLDER", "..")) + Directory("test-output");

FilePath mainProjectFile;
FilePath nugetPackagePath;
bool buildPackage;
string buildVersion;

DirectoryPath docFxSite;

Task("Init")
    .Does(() => {

        buildVersion = GetBuildVersion();

        foreach(var projectFile in GetFiles(projectRoot + File("**/*.csproj")))
        {
            var project = XElement.Load(projectFile.ToString());

            var packElem = project.Elements("PropertyGroup").Descendants("GeneratePackageOnBuild").FirstOrDefault();

            if (packElem != null)
            {
                mainProjectFile = MakeAbsolute(File(projectFile.ToString()));
                bool.TryParse(packElem.Value, out buildPackage);
            }
        }

        if (mainProjectFile == null)
        {
            throw new CakeException("Unable to locate main project file (i.e. the one that will create the nuget package)");
        }

        if (buildPackage)
        {
            nugetPackagePath = mainProjectFile.GetDirectory()
                .Combine(Directory($"bin/{configuration}"))
                + File($"/{mainProjectFile.GetFilenameWithoutExtension()}.{buildVersion}.nupkg");

            Information($"Package: {nugetPackagePath}");
        }
    });

Task("PrepareNugetProperties")
    .WithCriteria(IsRunningOnWindows())
    .Does(() => {

        var project = XElement.Load(mainProjectFile.ToString());
        var version = project.Elements("PropertyGroup").Descendants("Version").FirstOrDefault();

        if (version != null)
        {
            version.Value = buildVersion;
            project.Save(mainProjectFile.ToString());
        }
    });

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

Task("PushAppveyor")
    .WithCriteria(IsRunningOnWindows())
    .WithCriteria(isAppveyor)
    .Does(async () => {

        var packages = GetFiles($"../**/*.nupkg");

        // Push AppVeyor artifact
        Information($"Pushing {nugetPackagePath} as AppVeyor artifact");

        await UploadAppVeyorArtifact(nugetPackagePath);
    });

Task("PushNuget")
    .WithCriteria(isAppveyor)
    .WithCriteria(isReleasePublication)
    .WithCriteria(IsRunningOnWindows())
    .Does(() => {

        NuGetPush(nugetPackagePath, new NuGetPushSettings {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = EnvironmentVariable("NUGET_API_KEY")
            });
    });

Task("Build")
    .IsDependentOn("Init")
    .IsDependentOn("PrepareNugetProperties")
    .IsDependentOn("BuildOnWindows")
    .IsDependentOn("BuildOnLinux");

Task("Test")
    .IsDependentOn("Init")
    .IsDependentOn("TestOnWindows")
    .IsDependentOn("TestOnLinux");

Task("PushPackage")
    .IsDependentOn("Init")
    .IsDependentOn("PushAppveyor")
    .IsDependentOn("PushNuget");

Task("BuildDocumentation")
    .IsDependentOn("Init")
    .IsDependentOn("CompileDocumentation")
    .IsDependentOn("CopyDocumentationTo-github.io-clone");

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("BuildDocumentation")
    .IsDependentOn("PushPackage");

RunTarget(target);

#region Helper Functions

class AppveyorArtifactRequest
{
    public AppveyorArtifactRequest(FilePath artifact)
    {
        this.Path = artifact.ToString();
        this.FileName = artifact.GetFilename().ToString();
    }

    [JsonProperty("path")]
    public string Path { get; }

    [JsonProperty("fileName")]
    public string FileName { get; }

    [JsonProperty("name")]
    public string Name { get; } = null;

    [JsonProperty("type")]
    public string Type { get; } = "NuGetPackage";
}

class AppveyorArtifactResponse
{
    [JsonProperty("uploadUrl")]
    public string UploadUrl { get; set; }

    [JsonProperty("storageType")]
    public string StorageType { get; set; }
}

async Task UploadAppVeyorArtifact(FilePath artifact)
{
    if (!FileExists(artifact))
    {
        throw new FileNotFoundException(artifact.ToString());
    }

    var ub = new UriBuilder(EnvironmentVariableStrict("APPVEYOR_API_URL"));

    ub.Path = "/api/artifacts";

    var json = JsonConvert.SerializeObject(new AppveyorArtifactRequest(artifact));

    using (var client = new HttpClient())
    {
        string result = (await client.PostAsync(ub.Uri, new StringContent(json, Encoding.UTF8, "application/json"))).Content.ReadAsStringAsync().Result;

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new CakeException("No response from API");
        }

        var uploadDetails = JsonConvert.DeserializeObject<AppveyorArtifactResponse>(result);

        switch(uploadDetails.StorageType)
        {
            case "Azure":

                using (var data = new StreamContent(System.IO.File.OpenRead(artifact.ToString())))
                {
                    await client.PutAsync(uploadDetails.UploadUrl, data);
                }
                break;

            default:

                throw new CakeException ($"Unsupported storage Type: {uploadDetails.StorageType}");
        }
    }

    Information($"Uploaded '{artifact}' to Appveyor");
}

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

string GetBuildVersion()
{
    var tag = EnvironmentVariable("APPVEYOR_REPO_TAG_NAME");

    if (tag == null)
    {
        return EnvironmentVariable<string>("APPVEYOR_BUILD_VERSION", "0.0.1");;
    }

    var m = Regex.Match(tag, @"^v(?<version>\d+\.\d+\.\d+)$", RegexOptions.IgnoreCase);

    if (m.Success)
    {
        return m.Groups["version"].Value;
    }

    throw new CakeException($"Cannot determine version from tag: {tag}");
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