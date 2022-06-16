using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using helpers;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using static helpers.SonarQube;

namespace build
{
	[ShutdownDotNetAfterServerBuild]
	public class Build : NukeBuild
	{
		/// Support plugins are available for:
		///   - JetBrains ReSharper        https://nuke.build/resharper
		///   - JetBrains Rider            https://nuke.build/rider
		///   - Microsoft VisualStudio     https://nuke.build/visualstudio
		///   - Microsoft VSCode           https://nuke.build/vscode

		public static int Main () => Execute<Build>(x => x.Compile);

		[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
		readonly Configuration Configuration = Configuration.Release;

		public const string GenericRuntimeName = "generic-netcore";

		[Parameter("Runtime arg")]
		readonly string Runtime = GenericRuntimeName;
		
		[Parameter("Is SonarQube Disabled")]
		readonly bool NoSonar;

		[Parameter("Is Unit Test Disabled (same as NoUnitTest, NoUnitTests and NoTest)")] 
		readonly bool NoUnitTest;
		
		[Parameter("Is Unit Test Disabled (same as NoUnitTest, NoUnitTests and NoTest)")] 
		readonly bool NoUnitTests;
		
		[Parameter("Is Unit Test Disabled (same as NoUnitTest, NoUnitTests and NoTest)")] 
		readonly bool NoTest;
	
		bool IsUnitTestDisabled()
		{
			return NoUnitTest || NoUnitTests || NoTest;
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

		[Solution] 
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
    
		public const string NpmBaseCommand = "npm";
		public const string ClientAppFolder = "starsky/clientapp";

		Target Client => _ => _
			.Executes(() =>
			{
				Console.WriteLine("> client");
				ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
				ClientHelper.ClientCiCommand();
				ClientHelper.ClientBuildCommand();
				ClientHelper.ClientTestCommand();
			});
		
		Target ShowSettingsInformation => _ => _
			.Executes(() =>
			{
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
			});

		/// <summary>
		/// Default Target
		/// </summary>
		Target Compile => _ => _
			.DependsOn(ShowSettingsInformation)
			.DependsOn(Client)
			.DependsOn(SonarBuildTest)
			.DependsOn(BuildNetCoreRuntimeSpecific);
		
		Target SonarBuildTest => _ => _
			.DependsOn(ShowSettingsInformation)
			.Executes(() =>
			{
				ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
				DotnetGenericHelper.RestoreNetCoreCommand(Solution);
				InstallSonarTool();
				SonarBegin(NoUnitTest,NoSonar,GetBranchName(), ClientHelper.GetClientAppFolder(),"starskytest/coverage-merge-sonarqube.xml");
				DotnetGenericHelper.BuildNetCoreGenericCommand(Solution,Configuration);
				DotnetTestHelper.TestNetCoreGenericCommand(Configuration,IsUnitTestDisabled());
				MergeCoverageFiles.Merge(NoUnitTest);
				SonarEnd(NoUnitTest,NoSonar);
				DotnetGenericHelper.PublishNetCoreGenericCommand(Solution, Configuration);
			});
		
		Target BuildNetCoreRuntimeSpecific => _ => _
			.DependsOn(ShowSettingsInformation)
			.Executes(() =>
			{
				if ( !GetRuntimesWithoutGeneric().Any() )
				{
					Console.WriteLine("There are no runtime specific items selected");
					return;
				}
				
				DotnetRuntimeSpecificHelper.RestoreNetCoreCommand(Solution,
					GetRuntimesWithoutGeneric());
				DotnetRuntimeSpecificHelper.BuildNetCoreCommand(Solution,
					GetRuntimesWithoutGeneric(),Configuration);
				DotnetRuntimeSpecificHelper.PublishNetCoreGenericCommand(Solution,
					GetRuntimesWithoutGeneric(),Configuration);
			});
		
		Target BuildNetCore => _ => _
			.Executes(() =>
			{
				ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
				DotnetGenericHelper.RestoreNetCoreCommand(Solution);
				DotnetGenericHelper.BuildNetCoreGenericCommand(Solution,Configuration);
			});
		
		Target TestNetCore => _ => _
			.Executes(() =>
			{
				DotnetTestHelper.TestNetCoreGenericCommand(Configuration,IsUnitTestDisabled());
			});
		
		Target DocsGenerate => _ => _
			.DependsOn(ShowSettingsInformation)
			.Executes(() =>
			{
				// todo!
			});
		
		Target Zip => _ => _
			.DependsOn(ShowSettingsInformation)
			.Executes(() =>
			{
				// todo!
			});
	}
}
