using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

public class ThumbnailService(
	IWebLogger logger,
	ISelectorStorage selectorStorage,
	AppSettings appSettings) : IThumbnailService
{
	private readonly FolderToFileList _folderToFileList = new(selectorStorage);

	private readonly IStorage
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	/// <summary>
	///     Can be used for directories or single files
	/// </summary>
	/// <param name="fileOrFolderPath">subPath of file or folder</param>
	/// <param name="skipExtraLarge">skip large format creation</param>
	/// <returns>results</returns>
	public async Task<List<GenerationResultModel>> GenerateThumbnail(string fileOrFolderPath,
		bool skipExtraLarge = false)
	{
		var (success, toAddFilePaths) = _folderToFileList.AddFiles(fileOrFolderPath,
			e =>
				ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(e) ||
				ExtensionRolesHelper.IsExtensionVideoSupported(e));

		var sizes = ThumbnailSizes.GetSizes(skipExtraLarge);
		if ( !success )
		{
			return ErrorGenerationResultModel
				.FailedResult(sizes, fileOrFolderPath, string.Empty, false, "File is deleted");
		}

		var resultChunkList = await toAddFilePaths.ForEachAsync(
			async singleSubPath => await CreateThumbAsync(singleSubPath, sizes),
			appSettings.MaxDegreesOfParallelismThumbnail);

		var results = new List<GenerationResultModel>();

		foreach ( var resultChunk in resultChunkList! )
		{
			results.AddRange(resultChunk);
		}

		return results;
	}

	/// <summary>
	///     Only for one file
	/// </summary>
	/// <param name="fileHash">hash</param>
	/// <param name="subPath">subPath of file; make sure it is NOT a folder</param>
	/// <param name="skipExtraLarge">skip large format creation</param>
	/// <returns>results</returns>
	/// <returns></returns>
	public async Task<List<GenerationResultModel>> GenerateThumbnail(string subPath,
		string fileHash,
		bool skipExtraLarge = false)
	{
		var (success, toAddFilePaths) = _folderToFileList.AddFiles(subPath,
			e =>
				ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(e) ||
				ExtensionRolesHelper.IsExtensionVideoSupported(e));

		var sizes = ThumbnailSizes.GetSizes(skipExtraLarge);
		if ( !success || toAddFilePaths.Count != 1 )
		{
			return ErrorGenerationResultModel
				.FailedResult(sizes, subPath, fileHash, false, "File is deleted");
		}

		return ( await CreateThumbAsync(subPath, sizes) ).ToList();
	}

	/// <summary>
	///     Rotate a thumbnail
	/// </summary>
	/// <param name="fileHash">fileHash to rename</param>
	/// <param name="orientation">which direction</param>
	/// <param name="width">height of output</param>
	/// <param name="height">0 = keep in shape</param>
	/// <returns></returns>
	public Task<bool> RotateThumbnail(string fileHash, int orientation,
		int width = 1000, int height = 0)
	{
		return new RotateThumbnailHelper(selectorStorage).RotateThumbnail(fileHash, orientation,
			width, height);
	}

	private Task<IEnumerable<GenerationResultModel>> CreateThumbAsync(string singleSubPath,
		List<ThumbnailSize> sizes)
	{
		if ( string.IsNullOrWhiteSpace(singleSubPath) )
		{
			throw new ArgumentNullException(nameof(singleSubPath));
		}

		return CreateThumbInternal(singleSubPath, sizes);
	}

	private async Task<IEnumerable<GenerationResultModel>> CreateThumbInternal(
		string singleSubPath, List<ThumbnailSize> sizes)
	{
		var generator =
			new ThumbnailGeneratorFactory(selectorStorage, logger).GetGenerator(singleSubPath);
		var (fileHash, success) = await new FileHash(_storage).GetHashCodeAsync(singleSubPath);
		if ( !success )
		{
			return [];
		}

		return await generator.GenerateThumbnail(singleSubPath, fileHash, sizes);
	}
}
