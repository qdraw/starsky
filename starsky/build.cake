// CAKE FILE - a C# make file

/*
powershell -File build.ps1 -ScriptArgs '-runtime="osx.10.12-x64"'
./build.sh --runtime="osx.10.12-x64"
// or:
 ./build.sh --runtime="linux-arm,linux-arm64"

Windows 32 bits: 'win7-x86'
Mac: 'osx.10.12-x64'
Raspberry Pi: 'linux-arm'
ARM64: 'linux-arm64'
*/

// For the step CoverageReport
#tool "nuget:?package=ReportGenerator&version=4.3.6"

// SonarQube
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.6.0
#addin nuget:?package=Cake.Sonar&version=1.1.22

// Get Git info
#addin nuget:?package=Cake.Git&version=0.21.0

// For NPM
#addin "Cake.Npm&version=0.17.0"

// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

var genericName = "generic-netcore";
var runtimeInput = Argument("runtime", genericName);

/* to get a list with the generic item */
var runtimes = runtimeInput.Split(",").ToList();
if(!runtimes.Contains(genericName)) {
  // always the first item
  runtimes.Insert(0, genericName);
}

/* Build information, just to show */
var buildForInformation = new StringBuilder(">> Going to build for: ");
foreach(var runtime in runtimes)
{
  buildForInformation.Append($"{runtime} - ");
}
System.Console.WriteLine(buildForInformation.ToString());
/* done, build info*/

Information($"Running target {target} in configuration {configuration}");

var projectNames = new List<string>{
    "starskygeocli",
    "starskyimportercli",
    "starskysynccli",
    "starskywebftpcli",
    "starskywebhtmlcli",
    "starsky"
}; // ignore starskycore + starskygeocore


var testProjectNames = new List<string>{
    "starskytest"
};

// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")
    .Does(() =>
    {
        foreach(var runtime in runtimes)
        {
            if (FileExists($"starsky-{runtime}.zip"))
            {
                DeleteFile($"starsky-{runtime}.zip");
            }
            var distDirectory = Directory($"./{runtime}");
            CleanDirectory(distDirectory);

            CleanDirectory($"obj/Release/netcoreapp3.0/{runtime}");
        }
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

// npm run build
Task("ClientBuild")
    .Does(() =>
    {
        Environment.SetEnvironmentVariable("CI","false");
        NpmRunScript("build", s => s.FromPath("./starsky/clientapp/"));
  });

// npm run test:ci
Task("ClientTest")
    .Does(() =>
    {
        NpmRunScript("test:ci", s => s.FromPath("./starsky/clientapp/"));
  });

// Run dotnet restore to restore all package references.
Task("Restore")
    .Does(() =>
    {
        Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT","true");
        Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE","1");

        // make a new list
        var restoreProjectNames = new List<string>(projectNames);
        restoreProjectNames.AddRange(testProjectNames);


        foreach(var runtime in runtimes)
        {
            if (runtime == genericName)
            {
              System.Console.WriteLine(genericName);

              DotNetCoreRestore(".",
                  new DotNetCoreRestoreSettings());
              continue;
            }

            var dotnetRestoreSettings = new DotNetCoreRestoreSettings{
                Runtime = runtime
            };

            foreach(var projectName in projectNames)
            {
                System.Console.WriteLine($"Restore ./{projectName}/{projectName}.csproj for {runtime}");
                DotNetCoreRestore($"./{projectName}/{projectName}.csproj",
                    dotnetRestoreSettings);

                // Copy for runtime
                CopyFile($"./{projectName}/obj/project.assets.json",  $"./{projectName}/obj/project.assets_{runtime}.json");
            }
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

        foreach(var runtime in runtimes)
        {
            if (runtime == genericName)
            {
              DotNetCoreBuild(".",
                  dotnetBuildSettings);
              continue;
            }

            dotnetBuildSettings.Runtime = runtime;

            foreach(var projectName in projectNames)
            {
              System.Console.WriteLine($"Build ./{projectName}/{projectName}.csproj for {runtime}");

              // Restore project assets file to match the right runtime   // Not needed for generic-netcore
              CopyFile($"./{projectName}/obj/project.assets_{runtime}.json", $"./{projectName}/obj/project.assets.json");

                DotNetCoreBuild($"./{projectName}/{projectName}.csproj",
                    dotnetBuildSettings);
            }
        }
    });

// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("TestNetCore")
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
                                             .Append("/p:Exclude=\"[MySqlConnector]*%2c[starsky.Views]*\"")
                                             .Append("/p:ExcludeByFile=\"*C:\\projects\\mysqlconnector\\src\\MySqlConnector*%2c../starskycore/Migrations/*\"") // (, comma seperated)
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

// Merge front-end and backend coverage files
Task("MergeCoverageFiles")
  .Does(() => {

    var outputCoverageFile = $"./starskytest/coverage-merge-cobertura.xml";

    if (FileExists(outputCoverageFile)) {
      DeleteFile(outputCoverageFile);
    }

    // Gets the coverage file from the client folder
    if (FileExists($"./starsky/clientapp/coverage/cobertura-coverage.xml")) {
        Information($"Copy ./starsky/clientapp/coverage/cobertura-coverage.xml ./starskytest/jest-coverage.cobertura.xml");
        CopyFile($"./starsky/clientapp/coverage/cobertura-coverage.xml", $"./starskytest/jest-coverage.cobertura.xml");
    }

    // Merge all cobertura files
    ReportGenerator($"./starskytest/*coverage.*.xml", $"./starskytest/", new ReportGeneratorSettings{
        ReportTypes = new[] { ReportGeneratorReportType.Cobertura }
    });

    // And rename it
    MoveFile($"./starskytest/Cobertura.xml", outputCoverageFile);
  });

// Create a nice report and zip it
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
        foreach(var projectName in projectNames)
        {
            foreach(var runtime in runtimes)
            {
                var distDirectory = Directory($"./{runtime}");

                var dotnetPublishSettings = new DotNetCorePublishSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = distDirectory, // <= first to generic
                    ArgumentCustomization = args => args.Append("--no-restore --no-build --force"),
                };

                if(runtime != genericName) {
                    dotnetPublishSettings.Runtime = runtime;

                    // Restore project assets file to match the right runtime   // Not needed for generic-netcore
                    CopyFile($"./{projectName}/obj/project.assets_{runtime}.json", $"./{projectName}/obj/project.assets.json");
                }

                System.Console.WriteLine($"Publish ./{projectName}/{projectName}.csproj for {runtime}");

                DotNetCorePublish(
                    $"./{projectName}/{projectName}.csproj",
                    dotnetPublishSettings
                );
            }
        }
    });

// zip the runtime folders
Task("Zip")
    .Does(() =>
    {
        foreach(var runtime in runtimes)
        {
            var distDirectory = Directory($"./{runtime}");
            System.Console.WriteLine($"./{distDirectory}", $"starsky-{distDirectory}.zip");
            Zip($"./{distDirectory}", $"starsky-{distDirectory}.zip");
        }
    });

// Start SonarQube, you must also end it
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
                .Append($"/d:sonar.coverage.exclusions=\"**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts,*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/starskycore/Migrations/*,**/*spec.ts,**/*spec.tsx,**/src/index.tsx\"")
                .Append($"/d:sonar.exclusions=\"**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts,*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/starskycore/Migrations/*,**/*spec.tsx,**/*spec.ts,**/src/index.tsx\"")
        });
  });

// End the task and send it SonarCloud
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
Task("BuildNetCore")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")
    /* .IsDependentOn("Client") */
    .IsDependentOn("SonarBegin")
    .IsDependentOn("BuildNetCore")
    /* .IsDependentOn("TestNetCore") */
    .IsDependentOn("SonarEnd")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("MergeCoverageFiles")
    .IsDependentOn("CoverageReport")
    .IsDependentOn("Zip");

// To get fast all (net core) assemblies
// ./build.sh --Runtime=osx.10.12-x64 --Target=BuildPublishWithoutTest
Task("BuildPublishWithoutTest")
    .IsDependentOn("BuildNetCore")
    .IsDependentOn("PublishWeb");


// Executes the task specified in the target argument.
RunTarget(target);
