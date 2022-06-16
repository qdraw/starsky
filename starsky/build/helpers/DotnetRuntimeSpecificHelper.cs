using System.Collections.Generic;
using build;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace helpers;

public static class DotnetRuntimeSpecificHelper
{
	
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
				.SetProcessArgumentConfigurator(args => args.Add($"/p:OverwriteRuntimeIdentifier={runtime}")));
			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(runtime, solution);
		}
	}

}
