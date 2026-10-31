using System;
using System.Collections.Generic;

namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     Settings that differ per tenant: where the files of a tenant live.
///     Process-wide settings stay on AppSettings.
/// </summary>
public interface ITenantSettings
{
	string TenantId { get; }

	string StorageFolder { get; }

	string ThumbnailTempFolder { get; }

	string TempFolder { get; }

	IReadOnlyDictionary<string, string> StorageFolderMappings { get; }

	bool IsReadOnly(string path);

	/// <summary>
	///     Database style subPath to a full path, never outside the storage folder
	/// </summary>
	string DatabasePathToFilePath(string databaseFilePath);

	string DatabasePathToTempFolderFilePath(string databaseFilePath);

	List<string> RenameListItemsToDbStyle(IEnumerable<string> localSubFolderList);

	IEnumerable<KeyValuePair<string, DateTime>> RenameListItemsToDbStyle(
		IEnumerable<KeyValuePair<string, DateTime>> localSubFolderList);

	List<string> RenameTempListItemsToDbStyle(IEnumerable<string> localSubFolderList);
}
