using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
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
	private readonly IWebLogger _logger;
	private readonly IVideoProcess _videoProcess;


	public ThumbnailVideo(IStorage iStorage, IWebLogger logger, IVideoProcess videoProcess)
	{
		_iStorage = iStorage;
		_logger = logger;
		_videoProcess = videoProcess;
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

	private async Task<IEnumerable<GenerationResultModel>> CreateThumbInternal(string subPath,
		string fileHash, bool skipExtraLarge = false)
	{
		var extensionSupported = ExtensionRolesHelper.IsExtensionVideoSupported(subPath);
		var existsFile = _iStorage.ExistFile(subPath);

		if ( !extensionSupported || !existsFile )
		{
			return ThumbnailNameHelper.GeneratedThumbnailSizes.Select(size =>
				new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = !existsFile,
					ErrorMessage = !extensionSupported ? "not supported" : "File is not found",
					Size = size
				}).ToList();
		}

		// File is already tested
		if ( _iStorage.ExistFile(ErrorLogItemFullPath.GetErrorLogItemFullPath(subPath)) )
		{
			return ThumbnailNameHelper.GeneratedThumbnailSizes.Select(size =>
				new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = false,
					ErrorMessage = "File already failed before",
					Size = size
				}).ToList();
		}

		var result = await _videoProcess.Run(subPath, null, VideoProcessTypes.Thumbnail);

		if ( !result )
		{
			return ThumbnailNameHelper.GeneratedThumbnailSizes.Select(size =>
				new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = false,
					ErrorMessage = "Failed to create thumbnail",
					Size = size
				}).ToList();
		}

		return [];
	}
}
