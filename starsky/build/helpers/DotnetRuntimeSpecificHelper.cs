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
		static string BasePath()
		{
			return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
				?.Parent?.Parent?.Parent?.FullName;
		}
		
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
				var netMoniker = new Regex(".\\d+$", RegexOptions.None, TimeSpan.FromMilliseconds(100))
					.Replace(version, string.Empty).Replace(".NET ","net");
				// e.g net6.0 or net8.0
				
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

		public static void RestoreNetCoreCommand(Solution solution, string runtime)
		{
			Console.WriteLine("> dotnet restore next for: solution: " + solution + " runtime: " + runtime);
			// OverwriteRuntimeIdentifier is done via Directory.Build.props
			DotNetRestore(p => p
				.SetProjectFile(solution)
				.SetRuntime(runtime)
				.SetProcessArgumentConfigurator(args => args
					.Add($"/p:OverwriteRuntimeIdentifier={runtime}")
					.Add("/p:noSonar=true")));
		}

		public static void BuildNetCoreCommand(Solution solution, Configuration configuration, string runtime)
		{
			Console.WriteLine("> dotnet build next for: solution: " + solution + " runtime: " + runtime);
			
			// OverwriteRuntimeIdentifier is done via Directory.Build.props
			// search for: dotnet build
			DotNetBuild(p => p
				.SetProjectFile(solution)
				.EnableNoRestore()
				.EnableNoLogo()
				.DisableRunCodeAnalysis()
				.SetConfiguration(configuration)
				.SetProcessArgumentConfigurator(args => 
					args
						.Add($"/p:OverwriteRuntimeIdentifier={runtime}")
						// Warnings are disabled because in Generic build they are already checked
						.Add("-v q")
						.Add("/p:WarningLevel=0")
						.Add("/p:noSonar=true")
				));
		}

		public static void PublishNetCoreGenericCommand(Configuration configuration, string runtime)
		{
			foreach ( var publishProject in Build.PublishProjectsList )
			{
				Console.WriteLine(">> next publishProject: " + publishProject + " runtime: " + runtime);
				
				var publishProjectFullPath = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(),
					publishProject);
					
				var outputFullPath = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(),
					runtime);
					
				DotNetPublish(p => p
					.SetConfiguration(configuration)
					.EnableNoRestore()
					.EnableNoBuild()
					.EnableNoDependencies()
					.EnableSelfContained()
					.SetOutput(outputFullPath)
					.SetProject(publishProjectFullPath)
					.SetRuntime(runtime)
					.EnableNoLogo()
					.SetProcessArgumentConfigurator(args => args.Add("/p:noSonar=true"))
				);
			}
		}
	}
	
}
