using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using helpers;
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

namespace helpers;

public static class SonarQube
{
	public const string SonarQubePackageName = "dotnet-sonarscanner";
	public const string SonarQubePackageVersion = "5.6.0";

	public static void InstallSonarTool()
	{

		var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain
			.BaseDirectory).Parent.Parent.Parent.FullName;
			
		Console.WriteLine(rootDirectory);

		var envs =
			Environment.GetEnvironmentVariables() as
				IReadOnlyDictionary<string, string>;
		
		var r = DotNet($"new tool-manifest --force", rootDirectory, envs, null, true);

		DotNetToolInstall(_ => _
			.SetPackageName(SonarQubePackageName)
			//.SetProcessArgumentConfigurator(_ => _.Add("-d"))
			.SetProcessWorkingDirectory(rootDirectory)
			// .EnableGlobal()
			.SetVersion(SonarQubePackageVersion));


		DotNet($"run {SonarQubePackageName}", rootDirectory, null, null, true);

	}
}
