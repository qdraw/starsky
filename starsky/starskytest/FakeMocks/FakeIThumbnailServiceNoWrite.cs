using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.FakeMocks;

/// <summary>
///     /Fake thumbnail service that returns success generation results but doesn't write files
/// </summary>
public sealed class FakeIThumbnailServiceNoWrite : IThumbnailService
{
	public Task<List<GenerationResultModel>> GenerateThumbnail(string fileOrFolderPath,
		ThumbnailGenerationType type = ThumbnailGenerationType.All)
	{
		return Task.FromResult(new List<GenerationResultModel>());
	}

	public Task<List<GenerationResultModel>> GenerateThumbnail(string subPath, string fileHash,
		ThumbnailGenerationType type = ThumbnailGenerationType.All)
	{
		return Task.FromResult(new List<GenerationResultModel>());
	}

	public Task<(Stream?, GenerationResultModel)> GenerateThumbnail(string subPath,
		string fileHash, ThumbnailImageFormat imageFormat,
		ThumbnailSize size)
	{
		return Task.FromResult(new ValueTuple<Stream?, GenerationResultModel>(null,
			new GenerationResultModel
			{
				FileHash = fileHash, Size = size, ImageFormat = imageFormat, Success = true
			}));
	}

	public Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000,
		int height = 0)
	{
		return Task.FromResult(false);
	}
}
