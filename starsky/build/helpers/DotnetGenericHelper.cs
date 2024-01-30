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
		public static void RestoreNetCoreCommand(Solution solution)
		{
			Log.Information("solution: " + solution);
			DotNetRestore(p => p
				.SetProjectFile(solution.Path)
			);
		}
	
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
		/// <param name="solution">where</param>
		/// <param name="configuration">is Release</param>
		/// <param name="geoCliCsproj">geo.csproj file</param>
		/// <param name="noDependencies">skip this step if true</param>
		/// <param name="genericNetcoreFolder">genericNetcoreFolder</param>
		public static void DownloadDependencies(Solution solution,
			Configuration configuration, string geoCliCsproj, bool noDependencies,
			string genericNetcoreFolder)
		{
			if ( noDependencies )
			{
				Log.Information("skip --no-dependencies");
				return;
			}
			
			var genericDepsFullPath = Path.Combine(BasePath(), genericNetcoreFolder, "dependencies");
			Log.Information($"genericDepsFullPath: {genericDepsFullPath}");
		
			try
			{
				Environment.SetEnvironmentVariable("app__DependenciesFolder",genericDepsFullPath);
				Log.Information("Next: DownloadDependencies");
				Log.Information("Run: " + Path.Combine(WorkingDirectory.GetSolutionParentFolder(),geoCliCsproj));

				DotNetRun(p =>  p
					.SetConfiguration(configuration)
					.EnableNoRestore()
					.EnableNoBuild()
					.SetProjectFile(Path.Combine(WorkingDirectory.GetSolutionParentFolder(),geoCliCsproj)));
			}
			catch ( Exception exception)
			{
				Log.Information("--");
				Log.Error(exception.Message);
				Log.Information("-- continue");
			}

			Environment.SetEnvironmentVariable("app__DependenciesFolder", string.Empty);

			Log.Information($"   genericDepsFullPath: {genericDepsFullPath}");
			Log.Information("DownloadDependencies done");
		}

		public static void PublishNetCoreGenericCommand(Solution solution,
			Configuration configuration, bool isPublishDisabled)
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
				?.Parent?.Parent?.Parent?.FullName;
		}
	}
	
}
