using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

public class FolderToFileList(ISelectorStorage selectorStorage)
{
	private readonly IStorage _iStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	public (bool success, List<string> toAddFilePaths) AddFiles(string subPath,
		Func<string?, bool> delegateToCheckIfExtensionIsSupported)
	{
		var toAddFilePaths = new List<string>();
		switch ( _iStorage.IsFolderOrFile(subPath) )
		{
			case FolderOrFileModel.FolderOrFileTypeList.Deleted:
				return ( false, [] );
			case FolderOrFileModel.FolderOrFileTypeList.Folder:
			{
				var contentOfDir = _iStorage.GetAllFilesInDirectoryRecursive(subPath)
					.Where(delegateToCheckIfExtensionIsSupported).ToList();
				toAddFilePaths.AddRange(contentOfDir);
				break;
			}
			case FolderOrFileModel.FolderOrFileTypeList.File:
			default:
			{
				toAddFilePaths.Add(subPath);
				break;
			}
		}

		return ( true, toAddFilePaths );
	}
}
