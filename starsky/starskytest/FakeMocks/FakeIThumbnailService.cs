using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.FakeMocks;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class FakeIThumbnailService : IThumbnailService
{
	private readonly Exception? _exception;
	private readonly IStorage? _subPathStorage;

	public FakeIThumbnailService(FakeSelectorStorage? selectorStorage = null,
		Exception? exception = null)
	{
		_subPathStorage = selectorStorage?.Get(SelectorStorage.StorageServices.SubPath);
		_exception = exception;
	}

	public List<Tuple<string?, string?>> Inputs { get; set; } = new();

	public List<Tuple<string, int?, int?, int?>> InputsRotate { get; set; } = new();

	public async Task<List<GenerationResultModel>> GenerateThumbnail(string fileOrFolderPath,
		ThumbnailGenerationType type = ThumbnailGenerationType.All)
	{
		return await CreateThumbnailAsync(fileOrFolderPath);
	}

	public async Task<List<GenerationResultModel>> GenerateThumbnail(string subPath,
		string fileHash, ThumbnailGenerationType type = ThumbnailGenerationType.All)
	{
		return ( await CreateThumbAsync(subPath, fileHash) ).ToList();
	}

	public Task<(Stream?, GenerationResultModel)> GenerateThumbnail(string subPath, string fileHash,
		ThumbnailImageFormat imageFormat,
		ThumbnailSize size)
	{
		throw new NotImplementedException();
	}

	public Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000,
		int height = 0)
	{
		InputsRotate.Add(new Tuple<string, int?, int?, int?>(fileHash, orientation, width, height));
		return Task.FromResult(true);
	}

	private Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath)
	{
		if ( _exception != null )
		{
			throw _exception;
		}

		if ( _subPathStorage == null )
		{
			throw new NullReferenceException("_subPathStorage not be null");
		}

		_subPathStorage.WriteStream(
			StringToStreamHelper.StringToStream("test"), subPath);
		Inputs.Add(new Tuple<string?, string?>(subPath, null));

		var items = _subPathStorage.GetAllFilesInDirectory(subPath);
		if ( items == null )
		{
			return Task.FromResult(
				new List<GenerationResultModel>
				{
					new() { SubPath = subPath, Success = true, FileHash = "test" }
				});
		}

		var name = Base32.Encode(Encoding.UTF8.GetBytes(subPath));
		_subPathStorage.WriteStream(
			StringToStreamHelper.StringToStream("test"), "/" + name + "@2000.jpg");

		var resultModel = new List<GenerationResultModel>();
		foreach ( var item in items )
		{
			resultModel.Add(new GenerationResultModel
			{
				SubPath = item, Success = true, FileHash = "test"
			});
		}

		return Task.FromResult(resultModel);
	}

	private Task<IEnumerable<GenerationResultModel>> CreateThumbAsync(string? subPath,
		string fileHash)
	{
		ArgumentNullException.ThrowIfNull(subPath);

		if ( _exception != null )
		{
			throw _exception;
		}

		_subPathStorage?.WriteStream(
			StringToStreamHelper.StringToStream("test"), fileHash);
		Inputs.Add(new Tuple<string?, string?>(subPath, fileHash));

		return Task.FromResult(new List<GenerationResultModel>
		{
			new()
			{
				Success = _subPathStorage?.ExistFile(subPath) == true,
				IsNotFound = _subPathStorage?.ExistFile(subPath) != true,
				SubPath = subPath,
				FileHash = fileHash
			}
		}.AsEnumerable());
	}
}
