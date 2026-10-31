using System.Collections.Generic;
using System.IO;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.platform.Tenancy;

namespace starsky.foundation.import.Helpers;

public class RemoveTempAndParentStreamFolderHelper
{
	private readonly IStorage _hostFileSystemStorage;
	private readonly string _tempFolder;

	/// <param name="tenantSettings">
	///     the temp folder of the tenant: the staging folder of another tenant is not touched
	/// </param>
	public RemoveTempAndParentStreamFolderHelper(IStorage hostFileSystemStorage,
		AppSettings appSettings, ITenantSettings? tenantSettings = null)
	{
		_hostFileSystemStorage = hostFileSystemStorage;
		_tempFolder = (tenantSettings ?? new TenantSettings(TenantIdHelper.DefaultTenantId, appSettings)).TempFolder;
	}

	/// <summary>
	///     Is this folder inside the temp folder (not the temp folder itself)? The folders are
	///     compared without trailing separators: the temp folder ends with one, a parent path does not
	/// </summary>
	public static bool IsSubFolderOf(string path, string root)
	{
		var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
		var folder = path.TrimEnd(separators);
		var tempRoot = root.TrimEnd(separators);
		return folder.Length > tempRoot.Length &&
		       folder.StartsWith(tempRoot + Path.DirectorySeparatorChar,
			       System.StringComparison.OrdinalIgnoreCase);
	}

	public void RemoveTempAndParentStreamFolder(List<string> tempImportFullPaths)
	{
		foreach ( var tempImportSinglePath in tempImportFullPaths )
		{
			RemoveTempAndParentStreamFolder(tempImportSinglePath);
		}
	}

	public void RemoveTempAndParentStreamFolder(string tempImportFullPath)
	{
		_hostFileSystemStorage.FileDelete(tempImportFullPath);
		// remove parent folder of tempFile
		var parentPath = Directory.GetParent(tempImportFullPath)?.FullName;
		// only a folder inside the temp folder: never the temp folder itself, or anything else
		if ( !string.IsNullOrEmpty(parentPath) && IsSubFolderOf(parentPath, _tempFolder) )
		{
			_hostFileSystemStorage.FolderDelete(parentPath);
		}
	}
}
