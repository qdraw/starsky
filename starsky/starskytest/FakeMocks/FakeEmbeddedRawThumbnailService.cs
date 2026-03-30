using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Interfaces;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks;

public class FakeEmbeddedRawThumbnailService : IEmbeddedRawThumbnailService
{
	private readonly Exception? _exception;
	private readonly List<string> _outputLargePaths = [];
	private readonly ISelectorStorage _selectorStorage;
	private readonly bool _tryExtractStatus;

	public FakeEmbeddedRawThumbnailService(ISelectorStorage selectorStorage,
		List<string>? outputLargePaths = null, bool tryExtractStatus = false,
		Exception? exception = null)
	{
		_selectorStorage = selectorStorage;
		if ( outputLargePaths != null )
		{
			_outputLargePaths = outputLargePaths;
		}

		if ( exception != null )
		{
			_exception = exception;
		}

		_tryExtractStatus = tryExtractStatus;
	}

	public Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath)
	{
		if ( _exception != null )
		{
			throw _exception;
		}

		if ( string.IsNullOrEmpty(outputLargePath) )
		{
			return Task.FromResult(false);
		}

		if ( _outputLargePaths.Contains(outputLargePath) )
		{
			var tempStorage = _selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
			// write a small valid jpeg into temporary storage path
			using var ms = new MemoryStream([.. CreateAnImage.Bytes]);
			tempStorage.WriteStream(ms, outputLargePath);
			return Task.FromResult(true);
		}

		// default false
		return Task.FromResult(_tryExtractStatus);
	}
}
