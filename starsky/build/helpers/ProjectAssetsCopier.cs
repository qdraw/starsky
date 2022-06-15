using System.IO;
using Nuke.Common.ProjectModel;
using static helpers.GetSolutionAllProjects;
using static helpers.SonarQube;

namespace helpers;

public static class ProjectAssetsCopier
{
	public static void CopyAssetFileToCurrentRuntime(string runtime,
		Solution solution)
	{
		// Restore Asset runtime file
		foreach ( var path in GetSolutionAllProjectsList(solution) )
		{
			var parent = Directory.GetParent(path)?.FullName;
			var assetFile = $"{parent}/obj/project.assets.json";
			var assetRuntimeFile = $"{parent}/obj/project.assets_{runtime}.json";

			if ( File.Exists(assetRuntimeFile) )
			{
				File.Copy(assetRuntimeFile,assetFile,true);
				// // Restore
				// CopyFile(assetRuntimeFile,assetFile, FileExistsPolicy.Overwrite, false);
			}
		}
	}
	
	
	public static void CopyNewAssetFileByRuntimeId(string runtime,
		Solution solution)
	{
		// Create a new one
		foreach ( var path in GetSolutionAllProjectsList(solution) )
		{
			var parent = Directory.GetParent(path)?.FullName;
			var assetFile = $"{parent}/obj/project.assets.json";
			var assetRuntimeFile = $"{parent}/obj/project.assets_{runtime}.json";
			if ( File.Exists(assetFile) )
			{
				File.Copy(assetFile,assetRuntimeFile,true);
				// CopyFile(assetFile,assetRuntimeFile, FileExistsPolicy.Overwrite);
			}
		}
	}
}
