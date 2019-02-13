
// powershell -File build.ps1 -ScriptArgs '-runtime="osx.10.12-x64"'
// ./build.sh --runtime="osx.10.12-x64"
// or: ./build.sh -Target="CI"

// Windows 32 bits: 'win7-x86'
// Mac: 'osx.10.12-x64'
// Raspberry Pi: 'linux-arm'


// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

var genericName = "generic-netcore";
var runtime = Argument("runtime", genericName);


Information($"Running target {target} in configuration {configuration}");
Information($"\n>> Try to build on {runtime}");

if(runtime == null || runtime == "" ) runtime = genericName;
var distDirectory = Directory($"./{runtime}");
var genericDistDirectory = Directory($"./{genericName}");

// output for CI build -- overwrite when needed
var distDirectoryStarskyOnly = Directory($"./{runtime}-starskyonly");
var genericDistDirectoryStarskyOnly = Directory($"./{genericName}-starskyonly");


var projectNames = new List<string>{
    "starskygeocli",
    "starskyimportercli",
    "starskysynccli",
    "starskywebftpcli",
    "starskywebhtmlcli",
    "starsky"
}; // ignore starskycore


var testProjectNames = new List<string>{
    "starskytest"
};

Task("PrepStarskyOnly")
    .Does(() =>
    {
        projectNames = new List<string>{"starsky"};
        distDirectory = distDirectoryStarskyOnly;
        genericDistDirectory = genericDistDirectoryStarskyOnly;
    });

// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")
    .Does(() =>
    {

        if (FileExists($"starsky-{genericDistDirectory}.zip"))
        {
            DeleteFile($"starsky-{genericDistDirectory}.zip");
        }

        if (FileExists($"starsky-{distDirectory}.zip"))
        {
            DeleteFile($"starsky-{distDirectory}.zip");
        }

        CleanDirectory(distDirectory);
        CleanDirectory(genericDistDirectory);
    });

// Run dotnet restore to restore all package references.
Task("Restore")
    .Does(() =>
    {

        // make a new list
        var restoreProjectNames = new List<string>(projectNames);
        restoreProjectNames.AddRange(testProjectNames);

        // now restore test with generic settings (always)
        // used to get all dependencies
        DotNetCoreRestore(".",
            new DotNetCoreRestoreSettings());

        /* -- foreach project --
        foreach(var projectName in restoreProjectNames)
        {
            System.Console.WriteLine($"./{projectName}/{projectName}.csproj");
            DotNetCoreRestore($"./{projectName}/{projectName}.csproj",
                new DotNetCoreRestoreSettings());
        } */

        if(runtime == genericName) return;

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

        System.Console.WriteLine($"> build: {runtime}");
        // generic build for mstest
        DotNetCoreBuild(".",
            dotnetBuildSettings);

        // rebuild for specific target
        if(runtime != genericName) {

            System.Console.WriteLine($"> rebuild for specific target {runtime}");
            dotnetBuildSettings.Runtime = runtime;

            foreach(var projectName in projectNames)
            {
                System.Console.WriteLine($"./{projectName}/{projectName}.csproj");
                DotNetCoreBuild($"./{projectName}/{projectName}.csproj",
                    dotnetBuildSettings);
            }

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
                                             .Append("/p:CoverletOutput=coverage.cobertura.xml")
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
                OutputDirectory = genericDistDirectory, // <= first to generic
                ArgumentCustomization = args => args.Append("--no-restore"),
            };

            // The items are already build {generic build}
            DotNetCorePublish(
                $"./{projectName}/{projectName}.csproj",
                dotnetPublishSettings
            );

            // also publish the other files for runtimes
            if(runtime == genericName) return;

            dotnetPublishSettings.Runtime = runtime;
            dotnetPublishSettings.OutputDirectory = distDirectory; // <= then to linux-arm

            DotNetCorePublish(
                $"./{projectName}/{projectName}.csproj",
                dotnetPublishSettings
            );

        }

    });

Task("Zip")
    .Does(() =>
    {
        System.Console.WriteLine($"./{genericDistDirectory}", $"starsky-{genericDistDirectory}.zip");
        Zip($"./{genericDistDirectory}", $"starsky-{genericDistDirectory}.zip");

        if(runtime == genericName) return;

        System.Console.WriteLine($"./{distDirectory}", $"starsky-{distDirectory}.zip");
        Zip($"./{distDirectory}", $"starsky-{distDirectory}.zip");

    });
/*
Task("ZipStarskyOnly")
    .Does(() =>
    {
        Zip($"./{genericDistDirectory}", $"starsky-{genericDistDirectory}-starskyonly.zip");

        if(runtime == genericName) return;
        Zip($"./{distDirectory}", $"starsky-{distDirectory}-starskyonly.zip");

    }); */

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
    .IsDependentOn("PublishWeb")
    .IsDependentOn("Zip");


// Run only Starsky MVC and tests
Task("CI")
    .IsDependentOn("PrepStarskyOnly")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("Zip");



// Executes the task specified in the target argument.
RunTarget(target);
