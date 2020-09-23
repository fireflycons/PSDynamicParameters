#addin nuget:?package=Newtonsoft.Json&version=12.0.3
#addin nuget:?package=Cake.Http&version=0.7.0

using System.Collections.Generic;
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
Version buildVersion;

DirectoryPath docFxSite;
var documentationPath = "PSDynamicParameters";
var documentationBaseUrl = $"https://firefly-cons.github.io/{documentationPath}";
var xrefMapServiceEndpoint = "https://xref.docs.firefly-consulting.co.uk";


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

Task("SetAssemblyProperties")
    .WithCriteria(IsRunningOnWindows())
    .Does(() => {

        // Only manipulate the nuget/assembly properties on Windows as it's this run that publishes to nuget.
        // Other platform builds to run tests only
        var project = XElement.Load(mainProjectFile.ToString());

        var propertyGroups = project.Elements("PropertyGroup").ToList();

        SetProjectProperty(propertyGroups, "Version", buildVersion.ToString());
        SetProjectProperty(propertyGroups, "AssemblyVersion", $"{buildVersion.ToString(2)}.0.0");
        SetProjectProperty(propertyGroups, "FileVersion", $"{buildVersion.ToString(3)}.0");

        project.Save(mainProjectFile.ToString());
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
    .WithCriteria(isReleasePublication)
    .Does(() => {

        var outputDir = MakeAbsolute(Directory(System.IO.Path.Combine(EnvironmentVariableStrict("APPVEYOR_BUILD_FOLDER"), "..", "fireflycons.github.io", documentationPath)));

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

Task("UploadXrefMap")
    .WithCriteria(IsRunningOnWindows())
    .WithCriteria(isReleasePublication || !string.IsNullOrEmpty(EnvironmentVariable("FORCE_XREF_PUSH")))
    .Does(() => {

        string apiKey;
        var xrefmap = docFxSite.CombineWithFilePath(new FilePath("xrefmap.yml"));

        if (!FileExists(xrefmap))
        {
            Warning($"Cannot find '{xrefmap}'");
            return;
        }

        if ((apiKey = EnvironmentVariable<string>("XREFMAP_APIKEY", null)) == null)
        {
            Warning("Cannot find environment variable XREFMAP_APIKEY");
            return;
        }

        // Here we assume that mainProjectFile filename is the root namespace of what we're publishing
        var ns = mainProjectFile.GetFilenameWithoutExtension().ToString();

        var json = JsonConvert.SerializeObject(new {
            baseUrl = documentationBaseUrl,
            xrefmap = System.IO.File.ReadAllText(xrefmap.ToString())
        });

        var pushUrl = $"{xrefMapServiceEndpoint.Trim('/')}/xrefmap/{ns}";
        Information($"Pushing XRefMap to {pushUrl}");

        var result = HttpPut(pushUrl, new HttpSettings {
            Headers = new Dictionary<string, string> {
                { "x-api-key", apiKey }
            },
            RequestBody = System.Text.Encoding.UTF8.GetBytes(json)
        });

        Information(result);
    });

Task("PushAppveyor")
    .WithCriteria(IsRunningOnWindows())
    .WithCriteria(isAppveyor)
    .Does(async () => {

        await UploadAppveyorArtifact(nugetPackagePath);
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
    .IsDependentOn("SetAssemblyProperties")
    .IsDependentOn("BuildOnWindows")
    .IsDependentOn("BuildOnLinux");

Task("Test")
    .IsDependentOn("Init")
    .IsDependentOn("TestOnWindows")
    .IsDependentOn("TestOnLinux");

Task("BuildDocumentation")
    .IsDependentOn("Init")
    .IsDependentOn("CompileDocumentation")
    .IsDependentOn("CopyDocumentationTo-github.io-clone")
    .IsDependentOn("UploadXrefMap");

Task("PushPackage")
    .IsDependentOn("Init")
    .IsDependentOn("PushAppveyor")
    .IsDependentOn("PushNuget");

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

    [JsonIgnore]
    public bool IsAzureOrGoogle
    {
        get
        {
            return this.StorageType == "Azure" || this.UploadUrl.Contains("google");
        }
    }
}

class AppVeyorArtifactFinalisation
{
    public AppVeyorArtifactFinalisation(FilePath artifact)
    {
        this.FileName = artifact.GetFilename().ToString();
        this.Size = new System.IO.FileInfo(artifact.ToString()).Length;
    }

    [JsonProperty("fileName")]
    public string FileName { get; }

    [JsonProperty("size")]
    public long Size { get; }
}

async Task UploadAppveyorArtifact(FilePath artifact)
{
    if (!FileExists(artifact))
    {
        throw new FileNotFoundException(artifact.ToString());
    }

    var appveyorApi = new UriBuilder(EnvironmentVariableStrict("APPVEYOR_API_URL"));

    appveyorApi.Path = "/api/artifacts";

    using (var client = new HttpClient())
    {
        var artifactRequest = new AppveyorArtifactRequest(artifact);

        // Request upload URL
        var uploadDetails = JsonConvert.DeserializeObject<AppveyorArtifactResponse>(
            (
                await client.PostAsync(appveyorApi.Uri, new StringContent(
                    JsonConvert.SerializeObject(artifactRequest),
                    Encoding.UTF8,
                    "application/json"
                    )
                )
            )
            .Content
            .ReadAsStringAsync()
            .Result
        );

        Information($"Uploading Appveyor artifact '{artifactRequest.FileName}' to {uploadDetails.StorageType}");

        if (uploadDetails.IsAzureOrGoogle)
        {
            // PUT to cloud storage
            using (var data = new StreamContent(System.IO.File.OpenRead(artifact.ToString())))
            {
                await client.PutAsync(uploadDetails.UploadUrl, data);
            }

            // Finalise request with Appveyor
            await client.PutAsync(appveyorApi.Uri, new StringContent(
                    JsonConvert.SerializeObject(new AppVeyorArtifactFinalisation(artifact)),
                    Encoding.UTF8,
                    "application/json"
                    )
                );
        }
        else
        {
            // direct to Appveyor
            using (var wc = new WebClient())
            {
                wc.UploadFile(uploadDetails.UploadUrl, artifact.ToString());
            }
        }
    }
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

void SetProjectProperty(IEnumerable<XElement>properties, string propertyName, string propertyValue)
{
    var elem = properties.Descendants(propertyName).FirstOrDefault();

    if (elem != null)
    {
        elem.Value = propertyValue;
    }
    else
    {
        properties.First().Add(new XElement(propertyName, propertyValue));
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

Version GetBuildVersion()
{
    var tag = EnvironmentVariable("APPVEYOR_REPO_TAG_NAME");

    if (tag == null)
    {
        return new Version(EnvironmentVariable<string>("APPVEYOR_BUILD_VERSION", "0.0.1"));
    }

    var m = Regex.Match(tag, @"^v(?<version>\d+\.\d+\.\d+)$", RegexOptions.IgnoreCase);

    if (m.Success)
    {
        return new Version(m.Groups["version"].Value);
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