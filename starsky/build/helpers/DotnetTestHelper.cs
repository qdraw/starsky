using System.IO;
using build;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace helpers
{
	public static class DotnetTestHelper
	{
		static bool DirectoryExists(string path)
		{
			return Directory.Exists(path);
		}

		public static void TestNetCoreGenericCommand(Configuration configuration, bool noUnitTest)
		{
			Log.Information(">> next: TestNetCoreGenericCommand");

			if ( noUnitTest )
			{
				Log.Information($">> TestNetCore is disable due the --no-unit-test flag");
				return;
			}

			var projects = GetFilesHelper.GetFiles("*test/*.csproj");
			if ( projects.Count == 0 )
			{
				throw new FileNotFoundException("missing tests in *test/*.csproj");
			}

			foreach ( var project in projects )
			{
				var projectFullPath = Path.Combine(WorkingDirectory.GetSolutionParentFolder(),
					project);
				Log.Information("Testing project {Project}", project);

				var testParentPath = Directory.GetParent(projectFullPath)?.FullName!;
				Log.Information("testParentPath {TestParentPath} ", testParentPath);

				/* clean test results */
				var testResultsFolder = Path.Combine(testParentPath, "TestResults");
				if ( DirectoryExists(testResultsFolder) )
				{
					Log.Information(">> Removing folder => {testResultsFolder}", testResultsFolder);
					Directory.Delete(testResultsFolder, true);
				}

				var runSettingsFile = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(), "build.vstest.runsettings");

				Log.Information("runSettingsFile {RunSettingsFile}", runSettingsFile);

				try
				{
					// search for: dotnet test
					DotNetTest(p => p
						.SetConfiguration(configuration)
						.EnableNoRestore()
						.EnableNoBuild()
						.SetVerbosity(DotNetVerbosity.normal)
						.SetLoggers("trx;LogFileName=test_results.trx")
						.SetDataCollector("XPlat Code Coverage")
						.SetSettingsFile(runSettingsFile)
						.SetProjectFile(projectFullPath));
				}
				catch ( ProcessException )
				{
					var trxFullFilePath = Path.Combine(
						testParentPath,
						"TestResults",
						"test_results.trx");

					TrxParserHelper.DisplayFileTests(trxFullFilePath);
					throw;
				}

				var coverageEnum = GetFilesHelper.GetFiles("**/coverage.opencover.xml");

				foreach ( var coverageItem in coverageEnum )
				{
					Log.Information("coverageItem: {CoverageItem}", coverageItem);
				}

				// Get the FirstOrDefault() but there is no LINQ here
				var coverageFilePath =
					Path.Combine(testParentPath, "netcore-coverage.opencover.xml");
				Log.Information("next copy: coverageFilePath {CoverageFilePath}", coverageFilePath);

				foreach ( var item in coverageEnum )
				{
					CopyFile(Path.Combine(WorkingDirectory.GetSolutionParentFolder(), item),
						coverageFilePath, FileExistsPolicy.Overwrite);
				}

				if ( !FileExists(coverageFilePath) )
				{
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
