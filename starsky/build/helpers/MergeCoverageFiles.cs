using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers;

public static class MergeCoverageFiles
{
	public static void Merge()
	{
		
		if(noUnitTest)
		{
			Information($">> MergeCoverageFiles is disable due the --no-unit-test flag");
			return;
		}

		if (! FileExists($"./starsky/clientapp/coverage/cobertura-coverage.xml")) {
			throw new Exception($"Missing jest coverage file ./starsky/clientapp/coverage/cobertura-coverage.xml");
		}

		if (! FileExists("./starskytest/netcore-coverage.opencover.xml")) {
			throw new Exception($"Missing .NET Core coverage file ./starskytest/netcore-coverage.opencover.xml");
		}

		var outputCoverageFile = $"./starskytest/coverage-merge-cobertura.xml";

		if (FileExists(outputCoverageFile)) {
			DeleteFile(outputCoverageFile);
		}

		var outputCoverageSonarQubeFile = $"./starskytest/coverage-merge-sonarqube.xml";

		if (FileExists(outputCoverageSonarQubeFile)) {
			DeleteFile(outputCoverageSonarQubeFile);
		}

		// Gets the coverage file from the client folder
		if (FileExists($"./starsky/clientapp/coverage/cobertura-coverage.xml")) {
			Information($"Copy ./starsky/clientapp/coverage/cobertura-coverage.xml ./starskytest/jest-coverage.cobertura.xml");
			CopyFile($"./starsky/clientapp/coverage/cobertura-coverage.xml", $"./starskytest/jest-coverage.cobertura.xml");
		}


		Palmmedia.ReportGenerator.Core.Program.Main(new []{"--help"});
	    
		IEnumerable<string> redirectedStandardOutput;
		IEnumerable<string> redirectedErrorOutput;
		var exitCodeWithArgument =
			StartProcess(
				"dotnet",
				new ProcessSettings {
					Arguments = new ProcessArgumentBuilder()
						.Append($"reportgenerator")
						.Append($"-reports:./starskytest/*coverage.*.xml")
						.Append($"-targetdir:./starskytest/")
						.Append($"-reporttypes:Cobertura;SonarQube"),
					RedirectStandardOutput = true,
					RedirectStandardError = true
				},
				out redirectedStandardOutput,
				out redirectedErrorOutput
			);

		// Output process output.
		foreach(var stdOutput in redirectedStandardOutput)
		{
			Information("reportgenerator: {0}", stdOutput);
		}

		// Throw exception if anything was written to the standard error.
		if (redirectedErrorOutput.Any())
		{
			throw new Exception(
				string.Format(
					"Errors occurred: {0}",
					string.Join(", ", redirectedErrorOutput)));
		}

		// This should output 0 as valid arguments supplied
		Information("Exit code: {0}", exitCodeWithArgument);

		// And rename it
		MoveFile($"./starskytest/Cobertura.xml", outputCoverageFile);
		MoveFile($"./starskytest/SonarQube.xml", outputCoverageSonarQubeFile);
	}
}
