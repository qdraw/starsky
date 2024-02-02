using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using build;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
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
			if ( runtimesWithoutGeneric.Count == 0 )
			{
				return;
			}

			Log.Information("Next: Clean up the folder and zip files");

			foreach ( var runtime in runtimesWithoutGeneric )
			{
				var runtimeZip = $"{ZipperHelper.ZipPrefix}{runtime}.zip";

				Log.Information("\tRuntimeZip: " + runtimeZip + " exists: " +
				                File.Exists(runtimeZip));
				if ( File.Exists(runtimeZip) )
				{
					File.Delete(runtimeZip);
				}

				if ( Directory.Exists(Path.Combine(BasePath(), runtime)) )
				{
					Log.Information($"\tnext rm folder - {Path.Combine(BasePath(), runtime)}");
					Directory.Delete(Path.Combine(BasePath(), runtime), true);
				}
				else
				{
					Log.Information(
						$"\tskip folder due not exists - {Path.Combine(BasePath(), runtime)}");
				}

				// get current netMoniker
				var version = System.Runtime.InteropServices.RuntimeInformation
					.FrameworkDescription;
				var netMoniker =
					new Regex(".\\d+$", RegexOptions.None, TimeSpan.FromMilliseconds(100))
						.Replace(version, string.Empty).Replace(".NET ", "net");
				// e.g net6.0 or net8.0

				if ( Directory.Exists($"obj/Release/{netMoniker}/{runtime}") )
				{
					Log.Information($"\tNext: remove -> obj/Release/{netMoniker}/{runtime}");
					Directory.Delete($"obj/Release/{netMoniker}/{runtime}", true);
				}
				else
				{
					Log.Information(
						$"\tfolder is not removed -> obj/Release/{netMoniker}/{runtime}");
				}
			}

			Log.Information("Clean up done");
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
				if ( runtime.StartsWith("win") &&
				     Directory.Exists(Path.Combine(runtimeTempFolder, "exiftool-unix")) )
				{
					Directory.Delete(Path.Combine(runtimeTempFolder, "exiftool-unix"), true);
					Log.Information("removed exiftool-unix for windows");
				}

				// ReSharper disable once InvertIf
				if ( runtime.StartsWith("win") &&
				     File.Exists(Path.Combine(runtimeTempFolder, "exiftool.tar.gz")) )
				{
					File.Delete(Path.Combine(runtimeTempFolder, "exiftool.tar.gz"));
					Log.Information("removed exiftool.tar.gz for windows");
				}
			}
		}

		/// <summary>
		/// Specific build for runtime
		/// Runs the command: dotnet build
		/// </summary>
		/// <param name="solution">the solution file (sln)</param>
		/// <param name="configuration">Config file</param>
		/// <param name="runtime">which runtime e.g. linux-arm or osx-x64</param>
		/// <param name="isReadyToRunEnabled">Is Ready To Run Enabled</param>
		public static void BuildNetCoreCommand(Solution solution, Configuration
			configuration, string runtime, bool isReadyToRunEnabled)
		{
			Log.Information("> dotnet build next for: solution: " + solution + " runtime: " +
			                runtime);

			var readyToRunArgument =
				RuntimeIdentifier.IsReadyToRunSupported(runtime) && isReadyToRunEnabled
					? "-p:PublishReadyToRun=true"
					: "";

			DotNetBuild(p => p
				.SetProjectFile(solution)
				// Implicit restore here, since .NET 8 self contained is disabled
				// in dotnet restore and there a no options to enable it
				.EnableNoLogo()
				.DisableRunCodeAnalysis()
				.EnableSelfContained()
				.SetConfiguration(configuration)
				.SetProcessArgumentConfigurator(args =>
					args
						// OverwriteRuntimeIdentifier is done via Directory.Build.props
						// Building a solution with a specific RuntimeIdentifier is not supported
						// Dont use SetRuntime
						.Add($"/p:OverwriteRuntimeIdentifier={runtime}")
						// Warnings are disabled because in Generic build they are already checked
						.Add("-v q")
						.Add("/p:WarningLevel=0")
						// SonarQube analysis is done in the generic build
						.Add("/p:noSonar=true")
						.Add(readyToRunArgument)
				));
		}

		/// <summary>
		/// Runs the command: dotnet publish
		/// For RuntimeSpecific
		/// </summary>
		/// <param name="configuration">Release</param>
		/// <param name="runtime">runtime identifier</param>
		/// <param name="isReadyToRunEnabled">Is Ready To Run Enabled</param>
		public static void PublishNetCoreGenericCommand(Configuration configuration,
			string runtime, bool isReadyToRunEnabled)
		{
			foreach ( var publishProject in Build.PublishProjectsList )
			{
				Log.Information(">> next publishProject: " +
				                publishProject + " runtime: " + runtime);

				var publishProjectFullPath = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(),
					publishProject);

				var outputFullPath = Path.Combine(
					WorkingDirectory.GetSolutionParentFolder(),
					runtime);

				var readyToRunArgument =
					RuntimeIdentifier.IsReadyToRunSupported(runtime) &&
					isReadyToRunEnabled &&
					Build.ReadyToRunProjectsList.Contains(publishProject)
						? "-p:PublishReadyToRun=true"
						: "";

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
					.SetProcessArgumentConfigurator(args =>
						args.Add("/p:noSonar=true")
							.Add($"/p:OverwriteRuntimeIdentifier={runtime}")
							.Add(readyToRunArgument)
					)
				);
			}
		}
	}
}
