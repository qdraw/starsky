#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
using starskytest.Helpers;

namespace starskytest.FakeMocks
{
	public class FakeIThumbnailService : IThumbnailService
	{
		private readonly IStorage? _subPathStorage;

		public FakeIThumbnailService(FakeSelectorStorage? selectorStorage = null)
		{
			_subPathStorage = selectorStorage?.Get(SelectorStorage.StorageServices.SubPath);
		}

		public List<Tuple<string, string>> Inputs { get; set; } = new List<Tuple<string, string>>();
		
		public Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath)
		{
			_subPathStorage?.WriteStream(
				PlainTextFileHelper.StringToStream("test"), subPath);
			Inputs.Add(new Tuple<string, string>(subPath, null));

			var items = _subPathStorage?.GetAllFilesInDirectory(subPath);
			if ( items == null  )
			{
				return Task.FromResult(new List<GenerationResultModel>{new GenerationResultModel()
				{
					SubPath = subPath,
					Success = true
				}});
			}

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

		Task<IEnumerable<GenerationResultModel>> IThumbnailService.CreateThumbAsync(string subPath, string fileHash)
		{
			_subPathStorage?.WriteStream(
				PlainTextFileHelper.StringToStream("test"), fileHash);
			Inputs.Add(new Tuple<string, string>(subPath, fileHash));
			return Task.FromResult(new List<GenerationResultModel>{new GenerationResultModel()
			{
				Success = true,
				SubPath = subPath
			}}.AsEnumerable());
		}

	}
}
