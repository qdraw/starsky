using System.Collections.Generic;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.rename.RelatedFilePaths;

public class ReleatedFilePaths(IStorage storage)
{
	/// <summary>
	///     Get all related file paths (sidecars) for a given file
	/// </summary>
	internal List<(string source, string target)> GetRelatedFilePaths(string sourceFilePath,
		string targetFilePath)
	{
		var related = new List<(string, string)>();

		// Check for JSON sidecar
		var sourceJson = JsonSidecarLocation.JsonLocation(sourceFilePath);
		if ( storage.ExistFile(sourceJson) )
		{
			var targetJson = JsonSidecarLocation.JsonLocation(targetFilePath);
			related.Add(( sourceJson, targetJson ));
		}

		// Check for XMP sidecar
		if ( !ExtensionRolesHelper.IsExtensionForceXmp(sourceFilePath) )
		{
			return related;
		}

		var sourceXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(sourceFilePath);
		if ( !storage.ExistFile(sourceXmp) )
		{
			return related;
		}

		var targetXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(targetFilePath);
		related.Add(( sourceXmp, targetXmp ));

		return related;
	}
}
