using System;
using System.Collections.Generic;
using System.Linq;
using helpers;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.ProjectModel;
using Serilog;

// ReSharper disable once CheckNamespace
namespace build;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
	"S3887:Use an immutable collection or reduce the " +
	"accessibility of the non-private readonly field", 
	Justification = "Not production code.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
	"S2386:Use an immutable collection or reduce " +
	"the accessibility of the non-private readonly field", 
	Justification = "Not production code.")]
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
		
	/// <summary>
	/// --no-dependencies
	/// Only applies for External dependencies
	/// Nuget & NPM dependencies are always installed
	/// </summary>
	[Parameter("Skip Dependencies download e.g. exiftool / " +
	           "geo data, nuget/npm deps are always installed")] 
	readonly bool NoDependencies;
		
	/// <summary>
	/// --no-unit-test, --no-unit-tests, --no-test, --no-tests
	/// </summary>
	/// <returns></returns>
	bool IsUnitTestDisabled()
	{
		// --no-unit-test, --no-unit-tests, --no-test, --no-tests
		return NoUnitTest || NoUnitTests || NoTest || NoTests;
	}
		
	/// <summary>
	/// --no-publish
	/// </summary>
	[Parameter("Skip Publish step")] 
	readonly bool NoPublish;
		
	bool IsPublishDisabled()
	{
		// --no-publish
		return NoPublish;
	}
		
	/// <summary>
	/// Overwrite Branch name
	/// </summary>
	/// <returns>only if overwritten</returns>
	[Parameter("Overwrite branch name")] 
	readonly string Branch;
	
	/// <summary>
	/// Overwrite Branch name
	/// </summary>
	/// <returns>only if overwritten</returns>
	string GetBranchName()
	{
		var branchName = Branch;
		if( !string.IsNullOrEmpty(branchName) && branchName.StartsWith("refs/heads/")) {
			branchName  = branchName.Replace("refs/heads/","");
		}
		return branchName;
	}
	
	/// <summary>
	/// Enable for Ready to run build
	/// Only is combination is supported
	/// @see: https://learn.microsoft.com/en-us/dotnet/core/deploying/ready-to-run
	/// for build combinations, if not supported, it will auto skip
	/// </summary>
	/// <returns>true if explicit enabled</returns>
	[Parameter("Enable Ready to run builds")] 
	readonly bool ReadyToRun;
	
	/// <summary>
	/// --ready-to-run
	/// </summary>
	/// <returns></returns>
	public bool IsReadyToRunEnabled()
	{
		return ReadyToRun;
	}
		
	/// <summary>
	/// Only the OS specific runtimes, so skip generic-netcore
	/// </summary>
	/// <returns>Only OS specific runtimes</returns>
	List<string> GetRuntimesWithoutGeneric()
	{
		return Runtime.Split(",", 
				StringSplitOptions.TrimEntries).Where(p => p != GenericRuntimeName)
			.ToList();
	}

	/// <summary>
	/// Solution .sln file
	/// </summary>
	[Solution(SuppressBuildProjectCheck = true)] 
	readonly Solution Solution;

	/// <summary>
	/// List of output projects
	/// </summary>
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
	/// Link to GeoCli.csproj
	/// </summary>
	const string GeoCliCsproj = "starskygeocli/starskygeocli.csproj";
    
	/// <summary>
	/// Npm and node are required for preflight checks and building frontend code
	/// </summary>
	public const string NpmBaseCommand = "npm";
	
	/// <summary>
	/// Node name
	/// </summary>
	public const string NodeBaseCommand = "node";
	
	/// <summary>
	/// Client App folder
	/// </summary>
	public const string ClientAppFolder = "starsky/clientapp";
		
	/// <summary>
	/// Java is only needed for SonarQube, skip sonarCube with the --no-sonar flag
	/// </summary>
	public const string JavaBaseCommand = "java";

	Target Client => p => p
		.Executes(() =>
		{
			if ( NoClient )
			{
				Log.Information("--no-client flag is used\n so skip Client build");
				return;
			}
			Log.Information("> Continue with Client build");
			
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
				Log.Information("Test skipped due --no-unit-tests flag");
			}
		});

	void ShowSettingsInfo()
	{
		Log.Information("---");
		Log.Information("Settings:");
		
		Log.Information($"Current RID: {RuntimeIdentifier.GetCurrentRuntimeIdentifier()}");
		
		Log.Information("SolutionParentFolder: " + WorkingDirectory.GetSolutionParentFolder());
		
		Log.Information(NoClient
			? "Client is: disabled"
			: "Client is: enabled");
		
		Log.Information(IsUnitTestDisabled()
			? "Unit test: disabled"
			: "Unit test: enabled");
		
		Log.Information(IsPublishDisabled()	
			? "Publish: disabled"
			: "Publish: enabled");
				
		Log.Information( NoSonar ||
		                string.IsNullOrEmpty(SonarQube.GetSonarKey()) ||
		                string.IsNullOrEmpty(SonarQube.GetSonarToken() )
			? "Sonarcloud scan: disabled"
			: "Sonarcloud scan: enabled");
		
		Log.Information(NoDependencies ? "External dependencies: disabled" : 
			"External dependencies: enabled");
		
		Log.Information(NoDependencies ? "External dependencies: disabled" : 
			"External dependencies: enabled");
		
		if ( !string.IsNullOrEmpty(GetBranchName()) )
		{
			Log.Information("(Overwrite) Branch:");
			Log.Information(GetBranchName());
		}

		if ( GetRuntimesWithoutGeneric().Count != 0 )
		{
			Log.Information("(Set) Runtime:");
			foreach ( var runtime in GetRuntimesWithoutGeneric() )
			{
				Log.Information($"- {runtime}");
			}
		}

		Log.Information("---");
	}
		
	Target ShowSettingsInformation => p => p
		.Executes(ShowSettingsInfo);

	/// <summary>
	/// Default Target
	/// </summary>
	Target Compile => p => p
		.DependsOn(ShowSettingsInformation)
		.DependsOn(Client)
		.DependsOn(SonarBuildTest)
		.DependsOn(BuildNetCoreRuntimeSpecific)
		.DependsOn(CoverageReport)
		.DependsOn(Zip);
		
	Target SonarBuildTest => p => p
		.DependsOn(Client)
		.Executes(() =>
		{
			ShowSettingsInfo();
			ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
			DotnetGenericHelper.RestoreNetCoreCommand(Solution);
			SonarQube.InstallSonarTool(IsUnitTestDisabled(), NoSonar);
			SonarQube.SonarBegin(IsUnitTestDisabled(),NoSonar,GetBranchName(), 
				ClientHelper.GetClientAppFolder(),
				"starskytest/coverage-merge-sonarqube.xml");
			DotnetGenericHelper.BuildNetCoreGenericCommand(Solution,Configuration);
			DotnetTestHelper.TestNetCoreGenericCommand(Configuration,IsUnitTestDisabled());
			DotnetGenericHelper.DownloadDependencies(Configuration, GeoCliCsproj
				,NoDependencies, GenericRuntimeName);
			MergeCoverageFiles.Merge(IsUnitTestDisabled());
			SonarQube.SonarEnd(IsUnitTestDisabled(),NoSonar);
			DotnetGenericHelper.PublishNetCoreGenericCommand(Configuration, IsPublishDisabled());
		});

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
		"S1144:UnusedMember.Local", Justification = "Not production code.")]
	// ReSharper disable once UnusedMember.Local
	Target DownloadDependencies => p => p
		.Executes(() =>
		{
			DotnetGenericHelper.DownloadDependencies(Configuration, 
				"starskygeocli/starskygeocli.csproj",NoDependencies, 
				GenericRuntimeName);
			DotnetRuntimeSpecificHelper.CopyDependenciesFiles(NoDependencies,
				GenericRuntimeName,GetRuntimesWithoutGeneric());
				
		});

	Target BuildNetCoreRuntimeSpecific => p => p
		.DependsOn(SonarBuildTest)
		.Executes(() =>
		{
			if ( GetRuntimesWithoutGeneric().Count == 0 || IsPublishDisabled() )
			{
				if ( IsPublishDisabled() )
				{
					Log.Information("Publish is disabled " + IsPublishDisabled());
					return;
				}

				Log.Information("There are no runtime specific items selected\n " +
				                "So skip BuildNetCoreRuntimeSpecific");
				
				return;
			}
				
			ShowSettingsInfo();
			DotnetRuntimeSpecificHelper.Clean(GetRuntimesWithoutGeneric());

			foreach ( var runtime in GetRuntimesWithoutGeneric() )
			{
				DotnetRuntimeSpecificHelper.BuildNetCoreCommand(Solution, Configuration, 
					runtime, IsReadyToRunEnabled());
				DotnetRuntimeSpecificHelper.PublishNetCoreGenericCommand(Configuration, 
					runtime, IsReadyToRunEnabled());
			}
			
			DotnetRuntimeSpecificHelper.CopyDependenciesFiles(NoDependencies,
				GenericRuntimeName,GetRuntimesWithoutGeneric());
		});
		
	// ReSharper disable once UnusedMember.Local
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
		"S1144:UnusedMember.Local", Justification = "Not production code.")]
	Target BuildNetCore => p => p
		.Executes(() =>
		{
			ShowSettingsInfo();
			ProjectCheckNetCoreCommandHelper.ProjectCheckNetCoreCommand();
			DotnetGenericHelper.RestoreNetCoreCommand(Solution);
			DotnetGenericHelper.BuildNetCoreGenericCommand(Solution,Configuration);
		});
		
	// ReSharper disable once UnusedMember.Local
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", 
		"S1144:UnusedMember.Local", Justification = "Not production code.")]
	Target TestNetCore => p => p
		.Executes(() =>
		{
			ShowSettingsInfo();
			DotnetTestHelper.TestNetCoreGenericCommand(Configuration,IsUnitTestDisabled());
		});
		
	Target Zip => p => p
		.DependsOn(Client)
		.DependsOn(SonarBuildTest)
		.DependsOn(BuildNetCoreRuntimeSpecific)
		.Executes(() =>
		{
			if ( IsPublishDisabled() )
			{
				Log.Information("Publish is disabled " + IsPublishDisabled());
				return;
			}
				
			ShowSettingsInfo();
			ZipperHelper.ZipGeneric();
			ZipperHelper.ZipRuntimes(GetRuntimesWithoutGeneric());
		});
			
	/// <summary>
	/// Generates html coverage report
	/// </summary>
	Target CoverageReport => p => p
		.DependsOn(Client)
		.DependsOn(SonarBuildTest)
		.Executes(() =>
		{
			ShowSettingsInfo();
			var folder = CoverageReportHelper.GenerateHtml(IsUnitTestDisabled());
			ZipperHelper.ZipHtmlCoverageReport(folder, IsUnitTestDisabled());
		});
}
