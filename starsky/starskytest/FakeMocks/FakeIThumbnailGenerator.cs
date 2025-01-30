using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.FakeMocks;

public class FakeThumbnailGenerator : IThumbnailGenerator
{
	private Exception? _exception;
	private IEnumerable<GenerationResultModel> _results = new List<GenerationResultModel>();

	public Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash, ThumbnailImageFormat imageFormat, List<ThumbnailSize> thumbnailSizes)
	{
		if ( _exception != null )
		{
			throw _exception;
		}

		return Task.FromResult(_results);
	}

	public void SetResults(IEnumerable<GenerationResultModel> results)
	{
		_results = results;
	}

	public void SetException(Exception exception)
	{
		_exception = exception;
	}
}
