#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.FakeMocks
{
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public class FakeIThumbnailService : IThumbnailService
	{
		private readonly IStorage? _subPathStorage;
		private readonly Exception? _exception;

		public FakeIThumbnailService(FakeSelectorStorage? selectorStorage = null, Exception? exception = null)
		{
			_subPathStorage = selectorStorage?.Get(SelectorStorage.StorageServices.SubPath);
			_exception = exception;
		}

		public List<Tuple<string, string?>> Inputs { get; set; } = new List<Tuple<string, string?>>();
		
		public Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath)
		{
			if ( _exception != null ) throw _exception;
			
			_subPathStorage?.WriteStream(
				PlainTextFileHelper.StringToStream("test"), subPath);
			Inputs.Add(new Tuple<string, string?>(subPath, null));

			var items = _subPathStorage?.GetAllFilesInDirectory(subPath);
			if ( items == null  )
			{
				return Task.FromResult(new List<GenerationResultModel>{new GenerationResultModel()
				{
					SubPath = subPath,
					Success = true
				}});
			}

			var name = Base32.Encode(System.Text.Encoding.UTF8.GetBytes(subPath));
			_subPathStorage?.WriteStream(
				PlainTextFileHelper.StringToStream("test"), "/"+ name + "@2000.jpg");
			
			var resultModel = new List<GenerationResultModel>();
			foreach ( var item in items )
			{
				resultModel.Add(new GenerationResultModel
				{
					SubPath = item,
					Success = true
				});
			}
			return Task.FromResult(resultModel);
		}

		Task<IEnumerable<GenerationResultModel>> IThumbnailService.CreateThumbAsync(string subPath, string fileHash, bool skipExtraLarge)
		{
			if ( _exception != null ) throw _exception;

			_subPathStorage?.WriteStream(
				PlainTextFileHelper.StringToStream("test"), fileHash);
			Inputs.Add(new Tuple<string, string?>(subPath, fileHash));
			return Task.FromResult(new List<GenerationResultModel>{new GenerationResultModel()
			{
				Success = true,
				SubPath = subPath
			}}.AsEnumerable());
		}

		public List<Tuple<string, int?, int?, int?>> InputsRotate { get; set; } = new List<Tuple<string, int?, int?, int?>>();

		public Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000,
			int height = 0)
		{
			InputsRotate.Add(new Tuple<string, int?, int?, int?>(fileHash, orientation, width, height));
			return Task.FromResult(true);
		}
	}
}
