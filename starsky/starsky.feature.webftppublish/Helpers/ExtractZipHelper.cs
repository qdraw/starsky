using System;
using System.IO;
using starsky.feature.webftppublish.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;

namespace starsky.feature.webftppublish.Helpers;

/// <summary>
///     Helper to extract zip files or validate folders
/// </summary>
public class ExtractZipHelper(IStorage storage, IWebLogger logger)
{
	/// <summary>
	///     Extracts zip file or validates folder exists
	/// </summary>
	/// <param name="parentDirectoryOrZipFile">Path to zip or folder</param>
	/// <param name="tempFolderPrefix">Prefix for temp folder (e.g., "starsky-webftp")</param>
	/// <returns>Result model with extracted path</returns>
	public ExtractZipResultModel ExtractZip(string parentDirectoryOrZipFile,
		string tempFolderPrefix = "starsky-webftp")
	{
		var fileOrFolder = storage.IsFolderOrFile(parentDirectoryOrZipFile);
		switch ( fileOrFolder )
		{
			case FolderOrFileModel.FolderOrFileTypeList.Folder:
				return new ExtractZipResultModel
				{
					FullFileFolderPath = parentDirectoryOrZipFile,
					RemoveFolderAfterwards = false,
					IsError = false,
					IsFromZipFile = false
				};
			case FolderOrFileModel.FolderOrFileTypeList.Deleted:
				return new ExtractZipResultModel
				{
					FullFileFolderPath = parentDirectoryOrZipFile,
					IsError = true,
					IsFromZipFile = false
				};
		}

		var parentFolderTempPath = Path.Combine(Path.GetTempPath(), tempFolderPrefix,
			Path.GetFileNameWithoutExtension(parentDirectoryOrZipFile) + "_" +
			Guid.NewGuid().ToString("N"));
		storage.CreateDirectory(parentFolderTempPath);

		var zipper = new Zipper(new WebLogger());
		if ( zipper.ExtractZip(parentDirectoryOrZipFile, parentFolderTempPath) )
		{
			return new ExtractZipResultModel
			{
				FullFileFolderPath = parentFolderTempPath,
				RemoveFolderAfterwards = true,
				IsError = false,
				IsFromZipFile = true
			};
		}

		logger.LogError($"Zip extract failed {parentDirectoryOrZipFile}");
		return new ExtractZipResultModel
		{
			FullFileFolderPath = parentDirectoryOrZipFile, IsError = true, IsFromZipFile = true
		};
	}
}
