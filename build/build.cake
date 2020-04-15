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

        // ArgumentCustomization = args=>args.Append("-StorePasswordInClearText")
        foreach (var fw in new [] { "net452", "netcoreapp2.1", "netcoreapp3.1"})
        {
            var settings = new DotNetCoreTestSettings
            {
                Configuration = "Release",
                NoBuild = true,
                Framework = fw
            };

            if (isAppveyor)
            {
                settings.ArgumentCustomization = args => args.Append("-appveyor");
            }

            DotNetCoreTest("../tests/Firefly.PowerShell.DynamicParameters.Tests/Firefly.PowerShell.DynamicParameters.Tests.csproj", settings);
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
