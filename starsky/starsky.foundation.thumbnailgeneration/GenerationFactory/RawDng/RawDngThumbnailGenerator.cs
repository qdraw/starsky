using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

public class RawDngThumbnailGenerator(
	ISelectorStorage selectorStorage,
	IWebLogger logger)
	: IThumbnailGenerator
{
	private const string TemporaryPreviewPrefix = "/rawdng_large_";

	private IStorage SubPathStorage => selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private static bool IsDngFile(string? path)
	{
		return path?.EndsWith(".dng", StringComparison.OrdinalIgnoreCase) == true;
	}

	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash,
		ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		if ( !IsDngFile(singleSubPath) )
		{
			return [];
		}

		return await new SharedGenerate(selectorStorage, logger).GenerateThumbnail(
			ResizeThumbnailFromRawDng,
			IsDngFile,
			singleSubPath,
			fileHash,
			imageFormat,
			thumbnailSizes);
	}

	private async Task<GenerationResultModel> ResizeThumbnailFromRawDng(
		ThumbnailSize biggestThumbnailSize,
		string singleSubPath,
		string fileHash,
		ThumbnailImageFormat imageFormat)
	{
		var previewPath = $"{TemporaryPreviewPrefix}{fileHash}.jpg";
		var tempStorage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);

		try
		{
			using var source = SubPathStorage.ReadStream(singleSubPath);
			if ( source == Stream.Null )
			{
				return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
					singleSubPath, fileHash, true, imageFormat, true,
					"Failed to open DNG source file");
			}

			using var output = new MemoryStream();
			if ( !RawDngPipelineRunner.TryRunToJpeg(source, output, out var error) )
			{
				return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
					singleSubPath, fileHash, true, imageFormat, true,
					string.IsNullOrWhiteSpace(error)
						? "Failed to decode DNG RAW image"
						: error);
			}

			output.Seek(0, SeekOrigin.Begin);
			if ( !tempStorage.WriteStream(output, previewPath) || !tempStorage.ExistFile(previewPath) )
			{
				return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
					singleSubPath, fileHash, true, imageFormat, true,
					"Failed to write DNG preview output");
			}

			try
			{
				var resizeHelper = new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
				return await resizeHelper.ResizeThumbnailFromSourceImage(previewPath,
					SelectorStorage.StorageServices.Temporary,
					ThumbnailNameHelper.GetSize(biggestThumbnailSize), fileHash,
					false, imageFormat);
			}
			finally
			{
				if ( tempStorage.ExistFile(previewPath) )
				{
					tempStorage.FileDelete(previewPath);
				}
			}
		}
		catch ( Exception exception )
		{
			logger.LogError(
				$"[RawDngThumbnailGenerator] Failed to decode DNG {singleSubPath}: {exception.Message}");
			return ErrorGenerationResultModel.FailedResult(biggestThumbnailSize,
				singleSubPath, fileHash, true, imageFormat, true, exception.Message);
		}
	}
}
