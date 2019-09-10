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

// Get Git info
#addin nuget:?package=Cake.Git

// For NPM
#addin "Cake.Npm"


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

// Running Client Build
Task("ClientRestore")
    .Does(() =>
    {
        Environment.SetEnvironmentVariable("CI","true");
        Environment.SetEnvironmentVariable("DISABLE_OPENCOLLECTIVE","true"); // core-js
        if (!DirectoryExists($"./starsky/clientapp/node_modules/react"))
        {
            // Running `npm ci` instead of `npm install`
            Information("npm ci restore for ./starsky/clientapp");
            NpmCi(s => s.FromPath("./starsky/clientapp"));
        }
        else {
            Information("Restore skipped for ./starsky/clientapp");
        }
  });

Task("ClientBuild")
    .Does(() =>
    {
        Environment.SetEnvironmentVariable("CI","false");
        NpmRunScript("build", s => s.FromPath("./starsky/clientapp/"));
  });

Task("ClientTest")
    .Does(() =>
    {
        NpmRunScript("test:ci", s => s.FromPath("./starsky/clientapp/"));
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
                                             .Append("/p:CoverletOutput=\"netcore-coverage.opencover.xml\"")
                });

            // Check if there is any output
            string parent = System.IO.Directory.GetParent(project.ToString()).FullName;
            string coverageFile = System.IO.Path.Combine(parent, "netcore-coverage.opencover.xml");

            Information("CoverageFile " + coverageFile);

            if (!FileExists(coverageFile)) {
                throw new Exception("CoverageFile missing " + coverageFile);
            }
        }
    });


Task("MergeCoverageFiles")
  .Does(() => {

    var outputCoverageFile = $"./starskytest/coverage-merge-cobertura.xml";

    if (FileExists(outputCoverageFile)) {
      DeleteFile(outputCoverageFile);
    }

    // Gets the coverage file from the client folder
    if (FileExists($"./starsky/clientapp/coverage/cobertura-coverage.xml")) {
        CopyFile($"./starsky/clientapp/coverage/cobertura-coverage.xml", $"./starskytest/jest-coverage.cobertura.xml");
    }

    // Merge all cobertura files
    ReportGenerator($"./starskytest/*coverage.*.xml", $"./starskytest/", new ReportGeneratorSettings{
        ReportTypes = new[] { ReportGeneratorReportType.Cobertura }
    });

    // And rename it
    MoveFile($"./starskytest/Cobertura.xml", outputCoverageFile);


  });


Task("CoverageReport")
    .Does(() =>
    {
        var projects = GetFiles("./*test/coverage-merge-cobertura.xml");
        foreach(var project in projects)
        {
            Information("CoverageReport project " + project);
            // Generate html files for reports
            var reportFolder = project.ToString().Replace("merge-cobertura.xml","report");
            ReportGenerator(project, reportFolder, new ReportGeneratorSettings{
                ReportTypes = new[] { ReportGeneratorReportType.HtmlInline }
            });
            // Zip entire folder
            Zip(reportFolder, $"{reportFolder}.zip");
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
        // for generic projects
        System.Console.WriteLine($"./{genericDistDirectory}", $"starsky-{genericDistDirectory}.zip");
        Zip($"./{genericDistDirectory}", $"starsky-{genericDistDirectory}.zip");

        if(runtime == genericName) return;
        // for runtime projects e.g. linux-arm or osx.10.12-x64

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
        string netCoreCoverageFile = System.IO.Path.Combine(firstTestProject, "netcore-coverage.opencover.xml");

        // get jest
        var clientAppProject = GetDirectories("./starsky/clientapp/").FirstOrDefault().ToString();
        string jestCoverageFile = System.IO.Path.Combine(clientAppProject, "coverage", "lcov.info");

        // Current branch name
        string parent = System.IO.Directory.GetParent(".").FullName;
        var gitBranch = GitBranchCurrent(parent);
        var branchName = gitBranch.FriendlyName;
        if(branchName == "(no branch)") branchName = "master";

        /* branchName = "master"; */

        SonarBegin(new SonarBeginSettings{
            Name = "Starsky",
            Key = key,
            Login = login,
            Verbose = false,
            Url = url,
            Branch = branchName,
            UseCoreClr = true,
            TypescriptCoverageReportsPath = jestCoverageFile,
            OpenCoverReportsPath = netCoreCoverageFile,
            ArgumentCustomization = args => args
                .Append($"/o:" + organisation)
                .Append($"/d:sonar.coverage.exclusions=\"*wwwroot/js/*,starskycore/Migrations/*,*spec.tsx,*/src/index.tsx\"")
                .Append($"/d:sonar.exclusions=\"wwwroot/js/*,starskycore/Migrations/*,*spec.tsx,*/src/index.tsx\"")
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

// React app build steps
Task("Client")
  .IsDependentOn("ClientRestore")
  .IsDependentOn("ClientBuild")
  .IsDependentOn("ClientTest");

// A meta-task that runs all the steps to Build and Test the app
Task("BuildNetCoreAndTest")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")
    .IsDependentOn("Client")
    .IsDependentOn("SonarBegin")
    .IsDependentOn("BuildNetCoreAndTest")
    .IsDependentOn("SonarEnd")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("MergeCoverageFiles")
    .IsDependentOn("CoverageReport")
    .IsDependentOn("Zip");


// Run only Starsky MVC and tests
Task("CI")
    .IsDependentOn("Client")
    .IsDependentOn("PrepStarskyOnly")
    .IsDependentOn("BuildNetCoreAndTest")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("Zip");



// Executes the task specified in the target argument.
RunTarget(target);
