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
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Models;
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
	IVideoProcess videoProcess,
	IFileHashSubPathStorage fileHashSubPathStorage,
	INativePreviewThumbnailGenerator nativePreviewThumbnailGenerator)
	: IThumbnailService
{
	private readonly Func<string?, bool> _delegateToCheckIfExtensionIsSupported = e =>
		ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(e) ||
		ExtensionRolesHelper.IsExtensionVideoSupported(e);

	private readonly FolderToFileList _folderToFileList = new(selectorStorage);

	/// <summary>
	///     Can be used for directories or single files
	/// </summary>
	/// <param name="fileOrFolderPath">subPath of file or folder</param>
	/// <param name="type">skip large format creation</param>
	/// <returns>results</returns>
	public async Task<List<GenerationResultModel>> GenerateThumbnail(string fileOrFolderPath,
		ThumbnailGenerationType type = ThumbnailGenerationType.All)
	{
		var (success, toAddFilePaths) = _folderToFileList.AddFiles(fileOrFolderPath,
			_delegateToCheckIfExtensionIsSupported);

		var sizes = ThumbnailSizes.GetLargeToSmallSizes(type);
		if ( !success )
		{
			return ErrorGenerationResultModel
				.FailedResult(sizes, fileOrFolderPath, string.Empty,
					false, ThumbnailImageFormat.unknown, true, "File is deleted");
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
	/// <param name="type">skip large format creation</param>
	/// <returns>results</returns>
	/// <returns></returns>
	public async Task<List<GenerationResultModel>> GenerateThumbnail(string subPath,
		string fileHash,
		ThumbnailGenerationType type = ThumbnailGenerationType.All)
	{
		var (success, toAddFilePaths) =
			_folderToFileList.AddFiles(subPath, _delegateToCheckIfExtensionIsSupported);

		var sizes = ThumbnailSizes.GetLargeToSmallSizes(type);

		if ( !success || toAddFilePaths.Count != 1 )
		{
			return ErrorGenerationResultModel
				.FailedResult(sizes, subPath, fileHash, false,
					ThumbnailImageFormat.unknown, true, "File is deleted");
		}

		var generationResults = ( await GenerateThumbnailAsync(subPath, fileHash, sizes) ).ToList();
		await updateStatusGeneratedThumbnailService.AddOrUpdateStatusAsync(generationResults);

		// Socket updates are not the scope of this service
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
				.FailedResult(size, subPath, fileHash, false, imageFormat, true,
					"File is deleted") );
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
		var factory = new ThumbnailGeneratorFactory(selectorStorage, logger, videoProcess,
			nativePreviewThumbnailGenerator);
		var generator = factory.GetGenerator(singleSubPath);
		if ( !string.IsNullOrEmpty(fileHash) )
		{
			return await generator.GenerateThumbnail(singleSubPath, fileHash,
				appSettings.ThumbnailImageFormat, sizes);
		}

		var (fileHashLocal, success) =
			await fileHashSubPathStorage.GetHashCodeAsync(singleSubPath, null);
		if ( !success )
		{
			return [];
		}

		return await generator.GenerateThumbnail(singleSubPath, fileHashLocal,
			appSettings.ThumbnailImageFormat, sizes);
	}

	private async Task<(Stream?, GenerationResultModel)> GenerateSingleThumbnailAsync(
		string singleSubPath, ThumbnailImageFormat thumbnailImageFormat, ThumbnailSize size)
	{
		var factory = new ThumbnailGeneratorFactory(selectorStorage, logger, videoProcess,
			nativePreviewThumbnailGenerator);
		var generator = factory.GetGenerator(singleSubPath);

		var (fileHash, success) =
			await fileHashSubPathStorage.GetHashCodeAsync(singleSubPath, null);
		if ( !success )
		{
			return ( null, ErrorGenerationResultModel
				.FailedResult(size, singleSubPath, fileHash,
					true, thumbnailImageFormat, true, "Invalid fileHash") );
		}

		var generationResult =
			( await generator.GenerateThumbnail(singleSubPath, fileHash,
				thumbnailImageFormat, [size]) ).First();

		var stream =
			new GetThumbnailStream(selectorStorage).GetThumbnail(fileHash, size,
				thumbnailImageFormat);
		return ( stream, generationResult );
	}
}
