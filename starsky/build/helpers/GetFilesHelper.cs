using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace helpers;

public static class GetFilesHelper
{
	public static List<string> GetFiles(string globSearch)
	{
		Matcher matcher = new();
		matcher.AddIncludePatterns( new List<string>{globSearch});
		var result = matcher.Execute(
			new DirectoryInfoWrapper(
				new DirectoryInfo(WorkingDirectory.GetSolutionParentFolder())));
		return result.Files.Select(p => p.Path).ToList();
	}
}
