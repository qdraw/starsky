using System;
using System.IO;
using static SimpleExec.Command;
using static build.Build;

namespace helpers
{
	public static class ProjectCheckNetCoreCommandHelper
	{
		static string GetBuildToolsFolder()
		{
			var baseDirectory = AppDomain.CurrentDomain?
				.BaseDirectory;
			if ( baseDirectory == null )
				throw new DirectoryNotFoundException("base directory is null, this is wrong");
			var slnRootDirectory = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
			if ( slnRootDirectory == null )
				throw new DirectoryNotFoundException("slnRootDirectory is null, this is wrong");
			return Path.Combine(slnRootDirectory, BuildToolsPath);
		}
		
		public static void ProjectCheckNetCoreCommand()
		{
			ClientHelper.NpmPreflight();

			// check branch names on CI
			// release-version-check.js triggers app-version-update.js to update the csproj and package.json files
			Run(NpmBaseCommand, "run release-version-check", GetBuildToolsFolder());
		
			/* Checks for valid Project GUIDs in csproj files */
			Run(NpmBaseCommand, "run project-guid", GetBuildToolsFolder());
		
			/* List of nuget packages */
			Run(NpmBaseCommand, "run nuget-package-list", GetBuildToolsFolder());
		}

		const string BuildToolsPath = "starsky-tools/build-tools/";
	}
}


