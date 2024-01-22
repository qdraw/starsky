using System;
using System.IO;
using build;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static build.Build;

namespace helpers
{
	public static class DotnetGenericHelper
	{
		public static void RestoreNetCoreCommand(Solution solution)
		{
			Console.WriteLine("solution: " + solution);
			DotNetRestore(p => p
				.SetProjectFile(solution.Path)
			);
		}
	
		public static void BuildNetCoreGenericCommand(Solution solution,
			Configuration configuration)
		{
			DotNetBuild(_ => _
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
				Console.WriteLine("skip --no-dependencies");
				return;
			}
			
			var genericDepsFullPath = Path.Combine(BasePath(), genericNetcoreFolder, "dependencies");
			Console.WriteLine($"genericDepsFullPath: {genericDepsFullPath}");
		
			try
			{
				Environment.SetEnvironmentVariable("app__DependenciesFolder",genericDepsFullPath);
				Console.WriteLine("Next: DownloadDependencies");
				Console.WriteLine("Run: " + Path.Combine(WorkingDirectory.GetSolutionParentFolder(),geoCliCsproj));

				DotNetRun(_ =>  _
					.SetConfiguration(configuration)
					.EnableNoRestore()
					.EnableNoBuild()
					.SetProjectFile(Path.Combine(WorkingDirectory.GetSolutionParentFolder(),geoCliCsproj)));
			}
			catch ( Exception exception)
			{
				Console.WriteLine("--");
				Console.WriteLine(exception.Message);
				Console.WriteLine("-- continue");
			}

			Environment.SetEnvironmentVariable("app__DependenciesFolder", string.Empty);

			Console.WriteLine($"   genericDepsFullPath: {genericDepsFullPath}");
			Console.WriteLine("DownloadDependencies done");
		}

		public static void PublishNetCoreGenericCommand(Solution solution,
			Configuration configuration, bool isPublishDisabled)
		{
			if ( isPublishDisabled )
			{
				Console.WriteLine("Skip: PublishNetCoreGenericCommand isPublishDisabled");
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

				DotNetPublish(_ => _
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
