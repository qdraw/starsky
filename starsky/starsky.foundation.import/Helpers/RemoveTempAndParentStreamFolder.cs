using System.Collections.Generic;
using System.IO;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.import.Helpers;

public class RemoveTempAndParentStreamFolderHelper
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystemStorage;

	public RemoveTempAndParentStreamFolderHelper(IStorage hostFileSystemStorage,
		AppSettings appSettings)
	{
		_hostFileSystemStorage = hostFileSystemStorage;
		_appSettings = appSettings;
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
		if ( !string.IsNullOrEmpty(parentPath) && parentPath != _appSettings.TempFolder )
		{
			_hostFileSystemStorage.FolderDelete(parentPath);
		}
	}
}
