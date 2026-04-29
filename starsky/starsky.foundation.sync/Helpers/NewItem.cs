using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.foundation.sync.Helpers;

/// <summary>
///     Scope is only an object
/// </summary>
public sealed class NewItem
{
	private readonly IWebLogger _logger;
	private readonly IReadMeta _readMeta;
	private readonly IStorage _subPathStorage;

	public NewItem(IStorage subPathStorage, IReadMeta readMeta, IWebLogger logger)
	{
		_subPathStorage = subPathStorage;
		_readMeta = readMeta;
		_logger = logger;
	}

	public async Task<List<FileIndexItem>> NewFileItemAsync(List<FileIndexItem> inputItems)
	{
		var result = new List<FileIndexItem>();
		foreach ( var inputItem in inputItems )
		{
			result.Add(await NewFileItemAsync(inputItem));
		}

		return result;
	}

	/// <summary>
	///     Returns only an object (no db update)
	/// </summary>
	/// <param name="inputItem">at least FilePath and ParentDirectory, fileHash is optional</param>
	/// <returns></returns>
	public async Task<FileIndexItem> NewFileItemAsync(FileIndexItem inputItem)
	{
		return await NewFileItemAsync(inputItem.FilePath!, inputItem.FileHash!,
			inputItem.ParentDirectory!, inputItem.FileName!);
	}

	/// <summary>
	///     Prepare a new item (no update in db)
	/// </summary>
	/// <param name="filePath">path of file</param>
	/// <param name="fileHash">optional could be null</param>
	/// <param name="parentDirectory">parent directory name</param>
	/// <param name="fileName">name without path</param>
	/// <returns></returns>
	private async Task<FileIndexItem> NewFileItemAsync(string filePath, string fileHash,
		string parentDirectory, string fileName)
	{
		var updatedDatabaseItem = await _readMeta.ReadExifAndXmpFromFileAsync(filePath);
		var stream = _subPathStorage.ReadStream(filePath, ExtensionRolesHelper.ImageFormatByteSize);
		updatedDatabaseItem!.ImageFormat = new ExtensionRolesHelper(_logger).GetImageFormat(stream);
		await stream.DisposeAsync();

		// Read JSON sidecar (.starsky.filename.ext.json) and merge with EXIF/XMP data
		await ReadAndApplyJsonSidecarAsync(filePath, updatedDatabaseItem);

		await SetFileHashStatus(filePath, fileHash, updatedDatabaseItem);
		updatedDatabaseItem.SetAddToDatabase();
		var info = _subPathStorage.Info(filePath);

		updatedDatabaseItem.LastEdited = info.LastWriteTime;
		updatedDatabaseItem.IsDirectory = false;
		updatedDatabaseItem.Size = info.Size;
		updatedDatabaseItem.ParentDirectory = parentDirectory;
		updatedDatabaseItem.FileName = fileName;

		return updatedDatabaseItem;
	}

	/// <summary>
	///     Only update an item with updated content form disk
	/// </summary>
	/// <param name="dbItem">database item</param>
	/// <param name="size">byte size</param>
	/// <returns>the updated item</returns>
	public async Task<FileIndexItem> PrepareUpdateFileItemAsync(FileIndexItem dbItem, long size)
	{
		var metaDataItem = await _readMeta.ReadExifAndXmpFromFileAsync(dbItem.FilePath!);

		// Read JSON sidecar (.starsky.filename.ext.json) and merge with EXIF/XMP data
		await ReadAndApplyJsonSidecarAsync(dbItem.FilePath!, metaDataItem!);

		var compare = FileIndexCompareHelper.Compare(dbItem, metaDataItem);
		dbItem.Size = size;
		await SetFileHashStatus(dbItem.FilePath!, dbItem.FileHash!, dbItem);
		dbItem.LastChanged.AddRange(compare);
		if ( compare.Count == 0 )
		{
			dbItem.Status = FileIndexItem.ExifStatus.OkAndSame;
		}

		return dbItem;
	}

	/// <summary>
	///     Read the JSON sidecar file (.starsky.filename.ext.json) if present
	///     and merge its data onto the given FileIndexItem using FileIndexCompareHelper.
	///     JSON sidecar values take precedence over EXIF/XMP values.
	/// </summary>
	/// <param name="filePath">subPath of the file (e.g. /path/to/image.jpg)</param>
	/// <param name="fileIndexItem">item to enrich with JSON sidecar data</param>
	private async Task ReadAndApplyJsonSidecarAsync(string filePath, FileIndexItem fileIndexItem)
	{
		if ( string.IsNullOrEmpty(filePath) )
		{
			return;
		}

		// Guard: JsonSidecarLocation requires a non-empty filename component
		var fileName = PathHelper.GetFileName(filePath);
		if ( string.IsNullOrEmpty(fileName) )
		{
			return;
		}

		var jsonSubPath = JsonSidecarLocation.JsonLocation(filePath);

		if ( !_subPathStorage.ExistFile(jsonSubPath) )
		{
			return;
		}

		MetadataContainer? container;
		try
		{
			container = await new DeserializeJson(_subPathStorage)
				.ReadAsync<MetadataContainer>(jsonSubPath);
		}
		catch ( Exception ex )
		{
			_logger.LogError($"[NewItem] Failed to read JSON sidecar {jsonSubPath}: {ex.Message}",
				ex);
			return;
		}

		if ( container?.Item == null )
		{
			return;
		}

		FileIndexCompareHelper.Compare(fileIndexItem, container.Item);
	}

	/// <summary>
	///     Set file hash when not exist
	/// </summary>
	/// <param name="filePath">filePath</param>
	/// <param name="fileHash"></param>
	/// <param name="updatedDatabaseItem">new created object</param>
	/// <returns></returns>
	private async Task SetFileHashStatus(string filePath, string fileHash,
		FileIndexItem updatedDatabaseItem)
	{
		updatedDatabaseItem.Status = FileIndexItem.ExifStatus.Ok;
		if ( string.IsNullOrEmpty(fileHash) )
		{
			var fileHashService = new FileHash(_subPathStorage, _logger);
			var (localHash, success) =
				await fileHashService.GetHashCodeAsync(filePath,
					updatedDatabaseItem.ImageFormat);
			updatedDatabaseItem.FileHash = localHash;
			updatedDatabaseItem.Status = success
				? FileIndexItem.ExifStatus.Ok
				: FileIndexItem.ExifStatus.OperationNotSupported;
		}
	}
}
