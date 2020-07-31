// CAKE FILE - a C# make file

/*
powershell -File build.ps1 -ScriptArgs '-runtime="osx.10.12-x64"'
./build.sh --runtime="osx.10.12-x64"
// or:
 ./build.sh --runtime="linux-arm,linux-arm64"

.\build.ps1 --Target=BuildTestOnlyNetCore

Windows 64 bits: 'win7-x64'
Mac: 'osx.10.12-x64'
ARM64: 'linux-arm64'
Raspberry Pi: 'linux-arm'
Windows 32 bits: 'win7-x86'
*/

// Get Git info
#addin nuget:?package=Cake.Git&version=0.22.0

// For NPM
#addin "Cake.Npm&version=0.17.0"

// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

var genericName = "generic-netcore";
var runtimeInput = Argument("runtime", genericName);
var branchName = Argument("branch", "");
var noSonar = HasArgument("no-sonar") || HasArgument("nosonar");

/* to get a list with the generic item */
var runtimes = runtimeInput.Split(",").ToList();
if(!runtimes.Contains(genericName)) {
  // always the first item
  runtimes.Insert(0, genericName);
}

Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT","true");
Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE","1");

/* Build information, just to show */
var buildForInformation = new StringBuilder(">> Going to build for: ");
foreach(var runtime in runtimes)
{
  buildForInformation.Append($"{runtime} - ");
}
System.Console.WriteLine(buildForInformation.ToString());
/* done, build info*/

Information($"Running target {target} in configuration {configuration}");
if(branchName != "") Information($"Using branchName overwrite: {branchName}");

var publishProjectNames = new List<string>{
    "starskyadmincli",
    "starskygeocli",
    "starskyimportercli",
    "starskysynccli",
    "starskywebftpcli",
    "starskywebhtmlcli",
    "starsky"
}; // ignore starskycore + features/foundations


var testProjectNames = new List<string>{
    "starskytest"
};

Task("TestEnv")
    .Does(() =>
    {
        // is allowed to write in a temp folder (used by coverlet)
        string systemTempPath = System.IO.Path.GetTempPath();
        if (!DirectoryExists(systemTempPath))
        {
            throw new Exception($"missing temp path {systemTempPath}");
        }
        else {
            var tempTestFile = System.IO.Path.Combine(systemTempPath, "__starsky.test");
            if (FileExists(tempTestFile))
            {
                DeleteFile(tempTestFile);
            }

            System.IO.File.Create(tempTestFile).Dispose();
            // if not it will fail

            Information($"{systemTempPath} exist");
        }

        // DotNet localTools need to be part of the Env Path
        var pathEnv = Environment.GetEnvironmentVariable("PATH").Contains($".dotnet{System.IO.Path.DirectorySeparatorChar}tools");
        if(!pathEnv) {
          throw new Exception($".dotnet{System.IO.Path.DirectorySeparatorChar}tools is not part of the path");
        }
    });

// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("CleanNetCore")
    .Does(() =>
    {
        Information("DotNetCoreClean for .");
        DotNetCoreClean(".");

        foreach(var runtime in runtimes)
        {
            if (FileExists($"starsky-{runtime}.zip"))
            {
                DeleteFile($"starsky-{runtime}.zip");
            }
            var distDirectory = Directory($"./{runtime}");
            CleanDirectory(distDirectory);

            CleanDirectory($"obj/Release/netcoreapp3.1/{runtime}");

        }
    });

// Running Client Build
Task("ClientRestore")
    .Does(() =>
    {
        var isProduction = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

        Information($">> Running Npm as IsProduction = {isProduction}");

        Environment.SetEnvironmentVariable("DISABLE_OPENCOLLECTIVE","true"); // core-js

        if (!DirectoryExists($"./starsky/clientapp/node_modules/react"))
        {
            Information("npm ci restore for ./starsky/clientapp");
            var settings =
                new NpmCiSettings
                {
                    Production = isProduction
                };
            settings.FromPath("./starsky/clientapp");
            NpmCi(settings);
        }
        else {
            Information("Restore skipped for ./starsky/clientapp");
        }
  });

// npm run build
Task("ClientBuild")
    .Does(() =>
    {
        /* with CI=true eslint errors will break the build */
        Environment.SetEnvironmentVariable("CI","true");
        NpmRunScript("build", s => s.FromPath("./starsky/clientapp/"));
  });

// npm run test:ci
Task("ClientTest")
    .Does(() =>
    {
        NpmRunScript("test:ci", s => s.FromPath("./starsky/clientapp/"));
  });

// Run dotnet restore to restore all package references.
Task("RestoreNetCore")
    .Does(() =>
    {
        Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT","true");
        Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE","1");

        // make a new list
        var restoreProjectNames = new List<string>(publishProjectNames);
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

            foreach(var projectName in publishProjectNames)
            {
                System.Console.WriteLine($"Restore ./{projectName}/{projectName}.csproj for {runtime}");
                DotNetCoreRestore($"./{projectName}/{projectName}.csproj",
                    dotnetRestoreSettings);

                // Copy for runtime
                CopyFile($"./{projectName}/obj/project.assets.json",  $"./{projectName}/obj/project.assets_{runtime}.json");
            }
        }
    });

// Build for Generic items
Task("BuildNetCoreGeneric")
  .Does(() =>
  {
      System.Console.WriteLine($"Build . for generic");
      var dotnetBuildSettings = new DotNetCoreBuildSettings()
      {
          Configuration = configuration,
          ArgumentCustomization = args => args.Append("--nologo").Append("--no-restore"),
          /* Verbosity = DotNetCoreVerbosity.Detailed */
      };
      DotNetCoreBuild(".",
          dotnetBuildSettings);
  });


// Build for non-generic builds
// Generic must build first
 Task("BuildNetCoreRuntimeSpecific")
    .Does(() =>
    {
        var dotnetBuildSettings = new DotNetCoreBuildSettings()
        {
            Configuration = configuration,
            ArgumentCustomization = args => args.Append("--nologo").Append("--no-restore"),
        };

        foreach(var runtime in runtimes)
        {
            if (runtime == genericName)
            {
              // see BuildNetCoreGeneric
              continue;
            }

            dotnetBuildSettings.Runtime = runtime;

            foreach(var projectName in publishProjectNames)
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

            string testParentPath = System.IO.Directory.GetParent(project.ToString()).FullName;

            /* clean test results */
            var testResultsFolder = System.IO.Path.Combine(testParentPath, "TestResults");
            if (DirectoryExists(testResultsFolder))
            {
                Information(">> Removing folder => " + testResultsFolder);
                DeleteDirectory(testResultsFolder, new DeleteDirectorySettings {
                    Recursive = true
                });
            }

            DotNetCoreTest(
                project.ToString(),
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true,
                    ArgumentCustomization = args => args
                          .Append("--no-restore")
                          .Append("--no-build")
                          .Append("--nologo")
                          .Append("--logger trx")
                          .Append("--collect:\"XPlat Code Coverage\"")
                          .Append("--settings build.vstest.runsettings")
                });

            var coverageEnum = GetFiles("./**/coverage.opencover.xml");

            // Get the FirstOrDefault() but there is no LINQ here
            var coverageFilePath =  System.IO.Path.Combine(testParentPath, "netcore-coverage.opencover.xml");
            foreach(var item in coverageEnum)
            {
              CopyFile(item.FullPath, coverageFilePath);
            }
            Information("CoverageFile " + coverageFilePath);

            if (!FileExists(coverageFilePath)) {
                throw new Exception("CoverageFile missing " + coverageFilePath);
            }
        }
    });

// Merge front-end and backend coverage files
Task("MergeCoverageFiles")
  .Does(() => {

    if (! FileExists($"./starsky/clientapp/coverage/cobertura-coverage.xml")) {
        throw new Exception($"Missing jest coverage file ./starsky/clientapp/coverage/cobertura-coverage.xml");
    }

    if (! FileExists("./starskytest/netcore-coverage.opencover.xml")) {
      throw new Exception($"Missing .NET Core coverage file ./starskytest/netcore-coverage.opencover.xml");
    }

    var outputCoverageFile = $"./starskytest/coverage-merge-cobertura.xml";

    if (FileExists(outputCoverageFile)) {
      DeleteFile(outputCoverageFile);
    }

    // Gets the coverage file from the client folder
    if (FileExists($"./starsky/clientapp/coverage/cobertura-coverage.xml")) {
        Information($"Copy ./starsky/clientapp/coverage/cobertura-coverage.xml ./starskytest/jest-coverage.cobertura.xml");
        CopyFile($"./starsky/clientapp/coverage/cobertura-coverage.xml", $"./starskytest/jest-coverage.cobertura.xml");
    }

      IEnumerable<string> redirectedStandardOutput;
      IEnumerable<string> redirectedErrorOutput;
      var exitCodeWithArgument =
          StartProcess(
              "dotnet",
              new ProcessSettings {
                Arguments = new ProcessArgumentBuilder()
                    .Append($"reportgenerator")
                    .Append($"-reports:./starskytest/*coverage.*.xml")
                    .Append($"-targetdir:./starskytest/")
                    .Append($"-reporttypes:Cobertura"),
                  RedirectStandardOutput = true,
                  RedirectStandardError = true
              },
              out redirectedStandardOutput,
              out redirectedErrorOutput
          );

      // Output process output.
      foreach(var stdOutput in redirectedStandardOutput)
      {
          Information("reportgenerator: {0}", stdOutput);
      }

      // Throw exception if anything was written to the standard error.
      if (redirectedErrorOutput.Any())
      {
          throw new Exception(
              string.Format(
                  "Errors occurred: {0}",
                  string.Join(", ", redirectedErrorOutput)));
      }

      // This should output 0 as valid arguments supplied
      Information("Exit code: {0}", exitCodeWithArgument);

      // And rename it
      MoveFile($"./starskytest/Cobertura.xml", outputCoverageFile);
  });

Task("MergeOnlyNetCoreCoverageFiles")
  .Does(() => {
    var outputCoverageFile = $"./starskytest/coverage-merge-cobertura.xml";

    if (FileExists(outputCoverageFile)) {
      DeleteFile(outputCoverageFile);
    }

    // Client Side coverage does not exist
    if (FileExists($"./starskytest/jest-coverage.cobertura.xml")) {
       DeleteFile($"./starskytest/jest-coverage.cobertura.xml");
    }

    IEnumerable<string> redirectedStandardOutput;
    IEnumerable<string> redirectedErrorOutput;
    var exitCodeWithArgument =
        StartProcess(
            "dotnet",
            new ProcessSettings {
              Arguments = new ProcessArgumentBuilder()
                  .Append($"reportgenerator")
                  .Append($"-reports:./starskytest/*coverage.*.xml")
                  .Append($"-targetdir:./starskytest/")
                  .Append($"-reporttypes:Cobertura"),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            },
            out redirectedStandardOutput,
            out redirectedErrorOutput
        );

    // Output process output.
    foreach(var stdOutput in redirectedStandardOutput)
    {
        Information("reportgenerator: {0}", stdOutput);
    }

    // Throw exception if anything was written to the standard error.
    if (redirectedErrorOutput.Any())
    {
        throw new Exception(
            string.Format(
                "Errors occurred: {0}",
                string.Join(", ", redirectedErrorOutput)));
    }

    // This should output 0 as valid arguments supplied
    Information("Exit code: {0}", exitCodeWithArgument);

    // And rename it
    MoveFile($"./starskytest/Cobertura.xml", outputCoverageFile);
});

// Create a nice report and zip it
// coverage-merge-cobertura.xml
Task("CoverageReport")
    .Does(() =>
    {
        var projects = GetFiles("./*test/coverage-merge-cobertura.xml");
        foreach(var project in projects)
        {
            Information("CoverageReport project " + project);
            // Generate html files for reports
            var reportFolder = project.ToString().Replace("merge-cobertura.xml","report");

            IEnumerable<string> redirectedStandardOutput;
            IEnumerable<string> redirectedErrorOutput;
            var exitCodeWithArgument =
                StartProcess(
                    "dotnet",
                    new ProcessSettings {
                      Arguments = new ProcessArgumentBuilder()
                          .Append($"reportgenerator")
                          .Append($"-reports:{project}")
                          .Append($"-targetdir:{reportFolder}")
                          .Append($"-reporttypes:HtmlInline"),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    out redirectedStandardOutput,
                    out redirectedErrorOutput
                );

            // Output process output.
            foreach(var stdOutput in redirectedStandardOutput)
            {
                Information("reportgenerator: {0}", stdOutput);
            }

            // Throw exception if anything was written to the standard error.
            if (redirectedErrorOutput.Any())
            {
                throw new Exception(
                    string.Format(
                        "Errors occurred: {0}",
                        string.Join(", ", redirectedErrorOutput)));
            }

            // This should output 0 as valid arguments supplied
            Information("Exit code: {0}", exitCodeWithArgument);


            // Zip entire folder
            Zip(reportFolder, $"{reportFolder}.zip");
        }
    });

// Publish the app to the /dist folder
Task("PublishWeb")
    .Does(() =>
    {
        foreach(var projectName in publishProjectNames)
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

        if( noSonar ) {
          Information($">> SonarQube is disable due the --no-sonar flag");
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

        // allow to overwrite the branch name
        if (branchName == "" && gitBranch.FriendlyName != "(no branch)") {
          branchName = gitBranch.FriendlyName; // fallback as (no branch)
        }
        // replace default value to master
        if (branchName == "(no branch)" || branchName == "") {
          branchName = "master";
        }
        Information($">> Selecting Branch: {branchName}");

        IEnumerable<string> redirectedStandardOutput;
        IEnumerable<string> redirectedErrorOutput;
        var exitCodeWithArgument =
            StartProcess(
                "dotnet",
                new ProcessSettings {
                  Arguments = new ProcessArgumentBuilder()
                      .Append($"sonarscanner")
                      .Append($"begin")
                      .Append($"/d:sonar.host.url=\"{url}\"")
                      .Append($"/k:\"{key}\"")
                      .Append($"/n:\"Starsky\"")
                      .Append($"/d:sonar.login=\"{login}\"")
                      .Append($"/d:sonar.branch.name=\"{branchName}\"")
                      .Append($"/o:" + organisation)
                      .Append($"/d:sonar.cs.opencover.reportsPaths=\"{netCoreCoverageFile}\"")
                      .Append($"/d:sonar.typescript.lcov.reportPaths=\"{jestCoverageFile}\"")
                      .Append($"/d:sonar.exclusions=\"**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts,*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/*/Migrations/*,**/*spec.tsx,,**/*stories.tsx,**/*spec.ts,**/src/index.tsx,**/src/style/css/vendor/*\"")
                      .Append($"/d:sonar.coverage.exclusions=\"**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts,*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/*/Migrations/*,**/*spec.ts,**/*stories.tsx,**/*spec.tsx,**/src/index.tsx\""),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                out redirectedStandardOutput,
                out redirectedErrorOutput
            );

        // Output process output.
        foreach(var stdOutput in redirectedStandardOutput)
        {
            Information("sonarscanner: {0}", stdOutput);
        }

        // Throw exception if anything was written to the standard error.
        if (redirectedErrorOutput.Any())
        {
            throw new Exception(
                string.Format(
                    "Errors occurred: {0}",
                    string.Join(", ", redirectedErrorOutput)));
        }

        // This should output 0 as valid arguments supplied
        Information("Exit code: {0}", exitCodeWithArgument);
  });

// End the task and send it SonarCloud
Task("SonarEnd")
  .Does(() => {
    var login = EnvironmentVariable("STARSKY_SONAR_LOGIN");
    if( string.IsNullOrEmpty(login) ) {
        Information($">> SonarQube is disabled $ login={login}");
        return;
    }

    if( noSonar ) {
      Information($">> SonarQube is disable due the --no-sonar flag");
      return;
    }

    IEnumerable<string> redirectedStandardOutput;
    IEnumerable<string> redirectedErrorOutput;
    var exitCodeWithArgument =
        StartProcess(
            "dotnet",
            new ProcessSettings {
              Arguments = new ProcessArgumentBuilder()
                  .Append($"sonarscanner")
                  .Append($"end")
                  .Append($"/d:sonar.login=\"{login}\""),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            },
            out redirectedStandardOutput,
            out redirectedErrorOutput
        );

    // Output process output.
    foreach(var stdOutput in redirectedStandardOutput)
    {
        Information("sonarscanner: {0}", stdOutput);
    }

    // Throw exception if anything was written to the standard error.
    if (redirectedErrorOutput.Any())
    {
        throw new Exception(
            string.Format(
                "Errors occurred: {0}",
                string.Join(", ", redirectedErrorOutput)));
    }

    // This should output 0 as valid arguments supplied
    Information("Exit code: {0}", exitCodeWithArgument);
  });

Task("DocsGenerate")
  .Does(() => {
      if (!DirectoryExists($"../starsky-tools/docs"))
      {
        Information($"Docs generation disabled (folder does not exist)");
        return;
      }

      // Running `npm ci` instead of `npm install`
      Information("npm ci restore and build for ../starsky-tools/docs");

      // and build folder
      NpmRunScript("build", s => s.FromPath("../starsky-tools/docs"));

      Information("remove node node_modules for ../starsky-tools/docs/node_modules");

      DeleteDirectory("../starsky-tools/docs/node_modules", new DeleteDirectorySettings {
          Recursive = true
      });

      // copy to build directory
      foreach(var runtime in runtimes)
      {
          if(!DirectoryExists($"./{runtime}")) {
            continue;
          }

          Information("copy to: " + $"./{runtime}/docs");

          CopyDirectory("../starsky-tools/docs/", $"./{runtime}/docs");

          var toDeleteFiles = new string[]{
            $"./{runtime}/docs/docs.js",
            $"./{runtime}/docs/readme.md",
            $"./{runtime}/docs/package.json",
            $"./{runtime}/docs/package-lock.json",
          };

          foreach(var toDeleteFile in toDeleteFiles)
          {
              if (FileExists(toDeleteFile))
              {
                  DeleteFile(toDeleteFile);
              }
          }
      }

  });


Task("ProjectCheckNetCore")
    .Does(() =>
    {
        /* Checks for valid Project GUIDs in csproj files */
        NpmRunScript("project-guid", s => s.FromPath("../starsky-tools/build-tools/"));
  });

// React app build steps
Task("Client")
  .IsDependentOn("TestEnv")
  .IsDependentOn("ClientRestore")
  .IsDependentOn("ClientBuild")
  .IsDependentOn("ClientTest");

// A meta-task that runs all the steps to Build and Test the app
Task("BuildNetCore")
    .IsDependentOn("ProjectCheckNetCore")
    .IsDependentOn("CleanNetCore")
    .IsDependentOn("RestoreNetCore")
    .IsDependentOn("BuildNetCoreGeneric");

Task("SonarBuildTest")
    .IsDependentOn("SonarBegin")
    .IsDependentOn("BuildNetCore")
    .IsDependentOn("TestNetCore")
    .IsDependentOn("SonarEnd");

Task("CoverageDocs")
    .IsDependentOn("MergeCoverageFiles")
    .IsDependentOn("CoverageReport")
    .IsDependentOn("DocsGenerate");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Default")
    .IsDependentOn("Client")
    .IsDependentOn("SonarBuildTest")
    .IsDependentOn("BuildNetCoreRuntimeSpecific")
    .IsDependentOn("PublishWeb")
    .IsDependentOn("CoverageDocs")
    .IsDependentOn("Zip");

// ./build.sh --Target=BuildTestOnlyNetCore
Task("BuildTestOnlyNetCore")
    .IsDependentOn("TestEnv")
    .IsDependentOn("BuildNetCore")
    .IsDependentOn("TestNetCore")
    .IsDependentOn("BuildNetCoreGeneric")
    .IsDependentOn("MergeOnlyNetCoreCoverageFiles")
    .IsDependentOn("CoverageReport");



// To get fast all (net core) assemblies
// ./build.sh --Runtime=osx.10.12-x64 --Target=BuildPublishWithoutTest
Task("BuildPublishWithoutTest")
    .IsDependentOn("BuildNetCore")
    .IsDependentOn("PublishWeb");


// Executes the task specified in the target argument.
RunTarget(target);
