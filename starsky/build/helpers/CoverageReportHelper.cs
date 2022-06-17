using System;
using System.IO;

namespace helpers;

public static class CoverageReportHelper
{
	static void Information(string value)
	{
		Console.WriteLine(value);
	}

	public static string GenerateHtml(bool noUnitTest)
	{
		if ( noUnitTest )
		{
			Information(
				$">> MergeCoverageFiles is disable due the --no-unit-test flag");
			return null;
		}
		
		var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain
			.BaseDirectory).Parent.Parent.Parent.FullName;
		var outputCoverageFile = Path.Combine(rootDirectory,
			"./starskytest/coverage-merge-cobertura.xml");

		var reportFolder =
			outputCoverageFile.Replace("merge-cobertura.xml", "report");


		if ( !File.Exists(outputCoverageFile) )
		{
			throw new Exception(
				$"Missing .NET Core coverage file {outputCoverageFile}");
		}

		var args = new[]
		{
			$"-reports:{outputCoverageFile}", 
			$"-targetdir:{reportFolder}",
			$"-reporttypes:HtmlInline"
		};
		Palmmedia.ReportGenerator.Core.Program.Main(args);
		
		return reportFolder;
	}

}
