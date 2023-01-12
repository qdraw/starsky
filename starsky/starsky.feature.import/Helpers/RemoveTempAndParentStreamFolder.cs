using System.IO;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.import.Helpers;

public class RemoveTempAndParentStreamFolderHelper
{
	private readonly IStorage _hostFileSystemStorage;
	private readonly AppSettings _appSettings;

	public RemoveTempAndParentStreamFolderHelper(IStorage hostFileSystemStorage, AppSettings appSettings)
	{
		_hostFileSystemStorage = hostFileSystemStorage;
		_appSettings = appSettings;
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
