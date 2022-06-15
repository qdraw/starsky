using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using helpers;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static SimpleExec.Command;
using static helpers.GetSolutionAllProjects;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace helpers;

public static class DotnetTestHelper
{
	static void Information(string input)
	{
		Console.WriteLine(input);
	}

	static List<string> GetFiles(string globSearch)
	{
		Matcher matcher = new();
		matcher.AddIncludePatterns(new[] { globSearch });
		PatternMatchingResult result = matcher.Execute(
			new DirectoryInfoWrapper(
				new DirectoryInfo(".")));
		return result.Files.Select(p => p.Path).ToList();
	}

	static bool DirectoryExists(string path)
	{
		return Directory.Exists(path);
	}

	public static void TestNetCoreGenericCommand(Configuration configuration, bool noUnitTest)
    {
	    Information(">>> next: test");
	    
	    if(noUnitTest)
        {
          Information($">> TestNetCore is disable due the --no-unit-test flag");
          return;
        }

        var projects = GetFiles("./*test/*.csproj");
        foreach(var project in projects)
        {
            Information("Testing project " + project);

            var testParentPath = System.IO.Directory.GetParent(project.ToString()).FullName;

            /* clean test results */
            var testResultsFolder = System.IO.Path.Combine(testParentPath, "TestResults");
            if (DirectoryExists(testResultsFolder))
            {
                Information(">> Removing folder => " + testResultsFolder);
                Directory.Delete(testResultsFolder,true);
            }

            var testArgs = new StringBuilder()
	            .Append("--no-restore ")
	            .Append("--no-build ")
	            .Append("--nologo ")
	            .Append("--blame ") // for debug
	            .Append("-v=normal ") // v=normal is to show test names
	            .Append("--logger \"trx;LogFileName=test_results.trx\" ")
	            .Append("--collect:\"XPlat Code Coverage\" ")
	            .Append("--settings build.vstest.runsettings ");

            DotNetTest(_ => _
	            .SetConfiguration(configuration)
	            // .SetProcessArgumentConfigurator()
	            .EnableNoRestore()
	            .EnableNoBuild()
	            .SetVerbosity(DotNetVerbosity.Normal)
	            .SetLoggers("trx;LogFileName=test_results.trx")
	            .SetDataCollector("XPlat Code Coverage")
	            .SetSettingsFile("build.vstest.runsettings")
	            .SetProjectFile(project));

            // DotNetCoreTest(
            //     project.ToString(),
            //     new DotNetCoreTestSettings()
            //     {
            //         Configuration = configuration,
            //         NoBuild = true,
            //         ArgumentCustomization = args => args
            //               .Append("--no-restore")
            //               .Append("--no-build")
            //               .Append("--nologo")
            //               .Append("--blame") // for debug
            //               .Append("-v=normal") // v=normal is to show test names
            //               .Append("--logger \"trx;LogFileName=test_results.trx\"")
            //               .Append("--collect:\"XPlat Code Coverage\"")
            //               .Append("--settings build.vstest.runsettings")
            //     });

            Information("on Error: search for: Error Message");
            var coverageEnum = GetFiles("./**/coverage.opencover.xml");

            // Get the FirstOrDefault() but there is no LINQ here
            var coverageFilePath =  System.IO.Path.Combine(testParentPath, "netcore-coverage.opencover.xml");
            foreach(var item in coverageEnum)
            {
              CopyFile(item, coverageFilePath, FileExistsPolicy.Overwrite);
            }
            Information("CoverageFile " + coverageFilePath);

            if (!FileExists(coverageFilePath)) {
                throw new Exception("CoverageFile missing " + coverageFilePath);
            }
        }
    }

	static bool FileExists(string path)
	{
		return File.Exists(path);
	}

}
