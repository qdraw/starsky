// CAKE FILE

// powershell -File build.ps1 -ScriptArgs '-runtime="osx.10.12-x64"'
// ./build.sh --runtime="osx.10.12-x64"
// or: ./build.sh -Target="CI"

// Windows 32 bits: 'win7-x86'
// Mac: 'osx.10.12-x64'
// Raspberry Pi: 'linux-arm'

// For the step CoverageReport
#tool "nuget:?package=ReportGenerator"

// SonarQube
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool
#addin nuget:?package=Cake.Sonar

// Cake.OpenCoverToCoberturaConverter
#addin "nuget:?package=Cake.OpenCoverToCoberturaConverter"
#tool "nuget:?package=OpenCoverToCoberturaConverter"

// Get Git info
#addin nuget:?package=Cake.Git

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
        var projects = GetFiles("./*test/*.csproj");
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
                                             .Append("/p:CoverletOutputFormat=opencover")
                                             .Append("/p:ThresholdType=line")
                                             .Append("/p:hideMigrations=\"true\"")
                                             .Append("/p:Exclude=\"[starsky.Views]*\"")
                                             .Append("/p:ExcludeByFile=\"../starskycore/Migrations/*\"") // (, comma seperated)
                                             .Append("/p:CoverletOutput=\"coverage.opencover.xml\"")
                });

            // Check if there is any output
            string parent = System.IO.Directory.GetParent(project.ToString()).FullName;
            string coverageFile = System.IO.Path.Combine(parent, "coverage.opencover.xml");

            Information("CoverageFile " + coverageFile);

            if (!FileExists(coverageFile)) {
                throw new Exception("CoverageFile missing " + coverageFile); 
            }
        }
    });


Task("OpenCoverToCobertura")
  .Does(() => {
        var projects = GetFiles("./*test/*.csproj");
        foreach(var project in projects)
        {
            // Check if there is any output
            string parent = System.IO.Directory.GetParent(project.ToString()).FullName;
            string inputCoverageFile = System.IO.Path.Combine(parent, "coverage.opencover.xml");
            string outputCoverageFile = System.IO.Path.Combine(parent, "coverage.cobertura.xml");

            Information("inputCoverageFile " + inputCoverageFile);
            Information("outputCoverageFile " + outputCoverageFile);
            OpenCoverToCoberturaConverter(inputCoverageFile, outputCoverageFile);
        }
  });


Task("CoverageReport")
    .Does(() =>
    {
        var projects = GetFiles("./*test/coverage.opencover.xml");
        foreach(var project in projects)
        {
            Information("CoverageReport project " + project);
            var reportFolder = project.ToString().Replace("opencover.xml","report");
            ReportGenerator(project, reportFolder, new ReportGeneratorSettings{
                ReportTypes = new[] { ReportGeneratorReportType.HtmlInline }
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
            if(runtime == genericName) continue;

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

Task("SonarBegin")
   .Does(() => {
        var key = EnvironmentVariable("STARSKY_SONAR_KEY");
        var login = EnvironmentVariable("STARSKY_SONAR_LOGIN");
        var organisation = EnvironmentVariable("STARSKY_SONAR_ORGANISATION");

        var url = EnvironmentVariable("STARSKY_SONAR_URL");
        if(string.IsNullOrEmpty(url)) {
            url = "https://sonarcloud.io";
        }

        if( string.IsNullOrEmpty(key) || string.IsNullOrEmpty(login) || string.IsNullOrEmpty(organisation) ) {
            Information($">> SonarQube is disabled $ key={key}|login={login}|organisation={organisation}");
            return;
        }

        // get first test project
        var firstTestProject = GetDirectories("./*test").FirstOrDefault().ToString();
        string coverageFile = System.IO.Path.Combine(firstTestProject, "coverage.opencover.xml");

        // Current branch name
        string parent = System.IO.Directory.GetParent(".").FullName;
        var gitBranch = GitBranchCurrent(parent);
        var branchName = gitBranch.FriendlyName;
        if(branchName == "(no branch)") branchName = "master";

        SonarBegin(new SonarBeginSettings{
            Name = "Starsky",
            Key = key,
            Login = login,
            Verbose = false,
            Url = url,
            Branch = branchName,
            OpenCoverReportsPath = coverageFile,
            ArgumentCustomization = args => args
                .Append($"/o:" + organisation),
                .Append($"/d:sonar.coverage.exclusions=\"**Tests*.cs,**Migrations*\"")
                .Append($"/d:sonar.exclusions=\"**Tests*.cs,**Migrations*\"")
        });

  });
Task("SonarEnd")
  .Does(() => {
    var login = EnvironmentVariable("STARSKY_SONAR_LOGIN");
    if( string.IsNullOrEmpty(login) ) {
        Information($">> SonarQube is disabled $ login={login}");
        return;
    }
    SonarEnd(new SonarEndSettings { 
        Login = login,
        Silent = true,
    });
  });

// A meta-task that runs all the steps to Build and Test the app
Task("BuildAndTest")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("CoverageReport");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")
    .IsDependentOn("SonarBegin")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("SonarEnd")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("OpenCoverToCobertura")
    .IsDependentOn("Zip");


// Run only Starsky MVC and tests
Task("CI")
    .IsDependentOn("PrepStarskyOnly")
    .IsDependentOn("BuildAndTest")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("Zip");



// Executes the task specified in the target argument.
RunTarget(target);
