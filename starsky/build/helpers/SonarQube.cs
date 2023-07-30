using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using build;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static SimpleExec.Command;

namespace helpers
{
	[SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
	public static class SonarQube
	{
		public const string SonarQubePackageName = "dotnet-sonarscanner";
		/// <summary>
		/// @see: https://www.nuget.org/packages/dotnet-sonarscanner
		/// </summary>
		public const string SonarQubePackageVersion = "5.13.0";
		public const string GitCommand = "git";
		public const string DefaultBranchName = "master";

		public static void InstallSonarTool(bool noUnitTest, bool noSonar)
		{
			if(noUnitTest)
			{
				Information($">> SonarBegin is disable due the --no-unit-test flag");
				return;
			}
			if( noSonar ) {
				Information($">> SonarBegin is disable due the --no-sonar flag");
				return;
			}
		
			var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain
				.BaseDirectory!)!.Parent!.Parent!.Parent!.FullName;
			
			Console.WriteLine(rootDirectory);

			var envs =
				Environment.GetEnvironmentVariables() as
					IReadOnlyDictionary<string, string>;

			var toolList = DotNet($"tool list", rootDirectory, envs, null, true);
			if ( toolList.Any(p => p.Text.Contains(SonarQubePackageName) 
			                       && toolList.Any(p => p.Text.Contains(SonarQubePackageVersion)) ))
			{
				Console.WriteLine("Next: tool restore");
				DotNet($"tool restore", rootDirectory, envs, null, true);

				Console.WriteLine("Skip creation of manifest files and install");
				return;
			}

			Console.WriteLine("Next: Create new manifest file");
			DotNet($"new tool-manifest --force", rootDirectory, envs, null, true);

			Console.WriteLine("Next: Install Sonar tool");
			DotNetToolInstall(_ => _
				.SetPackageName(SonarQubePackageName)
				//.SetProcessArgumentConfigurator(_ => _.Add("-d"))
				.SetProcessWorkingDirectory(rootDirectory)
				.SetVersion(SonarQubePackageVersion));
		
		}

		private static string EnvironmentVariable(string input)
		{
			return Environment.GetEnvironmentVariable(input);
		}

		private static void Information(string input)
		{
			Console.WriteLine(input);
		}

		private static void IsJavaInstalled()
		{
			Console.WriteLine("Checking if Java is installed, will fail if not on this step");
			Run(Build.JavaBaseCommand, "-version");
		}

		private static string GetSonarToken()
		{
			var sonarToken = EnvironmentVariable("STARSKY_SONAR_TOKEN");
			if( string.IsNullOrEmpty(sonarToken) ) {
				sonarToken = EnvironmentVariable("SONAR_TOKEN");
			}
			return sonarToken;
		}
	
		public static bool SonarBegin(bool noUnitTest, bool noSonar, string branchName, string clientAppProject, string coverageFile)
		{
			var key = EnvironmentVariable("STARSKY_SONAR_KEY");
			
			Information($">> SonarQube key={key}");
			
			var sonarToken = GetSonarToken();

			var organisation = EnvironmentVariable("STARSKY_SONAR_ORGANISATION");

			var url = EnvironmentVariable("STARSKY_SONAR_URL");
			if(string.IsNullOrEmpty(url)) {
				url = "https://sonarcloud.io";
			}

			if( string.IsNullOrEmpty(key) || string.IsNullOrEmpty(sonarToken) || string.IsNullOrEmpty(organisation) ) {
				Information($">> SonarQube is disabled $ key={key}|token={sonarToken}|organisation={organisation}");
				return false;
			}

			if(noUnitTest)
			{
				Information($">> SonarBegin is disable due the --no-unit-test flag");
				return false;
			}

			if( noSonar ) {
				Information($">> SonarBegin is disable due the --no-sonar flag");
				return false;
			}

			IsJavaInstalled();

			// Current branch name
			var mainRepoPath = Directory.GetParent(".")?.FullName;
        
			var (gitBranchName,_) = ReadAsync(GitCommand, " branch --show-current", mainRepoPath!).Result;

			// allow to overwrite the branch name
			if (string.IsNullOrEmpty(branchName) && !string.IsNullOrEmpty(gitBranchName)) {
				branchName = gitBranchName.Trim(); // fallback as (no branch)
			}
			
			// replace default value to master
			if (branchName == "(no branch)" || string.IsNullOrEmpty(branchName)) {
				branchName = DefaultBranchName;
			}
        
			/* this should fix No inputs were found in config file 'tsconfig.json'.  */
			var tsconfig = Path.Combine(clientAppProject,"tsconfig.json");
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
			
			var sonarQubeCoverageFile = Path.Combine(WorkingDirectory.GetSolutionParentFolder(), coverageFile);
			Information(">> SonarQubeCoverageFile: " + sonarQubeCoverageFile);

			var sonarArguments = new StringBuilder()
				.Append($"sonarscanner ")
				.Append($"begin ")
				/* .Append($"/d:sonar.verbose=true ") */
				.Append($"/d:sonar.host.url={url} ")
				.Append($"/k:{key} ")
				.Append($"/n:\"Starsky\" ")
				.Append($"/d:sonar.token={sonarToken} ")
				.Append($"/o:" + organisation +" ")
				.Append($"/d:sonar.typescript.tsconfigPath={tsconfig} ")
				.Append($"/d:sonar.coverageReportPaths={sonarQubeCoverageFile} ")
				.Append($"/d:sonar.exclusions=\"**/build/*,**/build/helpers/*," +
				        "**/documentation/*,"+
				        "**/Interfaces/IQuery.cs," +
				        $"**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts," +
				        $"*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/*/Migrations/*,**/*spec.tsx," +
				        $"**/*stories.tsx,**/*spec.ts,**/src/index.tsx,**/src/style/css/vendor/*,**/node_modules/*," +
				        $"**/prestorybook.js," +
				        $"**/_bigimages-helper.js\" ")
				.Append($"/d:sonar.coverage.exclusions=\"**/build/*,**/build/helpers/*," +
				        "**/documentation/*,"+
				        "**/Interfaces/IQuery.cs," +
				        $"**/setupTests.js,**/react-app-env.d.ts,**/service-worker.ts," +
				        $"*webhtmlcli/**/*.js,**/wwwroot/js/**/*,**/*/Migrations/*," +
				        $"**/*spec.ts,**/*stories.tsx,**/*spec.tsx,**/src/index.tsx,**/node_modules/*," +
				        $"**/prestorybook.js," +
				        $"**/_bigimages-helper.js\" ");
        
			// Normal build
			if (!isPrBuild) {
				Information($">> Normal Build (non-pr)");
				sonarArguments
					.Append($"/d:sonar.branch.name=\"{branchName}\" ");
			}
        
			// Pull Request Build
			if (isPrBuild) {
				Information($">> PR Build isPRBuild={true}  githubPrNumber " +
				            $"{githubPrNumber} githubBaseBranch {githubBaseBranch} githubRepoSlug {githubRepoSlug}");

				sonarArguments
					.Append($"/d:sonar.pullrequest.key=\"{githubPrNumber}\" ")
					.Append($"/d:sonar.pullrequest.branch=\"{gitBranchName}\" ")
					.Append($"/d:sonar.pullrequest.base=\"{githubBaseBranch}\" ")
					.Append($"/d:sonar.pullrequest.provider=\"github\" ")
					.Append($"/d:sonar.pullrequest.github.endpoint=\"https://api.github.com/\" ")
					.Append($"/d:sonar.pullrequest.github.repository=\"{githubRepoSlug}\" ");
			}

			DotNet(sonarArguments.ToString());
			return true;
		}

		public static void SonarEnd(bool noUnitTest, bool noSonar)
		{
			var sonarToken = GetSonarToken();
			if( string.IsNullOrEmpty(sonarToken) ) {
				Information($">> SonarQube is disabled $ login={sonarToken}");
				return;
			}

			if(noUnitTest)
			{
				Information($">> SonarEnd is disable due the --no-unit-test flag");
				return;
			}
			if( noSonar ) {
				Information($">> SonarEnd is disable due the --no-sonar flag");
				return;
			}

			var sonarArguments = new StringBuilder()
				.Append($"sonarscanner ")
				.Append($"end ")
				.Append($"/d:sonar.token={sonarToken} ");

			DotNet(sonarArguments.ToString());
			
			Console.Write("- - - - - - - - - -  Sonar done - - - - - - - - - - \n");
		}
	}	
}
