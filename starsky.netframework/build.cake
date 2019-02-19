// CAKE FILE

// powershell -File build.ps1 -ScriptArgs '-Configuration="Release"'
// ./build.sh
// or: ./build.sh -Target="Default"


// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

Information($"Running target {target} in configuration {configuration}");

var runtime = "netframework-msbuild";
var distDirectory = Directory($"./{runtime}");

var projectNames = new List<string>{
    "starskyimportercliNetFramework",
    "starskyNetFrameworkShared",
    "starskySyncNetFramework"
};

var solutionName = "./starsky.netFramework.sln";

// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")
    .Does(() =>
    {

        if (FileExists($"starsky.netframework-msbuild.zip"))
        {
            DeleteFile($"sstarsky.netframework-msbuild.zip");
        }

        foreach (var projectName in projectNames)
        {
            var binReleaseDir = MakeAbsolute(Directory($"./{projectName}/bin/{configuration}"));
            System.Console.WriteLine($"{binReleaseDir}");
            CleanDirectory(binReleaseDir);
        }


        CleanDirectory(distDirectory);
    });

// Run dotnet restore to restore all package references.
Task("Restore")
    .Does(() =>
    {
        NuGetRestore(solutionName);
    });

// Build using the build configuration specified as an argument.
 Task("Build")
    .Does(() =>
    {
        MSBuild(solutionName, settings =>
            settings.SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "False")
                .SetVerbosity(Verbosity.Minimal)
                .AddFileLogger());
    });

// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("Test")
    .Does(() =>
    {
        Information("Testing disabled");
    });

Task("CoverageReport")
    .Does(() =>
    {
        Information("Testing disabled");
    });

// Publish the app to the /dist folder
Task("PublishWeb")
    .Does(() =>
    {
        foreach (var projectName in projectNames)
        {
            var binReleaseDir = MakeAbsolute(Directory($"./{projectName}/bin/{configuration}/*")).ToString();
            var distDirectoryAbsolute = MakeAbsolute(Directory($"./{distDirectory}")).ToString();

            System.Console.WriteLine($"{binReleaseDir}");
            System.Console.WriteLine($"{distDirectory}");

            // Child directories are ignored
            CopyFiles(binReleaseDir, distDirectoryAbsolute, true); //<= true = fake news
        }
    });

Task("Zip")
    .Does(() =>
    {
        System.Console.WriteLine($"./{distDirectory}", $"starsky-{distDirectory}.zip");
        Zip($"./{distDirectory}", $"starsky-{distDirectory}.zip");

    });

// A meta-task that runs all the steps to Build and Test the app
Task("BuildAndTest")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test") //<= disabled for now
    .IsDependentOn("CoverageReport"); //<= disabled for now

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("Zip");



// Executes the task specified in the target argument.
RunTarget(target);
