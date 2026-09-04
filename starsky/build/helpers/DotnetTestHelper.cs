using System.Diagnostics.CodeAnalysis;
using System.IO;
using build;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace helpers;

[SuppressMessage("Sonar",
	"S6664: Reduce the number of Information logging calls within this code block",
	Justification = "Not production code.")]
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
			Log.Information(">> TestNetCore is disable due the --no-unit-test flag");
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
				Log.Information(">> Removing folder => {TestResultsFolder}", testResultsFolder);
				Directory.Delete(testResultsFolder, true);
			}

			var runSettingsFile = Path.Combine(
				WorkingDirectory.GetSolutionParentFolder(), "build.vstest.runsettings");

			Log.Information("runSettingsFile {RunSettingsFile}", runSettingsFile);

			var trxFullFilePath = Path.Combine(
				testParentPath,
				"TestResults",
				"test_results.trx");

			// MTP (Microsoft.Testing.Platform) args — coverage file lands in testResultsFolder
			var mtpArgs = string.Join(" ",
				"--coverage",
				"--coverage-output coverage.cobertura.xml",
				"--coverage-output-format cobertura",
				$"--results-directory \"{testResultsFolder}\"",
				"--report-trx",
				"--report-trx-filename test_results.trx",
				$"--settings \"{runSettingsFile}\"");

			try
			{
				// dotnet msbuild -t:Test routes through Microsoft.Testing.Platform
				// (VSTest target removed on .NET 10 SDK for MTP projects)
				DotNetMSBuild(p => p
					.SetTargets("Test")
					.SetTargetPath(projectFullPath)
					.SetProperty("Configuration", configuration.ToString())
					.SetProperty("TestingPlatformCommandLineArguments", mtpArgs));
			}
			catch ( ProcessException )
			{
				TrxParserHelper.DisplayFailedFileTests(trxFullFilePath);
				throw;
			}
			finally
			{
				TrxParserHelper.DisplaySlowestTests(trxFullFilePath);
			}

			// Coverage file is written directly to testResultsFolder with a fixed name
			var coverageSourcePath = Path.Combine(testResultsFolder, "coverage.cobertura.xml");
			var coverageFilePath = Path.Combine(testParentPath, "netcore-coverage.cobertura.xml");
			Log.Information("next copy: coverageFilePath {CoverageFilePath}", coverageFilePath);

			if ( FileExists(coverageSourcePath) )
			{
				var fromPath = AbsolutePath.Create(coverageSourcePath);
				var toPath = AbsolutePath.Create(coverageFilePath);
				fromPath.Copy(toPath, ExistsPolicy.FileOverwrite);
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
