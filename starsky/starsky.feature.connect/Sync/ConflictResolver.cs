using System;
using System.IO;

namespace starsky.feature.connect.Sync;

/// <summary>
/// Renames a locally-modified file to a .sync-conflict-* name when the
/// version vector comparison yields <see cref="starsky.foundation.connect.Sync.VectorRelation.Concurrent"/>.
/// </summary>
public sealed class ConflictResolver
{
	/// <summary>
	/// Renames <paramref name="fullPath"/> to the conflict name and returns the new path.
	/// The caller is responsible for then downloading the winning remote version.
	/// </summary>
	public string RenameToConflict(string fullPath, string deviceId)
	{
		var dir = Path.GetDirectoryName(fullPath) ?? string.Empty;
		var stem = Path.GetFileNameWithoutExtension(fullPath);
		var ext = Path.GetExtension(fullPath);
		var shortDevice = deviceId.Replace("-", "", StringComparison.Ordinal).Length >= 7
			? deviceId.Replace("-", "", StringComparison.Ordinal)[..7]
			: deviceId.Replace("-", "", StringComparison.Ordinal);

		var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
		var conflictName = $"{stem}.sync-conflict-{timestamp}-{shortDevice}{ext}";
		var conflictPath = Path.Combine(dir, conflictName);

		File.Move(fullPath, conflictPath, overwrite: false);
		return conflictPath;
	}
}
