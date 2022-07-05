using System;
using System.Collections.Generic;
using System.IO;
using build;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace helpers;

public static class DotnetRuntimeSpecificHelper
{
	public static void Clean(List<string> runtimesWithoutGeneric)
	{
		foreach(var runtime in runtimesWithoutGeneric)
		{
			var runtimeZip = $"{ZipperHelper.ZipPrefix}{runtime}.zip";
			Console.WriteLine("runtimeZip: " + runtimeZip + " exists:" + File.Exists(runtimeZip));

			if (File.Exists(runtimeZip))
			{
				File.Delete(runtimeZip);
			}
					
			if (Directory.Exists(Path.Combine(BasePath(), runtime)))
			{
				Console.WriteLine($"next rm folder - {Path.Combine(BasePath(), runtime)}");
				Directory.Delete(Path.Combine(BasePath(), runtime),true);
			}
			else
			{
				Console.WriteLine($"folder is not removed - {Path.Combine(BasePath(), runtime)}");
			}

			// todo!
			if (Directory.Exists($"obj/Release/net6.0/{runtime}"))
			{
				Directory.Delete($"obj/Release/net6.0/{runtime}",true);
			}
		}
	}

	public static void CopyDependenciesFiles(bool noDependencies,
		string genericNetcoreFolder, List<string> getRuntimesWithoutGeneric)
	{
		if ( noDependencies || string.IsNullOrWhiteSpace(genericNetcoreFolder) )
		{
			return;
		}

		var genericTempFolderFullPath =
			Path.Combine(BasePath(), genericNetcoreFolder, "dependencies");
		foreach ( var runtime in getRuntimesWithoutGeneric )
		{
			var runtimeTempFolder = Path.Combine(BasePath(), runtime, "dependencies");
			FileSystemTasks.CopyDirectoryRecursively(genericTempFolderFullPath, 
				runtimeTempFolder, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);
		}

	}

	public static void RestoreNetCoreCommand(Solution solution,
		List<string> runtimesWithoutGeneric)
	{
		foreach ( var runtime in runtimesWithoutGeneric )
		{
			ProjectAssetsCopier.CopyAssetFileToCurrentRuntime(runtime, solution);
			// OverwriteRuntimeIdentifier is done via Directory.Build.props
			DotNetRestore(_ => _
				.SetProjectFile(solution)
				.SetProcessArgumentConfigurator(args => args.Add($"/p:OverwriteRuntimeIdentifier={runtime}")));
			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(runtime, solution);
		}
	}
	
	
	public static void PublishNetCoreGenericCommand(Solution solution,
		List<string> runtimesWithoutGeneric, Configuration configuration)
	{
	    
		foreach ( var runtime in runtimesWithoutGeneric )
		{
			ProjectAssetsCopier.CopyAssetFileToCurrentRuntime(runtime, solution);
			foreach ( var publishProject in Build.PublishProjectsList )
			{
				DotNetPublish(_ => _
					.SetConfiguration(configuration)
					.EnableNoRestore()
					.EnableNoBuild()
					.EnableNoDependencies()
					.EnableSelfContained()
					.SetOutput(runtime)
					.SetProject(publishProject)
					.SetRuntime(runtime)
					.EnableNoLogo());
			}
			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(runtime, solution);
		}
	}

	public static void BuildNetCoreCommand(Solution solution, List<string> getRuntimesWithoutGeneric, Configuration configuration)
	{
		foreach ( var runtime in getRuntimesWithoutGeneric )
		{
			ProjectAssetsCopier.CopyAssetFileToCurrentRuntime(runtime, solution);
			// OverwriteRuntimeIdentifier is done via Directory.Build.props
			DotNetBuild(_ => _
				.SetProjectFile(solution)
				.EnableNoRestore()
				.EnableNoLogo()
				.SetConfiguration(configuration)
				.SetProcessArgumentConfigurator(args => 
					args
						.Add($"/p:OverwriteRuntimeIdentifier={runtime}")
						.Add("/p:noSonar=true")
				));
			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(runtime, solution);
		}
	}
	static string BasePath()
	{
		return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
			?.Parent?.Parent?.Parent?.FullName;
	}
}
