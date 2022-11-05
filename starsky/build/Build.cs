using System;
using System.Collections.Generic;
using System.Linq;
using helpers;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.ProjectModel;

// ReSharper disable once CheckNamespace
namespace build
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S3887:Use an immutable collection or reduce the " +
		"accessibility of the non-private readonly field", Justification = "Not production code.")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S2386:Use an immutable collection or reduce " +
		"the accessibility of the non-private readonly field", Justification = "Not production code.")]
	[ShutdownDotNetAfterServerBuild]
	public sealed class Build : NukeBuild
	{
		/// Support plugins are available for:
		///   - JetBrains ReSharper        https://nuke.build/resharper
		///   - JetBrains Rider            https://nuke.build/rider
		///   - Microsoft VisualStudio     https://nuke.build/visualstudio
		///   - Microsoft VSCode           https://nuke.build/vscode

		public static int Main () => Execute<Build>(x => x.Compile);

		// Use `--target BuildNetCoreRuntimeSpecific --skip` parameter to run only this task 
		
		[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
		readonly Configuration Configuration = Configuration.Release;

		public const string GenericRuntimeName = "generic-netcore";

		[Parameter("Runtime arg")]
		readonly string Runtime = GenericRuntimeName;
		
		[Parameter("Is SonarQube Disabled")]
		readonly bool NoSonar;

		[Parameter("Is Unit Test Disabled (same as NoUnitTest, NoUnitTests, NoTest and NoTests)")] 
		readonly bool NoUnitTest;
		
		[Parameter("Is Unit Test Disabled (same as NoUnitTest, NoUnitTests, NoTest and NoTests)")] 
		readonly bool NoUnitTests;
		
		[Parameter("Is Unit Test Disabled (same as NoUnitTest, NoUnitTests, NoTest and NoTests)")] 
		readonly bool NoTest;
		
		[Parameter("Is Unit Test Disabled (same as NoUnitTest, NoUnitTests, NoTest and NoTests)")] 
		readonly bool NoTests;
	
		[Parameter("Skip clientside code")] 
		readonly bool NoClient;
		
		[Parameter("Skip Dependencies download e.g. exiftool / geo data, nuget/npm deps are always installed")] 
		readonly bool NoDependencies;
		
		bool IsUnitTestDisabled()
		{
			// --no-unit-test, --no-unit-tests, --no-test, --no-tests
			return NoUnitTest || NoUnitTests || NoTest || NoTests;
		}
		
		[Parameter("Overwrite branch name")] 
		readonly string Branch;
		string GetBranchName()
		{
			var branchName = Branch;
			if( !string.IsNullOrEmpty(branchName) && branchName.StartsWith("refs/heads/")) {
				branchName  = branchName.Replace("refs/heads/","");
			}
			return branchName;
		}
		
		List<string> GetRuntimesWithoutGeneric()
		{
			return Runtime.Split(",", StringSplitOptions.TrimEntries).Where(p => p != GenericRuntimeName).ToList();
		}

		[Solution(SuppressBuildProjectCheck = true)] 
		readonly Solution Solution;

		public static readonly List<string> PublishProjectsList = new List<string>
		{
			"starskyadmincli",
			"starskygeocli",
			"starskyimportercli",
			"starskysynchronizecli",
			"starskythumbnailcli",
			"starskywebftpcli",
			"starskywebhtmlcli",
			"starskythumbnailmetacli",
			"starsky"
		};
    
		/// <summary>
		/// Npm and node are required for preflight checks and building frontend code
		/// </summary>
		public const string NpmBaseCommand = "npm";
		public const string ClientAppFolder = "starsky/clientapp";
		
		/// <summary>
		/// Java is only needed for SonarQube, skip sonarCube with the --no-sonar flag
		/// </summary>
		public const string JavaBaseCommand = "java";

		Target Client => _ => _
			.Executes(() =>
			{
				if ( NoClient )
				{
					Console.WriteLine("--no-client flag is used");
					return;
				}
				Console.WriteLine("> client");
				ShowSettingsInfo();
				ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
				ClientHelper.NpmPreflight();
				ClientHelper.ClientCiCommand();
				ClientHelper.ClientBuildCommand();
				if ( !IsUnitTestDisabled() )
				{
					ClientHelper.ClientTestCommand();
				}
				else
				{
					Console.WriteLine("Test skipped due --no-unit-tests flag");
				}
			});

		void ShowSettingsInfo()
		{
			Console.WriteLine("---");
			Console.WriteLine(IsUnitTestDisabled()
				? "Unit test disabled"
				: "Unit test enabled");
				
			Console.WriteLine(NoSonar
				? "Sonar disabled"
				: "Sonar enabled");

			Console.WriteLine("Branch:");
			Console.WriteLine(GetBranchName());

			Console.WriteLine("Runtime:");
			foreach ( var runtime in GetRuntimesWithoutGeneric() )
			{
				Console.WriteLine(runtime);
			}

			Console.WriteLine("---");
		}
		
		Target ShowSettingsInformation => _ => _
			.Executes(ShowSettingsInfo);

		/// <summary>
		/// Default Target
		/// </summary>
		Target Compile => _ => _
			.DependsOn(ShowSettingsInformation)
			.DependsOn(Client)
			.DependsOn(SonarBuildTest)
			.DependsOn(BuildNetCoreRuntimeSpecific)
			.DependsOn(DocsGenerate)
			.DependsOn(CoverageReport)
			.DependsOn(Zip);
		
		Target SonarBuildTest => _ => _
			.DependsOn(Client)
			.Executes(() =>
			{
				ShowSettingsInfo();
				ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
				DotnetGenericHelper.RestoreNetCoreCommand(Solution);
				SonarQube.InstallSonarTool(IsUnitTestDisabled(), NoSonar);
				SonarQube.SonarBegin(IsUnitTestDisabled(),NoSonar,GetBranchName(), ClientHelper.GetClientAppFolder(),
					"starskytest/coverage-merge-sonarqube.xml");
				DotnetGenericHelper.BuildNetCoreGenericCommand(Solution,Configuration);
				DotnetTestHelper.TestNetCoreGenericCommand(Configuration,IsUnitTestDisabled());
				DotnetGenericHelper.DownloadDependencies(Solution,Configuration, 
					"starskygeocli/starskygeocli.csproj",NoDependencies, 
					"generic-netcore");
				MergeCoverageFiles.Merge(IsUnitTestDisabled());
				SonarQube.SonarEnd(IsUnitTestDisabled(),NoSonar);
				DotnetGenericHelper.PublishNetCoreGenericCommand(Solution, Configuration);
			});

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S1144:UnusedMember.Local", Justification = "Not production code.")]
		// ReSharper disable once UnusedMember.Local
		Target DownloadDependencies => _ => _
			.Executes(() =>
			{
				DotnetGenericHelper.DownloadDependencies(Solution,Configuration, 
					"starskygeocli/starskygeocli.csproj",NoDependencies, 
					"generic-netcore");
				DotnetRuntimeSpecificHelper.CopyDependenciesFiles(NoDependencies,
					"generic-netcore",GetRuntimesWithoutGeneric());
				
			});

		Target BuildNetCoreRuntimeSpecific => _ => _
			.DependsOn(SonarBuildTest)
			.Executes(() =>
			{
				if ( !GetRuntimesWithoutGeneric().Any() )
				{
					Console.WriteLine("There are no runtime specific items selected");
					return;
				}
				
				ShowSettingsInfo();
				DotnetRuntimeSpecificHelper.Clean(GetRuntimesWithoutGeneric());
				DotnetRuntimeSpecificHelper.RestoreNetCoreCommand(Solution,
					GetRuntimesWithoutGeneric());
				DotnetRuntimeSpecificHelper.BuildNetCoreCommand(Solution,
					GetRuntimesWithoutGeneric(),Configuration);
				DotnetRuntimeSpecificHelper.PublishNetCoreGenericCommand(Solution,
					GetRuntimesWithoutGeneric(),Configuration);
				DotnetRuntimeSpecificHelper.CopyDependenciesFiles(NoDependencies,
					"generic-netcore",GetRuntimesWithoutGeneric());
				
			});
		
		// ReSharper disable once UnusedMember.Local
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S1144:UnusedMember.Local", Justification = "Not production code.")]
		Target BuildNetCore => _ => _
			.Executes(() =>
			{
				ShowSettingsInfo();
				ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
				DotnetGenericHelper.RestoreNetCoreCommand(Solution);
				DotnetGenericHelper.BuildNetCoreGenericCommand(Solution,Configuration);
			});
		
		// ReSharper disable once UnusedMember.Local
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "S1144:UnusedMember.Local", Justification = "Not production code.")]
		Target TestNetCore => _ => _
			.Executes(() =>
			{
				ShowSettingsInfo();
				DotnetTestHelper.TestNetCoreGenericCommand(Configuration,IsUnitTestDisabled());
			});
		
		Target DocsGenerate => _ => _
			.DependsOn(SonarBuildTest)
			.Executes(() =>
			{
				ShowSettingsInfo();
				DocsGenerateHelper.Docs(GetRuntimesWithoutGeneric());
			});
		
		Target Zip => _ => _
			.DependsOn(Client)
			.DependsOn(SonarBuildTest)
			.DependsOn(BuildNetCoreRuntimeSpecific)
			.Executes(() =>
			{
				ShowSettingsInfo();
				ZipperHelper.ZipGeneric();
				ZipperHelper.ZipRuntimes(GetRuntimesWithoutGeneric());
			});
			
		/// <summary>
		/// Generates html coverage report
		/// </summary>
		Target CoverageReport => _ => _
			.DependsOn(Client)
			.DependsOn(SonarBuildTest)
			.Executes(() =>
			{
				ShowSettingsInfo();
				var folder = CoverageReportHelper.GenerateHtml(IsUnitTestDisabled());
				ZipperHelper.ZipHtmlCoverageReport(folder, IsUnitTestDisabled());
			});
	}
}
