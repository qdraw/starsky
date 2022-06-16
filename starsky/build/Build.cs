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
    
		List<string> GetRuntimesWithoutGeneric()
		{
			return Runtime.Split(",", StringSplitOptions.TrimEntries).Where(p => p != GenericRuntimeName).ToList();
		}

		[Solution] readonly Solution Solution;
		[GitRepository] readonly GitRepository GitRepository;

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

		/// <summary>
		/// Default Target
		/// </summary>
		Target Compile => _ => _
			.DependsOn(Client)
			.DependsOn(DotnetGenericBuildAndTest)
			.DependsOn(DotnetRuntimeSpecific);
    
		Target DotnetGenericBuildAndTest => _ => _
			.DependsOn(Client)
			.Executes(() =>
			{
				ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
				DotnetGenericHelper.RestoreNetCoreCommand(Solution);
				InstallSonarTool();
				SonarBegin(false,false,"test", ClientHelper.GetClientAppFolder(),"starskytest/coverage-merge-sonarqube.xml");
				DotnetGenericHelper.BuildNetCoreGenericCommand(Solution,Configuration);
				DotnetTestHelper.TestNetCoreGenericCommand(Configuration,false);
				MergeCoverageFiles.Merge(false);
				SonarEnd(false,false);
				DotnetGenericHelper.PublishNetCoreGenericCommand(Solution, Configuration);
			});
		
		Target DotnetRuntimeSpecific => _ => _
			.DependsOn(Client)
			.DependsOn(DotnetGenericBuildAndTest)
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
	}
}
