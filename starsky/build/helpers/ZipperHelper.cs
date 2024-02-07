using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using build;
using Serilog;

namespace helpers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage",
		"S1118:Add a 'protected' constructor " +
		"or the 'static' keyword to the class declaration", Justification = "Not production code.")]
	public sealed class ZipperHelper
	{
		public const string ZipPrefix = "starsky-";

		static string BasePath()
		{
			return Directory.GetParent(AppDomain.CurrentDomain
				.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
		}

		public static void ZipGeneric()
		{
			var fromFolder = Path.Join(BasePath(), Build.GenericRuntimeName);

			if ( !Directory.Exists(fromFolder) )
			{
				throw new DirectoryNotFoundException(
					$"dir {Build.GenericRuntimeName} not found {fromFolder}");
			}

			var zipPath = Path.Join(BasePath(),
				ZipPrefix + Build.GenericRuntimeName + ".zip");

			if ( File.Exists(zipPath) )
			{
				File.Delete(zipPath);
			}

			Log.Information($"next: {Build.GenericRuntimeName} zip  ~ {fromFolder} -> {zipPath}");
			ZipFile.CreateFromDirectory(fromFolder,
				zipPath);
		}

		public static void ZipRuntimes(List<string> getRuntimesWithoutGeneric)
		{
			if ( getRuntimesWithoutGeneric.Count == 0 )
			{
				Log.Information("There are no runtime specific items selected\n" +
				                "So skip ZipRuntimes");
				return;
			}

			foreach ( var runtime in getRuntimesWithoutGeneric )
			{
				var runtimeFullPath = Path.Join(BasePath(), runtime);

				if ( !Directory.Exists(runtimeFullPath) )
				{
					throw new DirectoryNotFoundException(
						$"dir {Build.GenericRuntimeName} not found ~ {runtimeFullPath}");
				}

				var zipPath = Path.Join(BasePath(),
					ZipPrefix + runtime + ".zip");

				if ( File.Exists(zipPath) )
				{
					File.Delete(zipPath);
				}

				Log.Information($"next: {runtime} zip ~ {runtimeFullPath} -> {zipPath}");

				ZipFile.CreateFromDirectory(runtimeFullPath, zipPath);
			}
		}

		const string CoverageReportZip = "coverage-report.zip";

		public static void ZipHtmlCoverageReport(string? fromFolder,
			bool noUnitTest)
		{
			if ( noUnitTest )
			{
				Log.Information(">> ZipHtmlCoverageReport " +
				                "is disable due the --no-unit-test flag\n" +
				                "So skip ZipHtmlCoverageReport");
				return;
			}

			if ( string.IsNullOrEmpty(fromFolder) || !Directory.Exists(fromFolder) )
			{
				throw new DirectoryNotFoundException($"dir {fromFolder} not found");
			}

			var zipPath = Path.Join(BasePath(), "starskytest", CoverageReportZip);

			if ( File.Exists(zipPath) )
			{
				File.Delete(zipPath);
			}

			Log.Information($"next: zip {fromFolder} -> {zipPath}");
			ZipFile.CreateFromDirectory(fromFolder,
				zipPath);
		}
	}
}
