using System;
using System.Collections.Generic;
using starsky.foundation.platform.Models;

namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     The path logic (traversal protection, db style names) lives on AppSettings, so a tenant
///     wraps an AppSettings that only carries its own folders. For the default tenant that is
///     the live process-wide instance, so edits made at runtime keep working.
/// </summary>
public sealed class TenantSettings : ITenantSettings
{
	private readonly AppSettings _paths;

	public TenantSettings(string tenantId, AppSettings pathSettings)
	{
		if ( !TenantIdHelper.IsValid(tenantId) )
		{
			throw new ArgumentException("Invalid tenant id", nameof(tenantId));
		}

		TenantId = tenantId;
		_paths = pathSettings;
	}

	public string TenantId { get; }
	public string StorageFolder => _paths.StorageFolder;
	public string ThumbnailTempFolder => _paths.ThumbnailTempFolder;
	public string TempFolder => _paths.TempFolder;
	public IReadOnlyDictionary<string, string> StorageFolderMappings =>
		_paths.StorageFolderMappings;

	public bool IsReadOnly(string path)
	{
		return _paths.IsReadOnly(path);
	}

	public string DatabasePathToFilePath(string databaseFilePath)
	{
		return _paths.DatabasePathToFilePath(databaseFilePath);
	}

	public string DatabasePathToTempFolderFilePath(string databaseFilePath)
	{
		return _paths.DatabasePathToTempFolderFilePath(databaseFilePath);
	}

	public List<string> RenameListItemsToDbStyle(IEnumerable<string> localSubFolderList)
	{
		return _paths.RenameListItemsToDbStyle(localSubFolderList);
	}

	public IEnumerable<KeyValuePair<string, DateTime>> RenameListItemsToDbStyle(
		IEnumerable<KeyValuePair<string, DateTime>> localSubFolderList)
	{
		return _paths.RenameListItemsToDbStyle(localSubFolderList);
	}

	public List<string> RenameTempListItemsToDbStyle(IEnumerable<string> localSubFolderList)
	{
		return _paths.RenameTempListItemsToDbStyle(localSubFolderList);
	}

	/// <summary>
	///     A non-default tenant: own folders, no storage mappings (they point outside the root),
	///     read-only folders shared with the process-wide setting
	/// </summary>
	public static TenantSettings ForFolders(string tenantId, AppSettings processSettings,
		string storageFolder, string thumbnailTempFolder, string tempFolder)
	{
		var paths = new AppSettings
		{
			StorageFolder = storageFolder,
			ThumbnailTempFolder = thumbnailTempFolder,
			TempFolder = tempFolder,
			ReadOnlyFolders = [..processSettings.ReadOnlyFolders]
		};
		return new TenantSettings(tenantId, paths);
	}
}
