using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.foundation.thumbnailgeneration.Helpers;

public static class ToAddFiles
{
	public static (bool, List<string>) AddFiles(IStorage storage, string subPath,
		Func<string?, bool> isExtensionExifToolSupported)
	{
		var toAddFilePaths = new List<string>();
		switch ( storage.IsFolderOrFile(subPath) )
		{
			case FolderOrFileModel.FolderOrFileTypeList.Deleted:
				return ( false, [] );
			case FolderOrFileModel.FolderOrFileTypeList.Folder:
			{
				var contentOfDir = storage.GetAllFilesInDirectoryRecursive(subPath)
					.Where(isExtensionExifToolSupported).ToList();
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
