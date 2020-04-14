#addin nuget:?package=Cake.DocFx&version=0.13.1

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

        DotNetCoreTest("../tests/Firefly.PowerShell.DynamicParameters.Tests/Firefly.PowerShell.DynamicParameters.Tests.csproj", new DotNetCoreTestSettings
        {
            Configuration = "Release",
            Framework = "netcoreapp2.1"
        });

        DotNetCoreTest("../tests/Firefly.PowerShell.DynamicParameters.Tests/Firefly.PowerShell.DynamicParameters.Tests.csproj", new DotNetCoreTestSettings
        {
            Configuration = "Release",
            Framework = "netcoreapp3.1"
        });
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
