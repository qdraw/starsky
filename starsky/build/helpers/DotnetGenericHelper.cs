using System;
using System.IO;
using build;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static build.Build;

namespace helpers
{
	public static class DotnetGenericHelper
	{
		/// <summary>
		/// dotnet restore for generic
		/// </summary>
		/// <param name="solution">solution file .sln</param>
		public static void RestoreNetCoreCommand(Solution solution)
		{
			Log.Information("dotnet restore: solution: {Solution}", solution);

			DotNetRestore(p => p
				.SetProjectFile(solution.Path)
			);
		}

		/// <summary>
		/// dotnet build for generic helper
		/// </summary>
		/// <param name="solution">the solution</param>
		/// <param name="configuration">Debug or Release</param>
		public static void BuildNetCoreGenericCommand(Solution solution,
			Configuration configuration)
		{
			DotNetBuild(p => p
				.SetConfiguration(configuration)
				.EnableNoRestore()
				.EnableNoLogo()
				.SetProjectFile(solution));
		}

		/// <summary>
		/// Download Exiftool and geo deps
		/// </summary>
		/// <param name="configuration">is Release</param>
		/// <param name="geoCliCsproj">geo.csproj file</param>
		/// <param name="noDependencies">skip this step if true (external deps)</param>
		/// <param name="genericNetcoreFolder">genericNetcoreFolder</param>
		public static void DownloadDependencies(Configuration configuration,
			string geoCliCsproj, bool noDependencies,
			string genericNetcoreFolder)
		{
			if ( noDependencies )
			{
				Log.Information("skip the flag: --no-dependencies is used");
				return;
			}

			var genericDepsFullPath =
				Path.Combine(BasePath(), genericNetcoreFolder, "dependencies");
			Log.Information("genericDepsFullPath: {GenericDepsFullPath}", genericDepsFullPath);

			try
			{
				Environment.SetEnvironmentVariable("app__DependenciesFolder", genericDepsFullPath);
				Log.Information("Next: DownloadDependencies");
				Log.Information("Run: {Path}", Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(), geoCliCsproj)
				);

				DotNetRun(p => p
					.SetConfiguration(configuration)
					.EnableNoRestore()
					.EnableNoBuild()
					.SetApplicationArguments("--runtime linux-x64,win-x64")
					.SetProjectFile(Path.Combine(WorkingDirectory.GetSolutionParentFolder(),
						geoCliCsproj)));
			}
			catch ( Exception exception )
			{
				Log.Information("--");
				Log.Information(exception.Message);
				Log.Information("-- continue");
			}

			Environment.SetEnvironmentVariable("app__DependenciesFolder", string.Empty);

			Log.Information("   genericDepsFullPath: {GenericDepsFullPath}", genericDepsFullPath);
			Log.Information("DownloadDependencies done");
		}

		public static void PublishNetCoreGenericCommand(Configuration configuration,
			bool isPublishDisabled)
		{
			if ( isPublishDisabled )
			{
				Log.Information("Skip: PublishNetCoreGenericCommand isPublishDisabled");
				return;
			}

			foreach ( var publishProject in PublishProjectsList )
			{
				var publishProjectFullPath = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(),
					publishProject);

				var outputFullPath = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(),
					GenericRuntimeName);

				DotNetPublish(p => p
					.SetConfiguration(configuration)
					.EnableNoRestore()
					.EnableNoBuild()
					.EnableNoDependencies()
					.SetOutput(outputFullPath)
					.SetProject(publishProjectFullPath)
					.EnableNoLogo());
			}
		}

		static string BasePath()
		{
			return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
				?.Parent?.Parent?.Parent?.FullName!;
		}
	}
}
