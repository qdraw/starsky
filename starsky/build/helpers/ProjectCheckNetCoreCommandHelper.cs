using static SimpleExec.Command;
using static build.Build;

namespace helpers;

public static class ProjectCheckNetCoreCommandHelper
{
	public static void ProjectCheckNetCoreCommand()
	{
		ClientHelper.NpmPreflight();

		// check branch names on CI
		Run(NpmBaseCommand, "run release-version-check", BuildToolsPath());
		
		/* Checks for valid Project GUIDs in csproj files */
		Run(NpmBaseCommand, "run project-guid", BuildToolsPath());
		
		/* List of nuget packages */
		Run(NpmBaseCommand, "run nuget-package-list", BuildToolsPath());
	}

	static string  BuildToolsPath()
	{
		return "../starsky-tools/build-tools/";
	}
}
