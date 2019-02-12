
// powershell -File build.ps1 -ScriptArgs '-runtime="osx.10.12-x64"'
// ./build.sh --runtime="osx.10.12-x64"

// Windows 32 bits: 'win7-x86'
// Mac: 'osx.10.12-x64'
// Raspberry Pi: 'linux-arm'


// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");
var runtime = Argument("runtime", "generic-netcore");


Information($"Running target {target} in configuration {configuration}");
Information($"\n>> Try to build on {runtime}");

if(runtime == null || runtime == "" ) runtime = "generic-netcore";
var distDirectory = Directory($"./{runtime}");

var projectNames = new List<string>{
    "starskygeocli",
    "starskyimportercli",
    "starskysynccli",
    "starskywebftpcli",
    "starskywebhtmlcli",
    "starsky"
}; // ignore starskycore


var testProjectNames = new List<string>{
    "starskyTests"
};


// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")
    .Does(() =>
    {
        CleanDirectory(distDirectory);
    });

// Run dotnet restore to restore all package references.
Task("Restore")
    .Does(() =>
    {

        // make a new list
        var restoreProjectNames = new List<string>(projectNames);
        projectNames.AddRange(testProjectNames);

        // now restore test with generic settings (always)
        foreach(var projectName in restoreProjectNames)
        {
            System.Console.WriteLine($"./{projectName}/{projectName}.csproj");
            DotNetCoreRestore($"./{projectName}/{projectName}.csproj",
                new DotNetCoreRestoreSettings());
        }

        if(runtime == "generic-netcore") return;

        System.Console.WriteLine($"> restore for {runtime}");

        var dotnetRestoreSettings = new DotNetCoreRestoreSettings{
            Runtime = runtime
        };

        foreach(var projectName in projectNames)
        {
            System.Console.WriteLine($"./{projectName}/{projectName}.csproj");
            DotNetCoreRestore($"./{projectName}/{projectName}.csproj",
                dotnetRestoreSettings);
        }


    });

// Build using the build configuration specified as an argument.
 Task("Build")
    .Does(() =>
    {
        var dotnetBuildSettings = new DotNetCoreBuildSettings()
        {
            Configuration = configuration,
            ArgumentCustomization = args => args.Append("--no-restore"),
        };

        // generic build for mstest
        DotNetCoreBuild(".",
            dotnetBuildSettings);

        // rebuild for specific target
        if(runtime != "generic-netcore") {
            dotnetBuildSettings.Runtime = runtime;
            DotNetCoreBuild(".",
                dotnetBuildSettings);
        }


    });

// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("Test")
    .Does(() =>
    {
        var projects = GetFiles("./*Tests/*.csproj");
        foreach(var project in projects)
        {
            Information("Testing project " + project);
            DotNetCoreTest(
                project.ToString(),
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true,
                    ArgumentCustomization = args => args.Append("--no-restore")
                                             .Append("/p:CollectCoverage=true")
                                             .Append("/p:CoverletOutputFormat=cobertura")
                                             .Append("/p:ThresholdType=line")
                                             .Append("/p:hideMigrations=\"true\"")
                                             .Append($"/p:CoverletOutput='../{runtime}/coverage.cobertura.xml'")
                });
        }
    });

// Publish the app to the /dist folder
Task("PublishWeb")
    .Does(() =>
    {
        foreach (var projectName in projectNames)
        {
            System.Console.WriteLine($"./{projectName}/{projectName}.csproj");

            var dotnetPublishSettings = new DotNetCorePublishSettings()
            {
                Configuration = configuration,
                OutputDirectory = distDirectory,
                ArgumentCustomization = args => args.Append("--no-restore"),
            };

            if(runtime != "generic-netcore") {
                dotnetPublishSettings.Runtime = runtime;
            }

            DotNetCorePublish(
                $"./{projectName}/{projectName}.csproj",
                dotnetPublishSettings
            );
        }

    });

// A meta-task that runs all the steps to Build and Test the app
Task("BuildAndTest")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("PublishWeb");

// Executes the task specified in the target argument.
RunTarget(target);
