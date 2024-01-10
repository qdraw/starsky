using System.IO;
using Nuke.Common.ProjectModel;
using static helpers.GetSolutionAllProjects;

namespace helpers
{
	public static class ProjectAssetsCopier
	{
		public static void CopyAssetFileToCurrentRuntime(string runtime,
			Solution solution)
		{
			var allSolutionProjectsList = GetSolutionAllProjectsList(solution);
			// Restore Asset runtime file
			foreach ( var path in allSolutionProjectsList )
			{
				var parent = Directory.GetParent(path)?.FullName;
				var assetFile = $"{parent}/obj/project.assets.json";
				var assetRuntimeFile = $"{parent}/obj/project.assets_{runtime}.json";

				if ( File.Exists(assetRuntimeFile) )
				{
					File.Copy(assetRuntimeFile,assetFile,true);
				}

				// restore dg spec
				var projectName = Path.GetFileName(path).Replace(".csproj",string.Empty);
				var assetFileDgSpec = $"{parent}/obj/{projectName}.csproj.nuget.dgspec.json";
				var assetRuntimeDgSpec = $"{parent}/obj/{projectName}.csproj.nuget.dgspec_{runtime}.json";
				
				if ( File.Exists(assetRuntimeDgSpec) )
				{
					File.Copy(assetRuntimeDgSpec, assetFileDgSpec,true);
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
				}
				
				var projectName = Path.GetFileName(path).Replace(".csproj",string.Empty);
				var assetFileDgSpec = $"{parent}/obj/{projectName}.csproj.nuget.dgspec.json";
				var assetRuntimeDgSpec = $"{parent}/obj/{projectName}.csproj.nuget.dgspec_{runtime}.json";
				
				if ( File.Exists(assetRuntimeDgSpec) )
				{
					File.Copy(assetRuntimeDgSpec,assetFileDgSpec,true);
				}
			}
		}
	}
}
