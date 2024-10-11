using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using build;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static SimpleExec.Command;

namespace helpers;

[SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
[SuppressMessage("Sonar",
	"S2629: Don't use string interpolation in logging message templates",
	Justification = "Not production code.")]
[SuppressMessage("Sonar",
	"S6664: Reduce the number of Information logging calls within this code block",
	Justification = "Not production code.")]
public static class SonarQube
{
	public const string SonarQubePackageName = "dotnet-sonarscanner";

	/// <summary>
	///     @see: https://www.nuget.org/packages/dotnet-sonarscanner
	/// </summary>
	private const string SonarQubePackageVersion = "9.0.0";

	private const string SonarQubeDotnetSonarScannerApi =
		"https://api.nuget.org/v3-flatcontainer/dotnet-sonarscanner/index.json";

	private const string GitCommand = "git";
	private const string DefaultBranchName = "master";

	public static void InstallSonarTool(bool noUnitTest, bool noSonar)
	{
		if ( noUnitTest )
		{
			Information(">> SonarBegin is disable due the --no-unit-test flag");
			return;
		}

		if ( noSonar )
		{
			Information(">> SonarBegin is disable due the --no-sonar flag");
			return;
		}

		CheckLatestVersionDotNetSonarScanner().Wait();

		var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain
			.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

		Log.Information(rootDirectory);

		var envs =
			Environment.GetEnvironmentVariables() as
				IReadOnlyDictionary<string, string>;

		var toolList = DotNet($"tool list", rootDirectory, envs, null, true);

		if ( toolList.Any(p => p.Text.Contains(SonarQubePackageName)
		                       && toolList.Any(p => p.Text.Contains(SonarQubePackageVersion))) )
		{
			Log.Information("Next: tool restore");
			DotNet($"tool restore", rootDirectory, envs, null, true);

			Log.Information("Skip creation of manifest files and install");
			return;
		}

		Log.Information("Next: Create new manifest file");

		DotNet($"new tool-manifest --force", rootDirectory, envs, null, true);

		Log.Information("Next: Install Sonar tool");
		DotNetToolInstall(p => p
			.SetPackageName(SonarQubePackageName)
			.SetProcessWorkingDirectory(rootDirectory)
			.SetVersion(SonarQubePackageVersion));
	}

	private static string? EnvironmentVariable(string input) =>
		Environment.GetEnvironmentVariable(input);

	private static void Information(string input) => Log.Information(input);

	static async Task CheckLatestVersionDotNetSonarScanner()
	{
		var result = await HttpQuery.GetJsonFromApi(SonarQubeDotnetSonarScannerApi);
		if ( result == null )
		{
			Log.Information("Nuget API is not available, " +
			                "so skip checking the latest version of {SonarQubePackageName}",
				SonarQubePackageName);
			return;
		}

		var latestVersionByApi = HttpQuery.ParseJsonVersionNumbers(result);
		if ( latestVersionByApi > new Version(SonarQubePackageVersion) )
		{
			Log.Warning("Please upgrade to the latest version " +
			            "of dotnet-sonarscanner {LatestVersionByApi} \n\n", latestVersionByApi);
			Log.Warning("Update the following values: \n" +
			            "- build/helpers/SonarQube.cs -> SonarQubePackageVersion to {LatestVersionByApi} \n" +
			            "The _build project will auto update: \n" +
			            "-  .config/dotnet-tools.json", latestVersionByApi);
		}
	}


	private static void IsJavaInstalled()
	{
		Log.Information("Checking if Java is installed, will fail if not on this step");
		Run(Build.JavaBaseCommand, "-version");
	}

	public static string? GetSonarToken()
	{
		var sonarToken = EnvironmentVariable("STARSKY_SONAR_TOKEN");
		if ( string.IsNullOrEmpty(sonarToken) )
		{
			sonarToken = EnvironmentVariable("SONAR_TOKEN");
		}

		return sonarToken;
	}

	public static string? GetSonarKey() => EnvironmentVariable("STARSKY_SONAR_KEY");

	public static bool SonarBegin(bool noUnitTest, bool noSonar, string branchName,
		string clientAppProject, string coverageFile)
	{
		Information($">> SonarQube key={GetSonarKey()}");

		var sonarToken = GetSonarToken();

		var organisation = EnvironmentVariable("STARSKY_SONAR_ORGANISATION");

		var url = EnvironmentVariable("STARSKY_SONAR_URL");
		if ( string.IsNullOrEmpty(url) )
		{
			url = "https://sonarcloud.io";
		}

		if ( string.IsNullOrEmpty(GetSonarKey()) || string.IsNullOrEmpty(sonarToken) ||
		     string.IsNullOrEmpty(organisation) )
		{
			Information(
				$">> SonarQube is disabled $ key={GetSonarKey()}|token={sonarToken}|organisation={organisation}");
			return false;
		}

		if ( noUnitTest )
		{
			Information(">> SonarBegin is disable due the --no-unit-test flag");
			return false;
		}

		if ( noSonar )
		{
			Information(">> SonarBegin is disable due the --no-sonar flag");
			return false;
		}

		IsJavaInstalled();

		// Current branch name
		var mainRepoPath = Directory.GetParent(".")?.FullName;

		var (gitBranchName, _) =
			ReadAsync(GitCommand, " branch --show-current", mainRepoPath!).Result;

		// allow to overwrite the branch name
		if ( string.IsNullOrEmpty(branchName) && !string.IsNullOrEmpty(gitBranchName) )
		{
			branchName = gitBranchName.Trim(); // fallback as (no branch)
		}

		// replace default value to master
		if ( branchName == "(no branch)" || string.IsNullOrEmpty(branchName) )
		{
			branchName = DefaultBranchName;
		}

		/* this should fix No inputs were found in config file 'tsconfig.json'.  */
		var tsconfig = Path.Combine(clientAppProject, "tsconfig.json");
		Information(">> tsconfig: " + tsconfig);

		// For Pull Requests  
		var isPrBuild = EnvironmentVariable("GITHUB_ACTIONS") != null &&
		                EnvironmentVariable("GITHUB_JOB") != null &&
		                EnvironmentVariable("GITHUB_BASE_REF") != null &&
		                !string.IsNullOrEmpty(EnvironmentVariable("PR_NUMBER_GITHUB"));

		var githubPrNumber = EnvironmentVariable("PR_NUMBER_GITHUB");
		var githubBaseBranch = EnvironmentVariable("GITHUB_BASE_REF");
		var githubRepoSlug = EnvironmentVariable("GITHUB_REPOSITORY");

		Information($">> Selecting Branch: {branchName}");

		var sonarQubeCoverageFile =
			Path.Combine(WorkingDirectory.GetSolutionParentFolder(), coverageFile);
		Information(">> SonarQubeCoverageFile: " + sonarQubeCoverageFile);
		Information(">> GetSolutionParentFolder: " + WorkingDirectory.GetSolutionParentFolder());

		var sonarArguments = new StringBuilder()
			.Append("sonarscanner ")
			.Append("begin ")
			// .Append($"/d:sonar.verbose=true ") 
			.Append($"/d:sonar.host.url={url} ")
			.Append($"/k:{GetSonarKey()} ")
			.Append("/n:Starsky ")
			.Append($"/d:sonar.projectBaseDir={WorkingDirectory.GetSolutionParentFolder()} ")
			.Append($"/d:sonar.token={sonarToken} ")
			.Append("/o:" + organisation + " ")
			.Append($"/d:sonar.typescript.tsconfigPath={tsconfig} ")
			.Append($"/d:sonar.coverageReportPaths={sonarQubeCoverageFile} ")
			.Append("/d:sonar.exclusions=**/build/*,**/build/helpers/*," +
			        "**/documentation/*," +
			        "**/Interfaces/IQuery.cs," +
			        "**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts," +
			        "*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/*/Migrations/*,**/*spec.tsx," +
			        "**/*stories.tsx,**/*spec.ts,**/src/main.tsx,**/src/index.tsx,**/src/style/css/vendor/*,**/node_modules/*," +
			        "**/prestorybook.js,**/vite.config.ts,**/.storybook/**,**/jest.setup.ts," +
			        "**/_bigimages-helper.js ")
			.Append("/d:sonar.coverage.exclusions=**/build/*,**/build/helpers/*," +
			        "**/build/Constants/*," +
			        "**/documentation/*," +
			        "**/Interfaces/IQuery.cs," +
			        "**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts," +
			        "*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/*/Migrations/*," +
			        "**/*spec.ts,**/*stories.tsx,**/*spec.tsx,**/src/main.tsx,**/src/index.tsx,**/node_modules/*," +
			        "**/prestorybook.js,**/vite.config.ts,**/.storybook/**,**/jest.setup.ts," +
			        "**/_bigimages-helper.js ");

		// Normal build
		if ( !isPrBuild )
		{
			Information(">> Normal Build (non-pr)");
			sonarArguments
				.Append($"/d:sonar.branch.name={branchName} ");
		}

		// Pull Request Build
		if ( isPrBuild )
		{
			Information($">> PR Build isPRBuild={true}  githubPrNumber " +
			            $"{githubPrNumber} githubBaseBranch {githubBaseBranch} githubRepoSlug {githubRepoSlug}");

			sonarArguments
				.Append($"/d:sonar.pullrequest.key={githubPrNumber} ")
				.Append($"/d:sonar.pullrequest.branch={gitBranchName} ")
				.Append($"/d:sonar.pullrequest.base={githubBaseBranch} ")
				.Append("/d:sonar.pullrequest.provider=github ")
				.Append("/d:sonar.pullrequest.github.endpoint=https://api.github.com/ ")
				.Append($"/d:sonar.pullrequest.github.repository={githubRepoSlug} ");
		}

		DotNet(sonarArguments.ToString());
		return true;
	}

	public static void SonarEnd(bool noUnitTest, bool noSonar)
	{
		var sonarToken = GetSonarToken();
		if ( string.IsNullOrEmpty(sonarToken) )
		{
			Information($">> SonarQube is disabled $ login={sonarToken}");
			return;
		}

		if ( noUnitTest )
		{
			Information(">> SonarEnd is disable due the --no-unit-test flag");
			return;
		}

		if ( noSonar )
		{
			Information(">> SonarEnd is disable due the --no-sonar flag");
			return;
		}

		var sonarArguments = new StringBuilder()
			.Append("sonarscanner ")
			.Append("end ")
			.Append($"/d:sonar.token={sonarToken} ");

		DotNet(sonarArguments.ToString());

		Log.Information("- - - - - - - - - -  Sonar done - - - - - - - - - - \n");
	}
}
