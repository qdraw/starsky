using System.Collections.Generic;
using System.IO;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Helpers;

public class OllamaExePath(AppSettings appSettings)
{
	private const string OllamaDependenciesFolder = "ollama";
	private const string OllamaExecutableBaseName = "ollama";

	internal string GetExeParentFolder(string currentArchitecture)
	{
		return Path.Combine(appSettings.DependenciesFolder,
			string.IsNullOrEmpty(currentArchitecture)
				? OllamaDependenciesFolder
				: $"{OllamaDependenciesFolder}-{currentArchitecture}");
	}

	internal string GetExePath()
	{
		return GetExePath(CurrentArchitecture.GetCurrentRuntimeIdentifier());
	}

	internal string GetExePath(string currentArchitecture)
	{
		var exeFile = Path.Combine(GetExeParentFolder(currentArchitecture),
			OllamaExecutableBaseName);
		if ( currentArchitecture.StartsWith("win-") )
		{
			exeFile += ".exe";
		}

		return exeFile;
	}

	internal string GetConfiguredOrDefaultPath(string? currentArchitecture = null)
	{
		if ( IsValidExecutablePath(appSettings.OllamaExecutablePath) )
		{
			return appSettings.OllamaExecutablePath;
		}

		var runtime = currentArchitecture ?? CurrentArchitecture.GetCurrentRuntimeIdentifier();
		var candidatePaths = GetCandidateExePaths(runtime);
		foreach ( var candidatePath in candidatePaths )
		{
			if ( IsValidExecutablePath(candidatePath) )
			{
				return candidatePath;
			}
		}

		return GetExePath(runtime);
	}

	internal bool IsValidExecutablePath(string? executablePath)
	{
		return !string.IsNullOrWhiteSpace(executablePath) && File.Exists(executablePath);
	}

	private IEnumerable<string> GetCandidateExePaths(string currentArchitecture)
	{
		yield return GetExePath(currentArchitecture);
		foreach ( var alias in GetArchitectureAliases(currentArchitecture) )
		{
			yield return GetExePath(alias);
		}
	}

	private static IEnumerable<string> GetArchitectureAliases(string currentArchitecture)
	{
		switch ( currentArchitecture )
		{
			case "linux-x64":
				yield return "linux-amd64";
				break;
			case "linux-amd64":
				yield return "linux-x64";
				break;
			case "osx-x64":
				yield return "darwin-amd64";
				break;
			case "darwin-amd64":
				yield return "osx-x64";
				break;
			case "osx-arm64":
				yield return "darwin-arm64";
				break;
			case "darwin-arm64":
				yield return "osx-arm64";
				break;
		}
	}
}


