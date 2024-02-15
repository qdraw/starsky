using System.Collections.Generic;
using starsky.foundation.native.OpenApplicationNative;
using starsky.foundation.native.OpenApplicationNative.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIOpenApplicationNativeService : IOpenApplicationNativeService
{
	private readonly List<string> _fullFilePaths;
	private readonly string _applicationUrl;

	public FakeIOpenApplicationNativeService(List<string> fullPaths, string applicationUrl)
	{
		_fullFilePaths = fullPaths;
		_applicationUrl = applicationUrl;
	}

	public string FindPath(List<string> fullPaths)
	{
		var fullFilePath = string.Empty;
		foreach ( var path in fullPaths )
		{
			var findPath = _fullFilePaths.Find(p => p == path);
			if ( findPath != null )
			{
				fullFilePath = findPath;
			}
		}

		return fullFilePath;
	}

	public bool? OpenApplicationAtUrl(List<(string, string)> fullPathAndApplicationUrl)
	{
		var filesByApplicationPath =
			OpenApplicationNativeService
				.SortToOpenFilesByApplicationPath(fullPathAndApplicationUrl);

		var results = new List<bool?>();
		foreach ( var (fullFilePaths, applicationPath) in filesByApplicationPath )
		{
			results.Add(OpenApplicationAtUrl(fullFilePaths, applicationPath));
		}

		return results.TrueForAll(p => p == true);
	}

	public bool? OpenApplicationAtUrl(List<string> fullPaths, string applicationUrl)
	{
		var findPath = FindPath(fullPaths);
		return !string.IsNullOrEmpty(findPath) && applicationUrl == _applicationUrl;
	}

	public bool? OpenDefault(List<string> fullPaths)
	{
		var findPath = FindPath(fullPaths);
		return !string.IsNullOrEmpty(findPath);
	}
}
