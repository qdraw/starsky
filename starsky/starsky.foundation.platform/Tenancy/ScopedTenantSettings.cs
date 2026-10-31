using System;
using System.Collections.Generic;
using starsky.foundation.injection;

namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     The settings of the tenant of the current request or job, for scoped services. The tenant
///     is read on every call: it is set by the middleware after the scope was created
/// </summary>
[Service(typeof(ITenantSettings), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ScopedTenantSettings(
	ITenantContext tenantContext,
	ITenantSettingsProvider provider) : ITenantSettings
{
	private ITenantSettings Current => provider.GetSettings(tenantContext.TenantId);

	public string TenantId => tenantContext.TenantId;
	public string StorageFolder => Current.StorageFolder;
	public string ThumbnailTempFolder => Current.ThumbnailTempFolder;
	public string TempFolder => Current.TempFolder;
	public IReadOnlyDictionary<string, string> StorageFolderMappings =>
		Current.StorageFolderMappings;

	public bool IsReadOnly(string path)
	{
		return Current.IsReadOnly(path);
	}

	public string DatabasePathToFilePath(string databaseFilePath)
	{
		return Current.DatabasePathToFilePath(databaseFilePath);
	}

	public string DatabasePathToTempFolderFilePath(string databaseFilePath)
	{
		return Current.DatabasePathToTempFolderFilePath(databaseFilePath);
	}

	public string FullPathStorageFolderToDatabaseStyle(string fullPath)
	{
		return Current.FullPathStorageFolderToDatabaseStyle(fullPath);
	}

	public List<string> RenameListItemsToDbStyle(IEnumerable<string> localSubFolderList)
	{
		return Current.RenameListItemsToDbStyle(localSubFolderList);
	}

	public IEnumerable<KeyValuePair<string, DateTime>> RenameListItemsToDbStyle(
		IEnumerable<KeyValuePair<string, DateTime>> localSubFolderList)
	{
		return Current.RenameListItemsToDbStyle(localSubFolderList);
	}

	public List<string> RenameTempListItemsToDbStyle(IEnumerable<string> localSubFolderList)
	{
		return Current.RenameTempListItemsToDbStyle(localSubFolderList);
	}
}
