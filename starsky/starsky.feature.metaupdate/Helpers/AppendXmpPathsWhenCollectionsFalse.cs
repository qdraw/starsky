using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Helpers;

namespace starsky.feature.metaupdate.Helpers;

public static class AppendXmpPathsWhenCollectionsFalseHelper
{
	public static List<string> AppendXmpPathsWhenCollectionsFalse(bool collections, List<string> inputFilePaths)
	{
		if ( collections )
		{
			return inputFilePaths;
		}

		var inputFilePathsWithXmpFiles = new List<string>();

		// append xmp files to list (does not need to exist on disk)
		// ReSharper disable once LoopCanBeConvertedToQuery
		foreach ( var inputFilePath in inputFilePaths.Where(ExtensionRolesHelper.IsExtensionForceXmp) )
		{
			inputFilePathsWithXmpFiles.Add(
				ExtensionRolesHelper.ReplaceExtensionWithXmp(
					inputFilePath));
		}

		inputFilePaths.AddRange(inputFilePathsWithXmpFiles);
		return inputFilePaths;
	}
}
