using System;
using System.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Build;

namespace helpers
{
	public static class DotnetGenericHelper
	{
		public static void RestoreNetCoreCommand(Solution solution)
		{
			ProjectAssetsCopier.CopyAssetFileToCurrentRuntime(GenericRuntimeName, solution);
			DotNetRestore(_ => _
				.SetProjectFile(solution));
			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(GenericRuntimeName, solution);
		}
	
		public static void BuildNetCoreGenericCommand(Solution solution,
			Configuration configuration)
		{
			ProjectAssetsCopier.CopyAssetFileToCurrentRuntime(GenericRuntimeName, solution);
		    
			DotNetBuild(_ => _
				.SetConfiguration(configuration)
				.EnableNoRestore()
				.EnableNoLogo()
				.SetProjectFile(solution));

			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(GenericRuntimeName, solution);
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
		
			ProjectAssetsCopier.CopyAssetFileToCurrentRuntime(GenericRuntimeName, solution);

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
			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(GenericRuntimeName, solution);

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
			
			ProjectAssetsCopier.CopyAssetFileToCurrentRuntime(GenericRuntimeName, solution);

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
					.EnableSelfContained()
					.SetOutput(outputFullPath)
					.SetProject(publishProjectFullPath)
					.EnableNoLogo());
			}
			ProjectAssetsCopier.CopyNewAssetFileByRuntimeId(GenericRuntimeName, solution);
		}
	
		static string BasePath()
		{
			return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
				?.Parent?.Parent?.Parent?.FullName;
		}
	}
	
}
