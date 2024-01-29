using System;
using System.IO;
using Serilog;

namespace helpers
{
	public static class MergeCoverageFiles
	{
		static void Information(string value)
		{
			Log.Information(value);
		}
	
		static bool FileExists(string value)
		{
			return File.Exists(value);
		}
	
		static void DeleteFile(string value)
		{
			File.Delete(value);
		}
	
		static void MoveFile(string from, string to)
		{
			File.Move(from,to, true);
		}
	
		static void CopyFile(string from, string to)
		{
			File.Copy(from,to, true);
		}
	
		public static void Merge(bool noUnitTest)
		{
			var rootDirectory = Directory.GetParent(AppDomain.CurrentDomain
				.BaseDirectory!)!.Parent!.Parent!.Parent!.FullName;
		
			if(noUnitTest)
			{
				Information($">> MergeCoverageFiles is disable due the --no-unit-test flag");
				return;
			}

			var jestCoverageFile = Path.Combine(rootDirectory, "starsky/clientapp/coverage/cobertura-coverage.xml");
			if (! FileExists(jestCoverageFile)) {
				throw new FileNotFoundException($"Missing jest coverage file {jestCoverageFile}");
			}

			var netCoreCoverageFile = Path.Combine(rootDirectory, "starskytest/netcore-coverage.opencover.xml");
			if (! FileExists(netCoreCoverageFile)) {
				throw new FileNotFoundException($"Missing .NET coverage file ${netCoreCoverageFile}");
			}

			var outputCoverageFile = Path.Combine(rootDirectory,"starskytest/coverage-merge-cobertura.xml");

			if (FileExists(outputCoverageFile)) {
				DeleteFile(outputCoverageFile);
			}

			var outputCoverageSonarQubeFile = Path.Combine(rootDirectory,"starskytest/coverage-merge-sonarqube.xml");

			if (FileExists(outputCoverageSonarQubeFile)) {
				DeleteFile(outputCoverageSonarQubeFile);
			}

			// Gets the coverage file from the client folder
			if (FileExists($"./starsky/clientapp/coverage/cobertura-coverage.xml")) {
				Information($"Copy ./starsky/clientapp/coverage/cobertura-coverage.xml ./starskytest/jest-coverage.cobertura.xml");
				CopyFile($"./starsky/clientapp/coverage/cobertura-coverage.xml", $"./starskytest/jest-coverage.cobertura.xml");
			}
		
			var args = new []
			{
				$"-reports:{rootDirectory}/starskytest/*coverage.*.xml",
				$"-targetdir:{rootDirectory}/starskytest/",
				$"-reporttypes:Cobertura;SonarQube"
			};
		
			Palmmedia.ReportGenerator.Core.Program.Main(args);

			// And rename it
			MoveFile($"{rootDirectory}/starskytest/Cobertura.xml", outputCoverageFile);
			MoveFile($"{rootDirectory}/starskytest/SonarQube.xml", outputCoverageSonarQubeFile);
			
			if (!FileExists(outputCoverageSonarQubeFile)) {
				throw new FileNotFoundException($"Missing Sonarqube coverage file {outputCoverageSonarQubeFile}");
			}
			Information($"Sonarqube Coverage file is ready: {outputCoverageSonarQubeFile}");

			if (!FileExists(outputCoverageFile)) {
				throw new FileNotFoundException($"Missing Cobertura coverage file {outputCoverageFile}");
			}
			Information($"Cobertura Coverage file is ready: {outputCoverageFile}");

		}
	}
	
}
