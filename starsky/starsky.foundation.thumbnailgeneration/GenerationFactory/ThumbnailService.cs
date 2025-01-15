using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.video.Process.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

[Service(typeof(IThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class ThumbnailService(
	ISelectorStorage selectorStorage,
	IWebLogger logger,
	AppSettings appSettings,
	IUpdateStatusGeneratedThumbnailService updateStatusGeneratedThumbnailService,
	IVideoProcess videoProcess)
	: IThumbnailService
{
	private readonly Func<string?, bool> _delegateToCheckIfExtensionIsSupported = e =>
		ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(e) ||
		ExtensionRolesHelper.IsExtensionVideoSupported(e);

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
			_delegateToCheckIfExtensionIsSupported);

		var sizes = ThumbnailSizes.GetLargeToSmallSizes(skipExtraLarge);
		if ( !success )
		{
			return ErrorGenerationResultModel
				.FailedResult(sizes, fileOrFolderPath, string.Empty,
					false, "File is deleted");
		}

		var resultChunkList = await toAddFilePaths.ForEachAsync(
			async singleSubPath => await GenerateThumbnailAsync(singleSubPath, null, sizes),
			appSettings.MaxDegreesOfParallelismThumbnail);

		var generationResults = new List<GenerationResultModel>();
		foreach ( var resultChunk in resultChunkList! )
		{
			generationResults.AddRange(resultChunk);
		}

		await updateStatusGeneratedThumbnailService.AddOrUpdateStatusAsync(generationResults);

		return generationResults;
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
		var (success, toAddFilePaths) =
			_folderToFileList.AddFiles(subPath, _delegateToCheckIfExtensionIsSupported);

		var sizes = ThumbnailSizes.GetLargeToSmallSizes(skipExtraLarge);
		if ( !success || toAddFilePaths.Count != 1 )
		{
			return ErrorGenerationResultModel
				.FailedResult(sizes, subPath, fileHash, false, "File is deleted");
		}

		var generationResults = ( await GenerateThumbnailAsync(subPath, fileHash, sizes) ).ToList();
		await updateStatusGeneratedThumbnailService.AddOrUpdateStatusAsync(generationResults);

		return generationResults;
	}

	public async Task<(Stream?, GenerationResultModel)> GenerateThumbnail(string subPath,
		string fileHash, ThumbnailImageFormat imageFormat, ThumbnailSize size)
	{
		var (success, toAddFilePaths) =
			_folderToFileList.AddFiles(subPath, _delegateToCheckIfExtensionIsSupported);

		if ( !success || toAddFilePaths.Count != 1 )
		{
			return ( null, ErrorGenerationResultModel
				.FailedResult(size, subPath, fileHash, false, "File is deleted") );
		}

		var (stream, status) = await GenerateSingleThumbnailAsync(subPath, imageFormat, size);
		await updateStatusGeneratedThumbnailService.AddOrUpdateStatusAsync([status]);

		return ( stream, status );
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
		var service = new RotateThumbnailHelper(selectorStorage, appSettings, logger);
		return service.RotateThumbnail(fileHash, orientation,
			width, height);
	}

	private async Task<IEnumerable<GenerationResultModel>> GenerateThumbnailAsync(
		string singleSubPath, string? fileHash, List<ThumbnailSize> sizes)
	{
		var factory = new ThumbnailGeneratorFactory(selectorStorage, logger, videoProcess);
		var generator = factory.GetGenerator(singleSubPath);
		if ( !string.IsNullOrEmpty(fileHash) )
		{
			return await generator.GenerateThumbnail(singleSubPath, fileHash,
				appSettings.ThumbnailImageFormat, sizes);
		}

		var (fileHashLocal, success) =
			await new FileHash(_storage, logger).GetHashCodeAsync(singleSubPath);
		if ( !success )
		{
			return [];
		}

		return await generator.GenerateThumbnail(singleSubPath, fileHashLocal,
			appSettings.ThumbnailImageFormat, sizes);
	}

	private async Task<(Stream?, GenerationResultModel)> GenerateSingleThumbnailAsync(
		string singleSubPath, ThumbnailImageFormat imageFormat, ThumbnailSize size)
	{
		var factory = new ThumbnailGeneratorFactory(selectorStorage, logger, videoProcess);
		var generator = factory.GetGenerator(singleSubPath);

		var (fileHash, success) =
			await new FileHash(_storage, logger).GetHashCodeAsync(singleSubPath);
		if ( !success )
		{
			return ( null, ErrorGenerationResultModel
				.FailedResult(size, singleSubPath, fileHash,
					true, "Invalid fileHash") );
		}

		var generationResult =
			( await generator.GenerateThumbnail(singleSubPath, fileHash,
				imageFormat, [size]) ).First();

		var stream =
			new GetThumbnailStream(selectorStorage).GetThumbnail(fileHash, size, imageFormat);
		return ( stream, generationResult );
	}
}
