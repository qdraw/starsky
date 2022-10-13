using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using build;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace helpers
{
	/// <summary>
	/// use --skip to run only this test
	/// </summary>
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

				// get current netMoniker
				var version = System.Runtime.InteropServices.RuntimeInformation
					.FrameworkDescription;
				var netMoniker = new Regex(".\\d+$").Replace(version, string.Empty).Replace(".NET ","net");
				// e.g net6.0
				
				if (Directory.Exists($"obj/Release/{netMoniker}/{runtime}"))
				{
					Console.WriteLine($"remove -> obj/Release/{netMoniker}/{runtime}");
					Directory.Delete($"obj/Release/{netMoniker}/{runtime}",true);
				}
				else
				{
					Console.WriteLine($"folder is not removed -> obj/Release/{netMoniker}/{runtime}");
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
			
				// For Windows its not needed to copy unix dependencies 
				if ( runtime.StartsWith("win") && Directory.Exists(Path.Combine(runtimeTempFolder, "exiftool-unix")) )
				{
					Directory.Delete(Path.Combine(runtimeTempFolder, "exiftool-unix"), true);
					Console.WriteLine("removed exiftool-unix for windows");
				}
				// ReSharper disable once InvertIf
				if ( runtime.StartsWith("win") && File.Exists(Path.Combine(runtimeTempFolder, "exiftool.tar.gz")) )
				{
					File.Delete(Path.Combine(runtimeTempFolder, "exiftool.tar.gz"));
					Console.WriteLine("removed exiftool.tar.gz for windows");
				}
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

				// to check if the right runtime is published
				var runtimeDebugFile = Path.Combine(runtime, "_runtime_" + runtime + ".debug");
				if ( !File.Exists(runtimeDebugFile) )
				{
					File.Create(runtimeDebugFile).Close();
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
	
}
