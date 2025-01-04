using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.video.Process;
using starsky.foundation.video.Process.Interfaces;

namespace starsky.foundation.thumbnailgeneration.Helpers;

public class ThumbnailVideo
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly ReadMeta _readMeta;
	private readonly IVideoProcess _videoProcess;

	public ThumbnailVideo(IStorage iStorage, IWebLogger logger, IVideoProcess videoProcess,
		AppSettings appSettings, IMemoryCache memoryCache)
	{
		_iStorage = iStorage;
		_videoProcess = videoProcess;
		_readMeta = new ReadMeta(iStorage,
			appSettings, memoryCache, logger);
	}

	internal async Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath)
	{
		var (success, toAddFilePaths) = ToAddFiles.AddFiles(_iStorage, subPath,
			ExtensionRolesHelper.IsExtensionExifToolSupported);
		if ( !success )
		{
			return
			[
				new GenerationResultModel
				{
					SubPath = subPath,
					Success = false,
					IsNotFound = true,
					ErrorMessage = "File is deleted"
				}
			];
		}

		var resultChunkList = await toAddFilePaths.ForEachAsync(
			async singleSubPath =>
			{
				var hashResult = await new FileHash(_iStorage).GetHashCodeAsync(singleSubPath);
				if ( !hashResult.Value )
				{
					return null;
				}

				return await CreateThumbAsync(singleSubPath, hashResult.Key);
			}, _appSettings.MaxDegreesOfParallelismThumbnail);

		var results = new List<GenerationResultModel>();

		foreach ( var resultChunk in resultChunkList! )
		{
			results.AddRange(resultChunk ?? new List<GenerationResultModel>());
		}

		return results;
	}

	internal Task<IEnumerable<GenerationResultModel>> CreateThumbAsync(string? subPath,
		string fileHash, bool skipExtraLarge = false)
	{
		if ( string.IsNullOrWhiteSpace(fileHash) )
		{
			throw new ArgumentNullException(nameof(fileHash));
		}

		if ( string.IsNullOrWhiteSpace(subPath) )
		{
			throw new ArgumentNullException(nameof(fileHash));
		}

		return CreateThumbInternal(subPath, fileHash, skipExtraLarge);
	}

	private IEnumerable<GenerationResultModel> FailedResult(string subPath, string fileHash,
		bool existsFile,
		string errorMessage)
	{
		return ThumbnailNameHelper.GeneratedThumbnailSizes.Select(size =>
			new GenerationResultModel
			{
				SubPath = subPath,
				FileHash = fileHash,
				Success = false,
				IsNotFound = !existsFile,
				ErrorMessage = errorMessage,
				Size = size
			}).ToList();
	}

	private async Task<IEnumerable<GenerationResultModel>> CreateThumbInternal(string subPath,
		string fileHash, bool skipExtraLarge = false)
	{
		var extensionSupported = ExtensionRolesHelper.IsExtensionVideoSupported(subPath);
		var existsFile = _iStorage.ExistFile(subPath);

		if ( !extensionSupported || !existsFile )
		{
			return FailedResult(subPath, fileHash, existsFile,
				!extensionSupported ? "not supported" : "File is not found");
		}

		// File is already tested
		if ( _iStorage.ExistFile(ErrorLogItemFullPath.GetErrorLogItemFullPath(subPath)) )
		{
			return FailedResult(subPath, fileHash, true,
				"File already failed before");
		}

		var videoResult = await _videoProcess.RunVideo(subPath,
			fileHash, VideoProcessTypes.Thumbnail);


		if ( !videoResult.IsSuccess || string.IsNullOrWhiteSpace(videoResult.ResultPath) )
		{
			return FailedResult(subPath, fileHash, true,
				"Failed to create thumbnail");
		}

		var meta = await _readMeta.ReadExifAndXmpFromFileAsync(videoResult.ResultPath);
		if ( meta == null )
		{
			return FailedResult(subPath, fileHash, true,
				"Failed to read meta");
		}

		// create based on the sizes a thumbnail image, skip if the source image is smaller
		var results = ThumbnailNameHelper.GeneratedThumbnailSizes
			.Where(p => !skipExtraLarge || p != ThumbnailSize.ExtraLarge)
			.Select(size =>
			{
				var sizeInPixels = ThumbnailNameHelper.GetSize(size);
				if ( meta.ImageWidth < sizeInPixels || meta.ImageHeight < sizeInPixels )
				{
					return new GenerationResultModel
					{
						SubPath = subPath,
						FileHash = fileHash,
						Success = false,
						IsNotFound = false,
						ErrorMessage = "Source image is smaller",
						Size = size
					};
				}

				return new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = true,
					IsNotFound = false,
					Size = size
				};
			}).ToList();


		return [];
	}
}
