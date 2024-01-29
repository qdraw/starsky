using System;
using System.IO;
using build;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace helpers
{
	public static class DotnetTestHelper
	{
		static void Information(string input)
		{
			Log.Information(input);
		}

		static bool DirectoryExists(string path)
		{
			return Directory.Exists(path);
		}

		public static void TestNetCoreGenericCommand(Configuration configuration, bool noUnitTest)
		{
			Information(">> next: TestNetCoreGenericCommand");
	    
			if(noUnitTest)
			{
				Information($">> TestNetCore is disable due the --no-unit-test flag");
				return;
			}
			
			var projects = GetFilesHelper.GetFiles("*test/*.csproj");
			if ( projects.Count == 0 )
			{
				throw new FileNotFoundException("missing tests in *test/*.csproj" );
			}
			
			foreach(var project in projects)
			{
				var projectFullPath = Path.Combine(WorkingDirectory.GetSolutionParentFolder(),
					project);
				Information("Testing project " + project);

				var testParentPath = Directory.GetParent(projectFullPath)?.FullName;
				Information("testParentPath " + testParentPath);

				/* clean test results */
				var testResultsFolder = Path.Combine(testParentPath!, "TestResults");
				if (DirectoryExists(testResultsFolder))
				{
					Information(">> Removing folder => " + testResultsFolder);
					Directory.Delete(testResultsFolder,true);
				}
				
				var runSettingsFile = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(), "build.vstest.runsettings");
				Console.WriteLine("runSettingsFile " + runSettingsFile);
				
				// search for: dotnet test
				DotNetTest(_ => _
					.SetConfiguration(configuration)
					// .SetProcessArgumentConfigurator()
					.EnableNoRestore()
					.EnableNoBuild()
					.SetVerbosity(DotNetVerbosity.Normal)
					.SetLoggers("trx;LogFileName=test_results.trx")
					.SetDataCollector("XPlat Code Coverage")
					.SetSettingsFile(runSettingsFile)
					.SetProjectFile(projectFullPath));

				Information("on Error: search for: Error Message");
				var coverageEnum = GetFilesHelper.GetFiles("**/coverage.opencover.xml");

				foreach ( var coverageItem in coverageEnum )
				{
					Information("coverageItem: " + coverageItem);
				}

				// Get the FirstOrDefault() but there is no LINQ here
				var coverageFilePath =  Path.Combine(testParentPath, "netcore-coverage.opencover.xml");
				Information("next copy: coverageFilePath " + coverageFilePath);

				foreach(var item in coverageEnum)
				{
					CopyFile(Path.Combine(WorkingDirectory.GetSolutionParentFolder(), item), 
						coverageFilePath, FileExistsPolicy.Overwrite);
				}

				if (!FileExists(coverageFilePath)) {
					throw new FileNotFoundException("CoverageFile missing " + coverageFilePath);
				}
			}
		}

		static bool FileExists(string path)
		{
			return File.Exists(path);
		}
	}	
}
