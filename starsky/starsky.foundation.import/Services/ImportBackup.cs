using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.storage.Structure;

namespace starsky.foundation.import.Services;

public class ImportBackup(ISelectorStorage selectorStorage, IWebLogger logger)
{
	private readonly IStorage _filesystemStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	public async Task<bool?> CopyStreamFromHostToBackup(
		ImportIndexItem importIndexItem,
		AppSettingsImportBackupModel importBackup)
	{
		if ( !importBackup.Enabled || 
		     !_filesystemStorage.ExistFolder(importBackup.StorageFolder ?? ""))
		{
			return null;
		}

		var backupFileName = GetFileName(importIndexItem);
		var backupFilePath = Path.Combine(importBackup.StorageFolder!, backupFileName);
		var sourceFileSize = _filesystemStorage.Info(importIndexItem.SourceFullFilePath).Size;

		try
		{
			var sourceStream = _filesystemStorage.ReadStream(importIndexItem.SourceFullFilePath);
			await _filesystemStorage.WriteStreamAsync(sourceStream, backupFilePath);
			// SourceStream is disposed in WriteStreamAsync
		}
		catch ( AggregateException exception )
		{
			//  For example: System.IO.IOException: No space left on device 
			logger.LogError(
				$"[ImportBackup] CopyStream  {backupFilePath} - retry helper failed {exception.Message}",
				exception);
		}

		var backupFileSize = _filesystemStorage.Info(backupFilePath).Size;
		if ( sourceFileSize == backupFileSize )
		{
			return true;
		}

		logger.LogError(
			$"[ImportBackup] Filesize does not match H:{sourceFileSize} - B:{backupFileSize} " +
			$"H: {importIndexItem.SourceFullFilePath} - S: {importIndexItem.FilePath}");

		return false;
	}

	private string GetFileName(ImportIndexItem importIndexItem)
	{
		var inputModel = new StructureInputModel(
			importIndexItem.FileIndexItem!.DateTime,
			Path.GetFileNameWithoutExtension(importIndexItem.SourceFullFilePath),
			FilenamesHelper.GetFileExtensionWithoutDot(importIndexItem.FileIndexItem
				.FileName!),
			importIndexItem.FileIndexItem.ImageFormat,
			string.Empty);

		var structureService =
			new StructureService(selectorStorage,
				new AppSettingsStructureModel
				{
					DefaultPattern = "/yyyyMMdd_HHmmss_{filenamebase}.ext"
				}, logger);


		return structureService.ParseFileName(inputModel);
	}
}
