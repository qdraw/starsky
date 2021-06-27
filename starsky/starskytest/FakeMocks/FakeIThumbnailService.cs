using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIThumbnailService : IThumbnailService
	{
		public List<Tuple<string, string>> Inputs { get; set; } = new List<Tuple<string, string>>();


		public Task<List<(string, bool)>> CreateThumb(string subPath)
		{
			Inputs.Add(new Tuple<string, string>(subPath, null));
			return Task.FromResult(new List<(string, bool)>{(subPath, true)});
		}

		public Task<bool> CreateThumb(string subPath, string fileHash)
		{
			Inputs.Add(new Tuple<string, string>(subPath, fileHash));
			return Task.FromResult(true);
		}
	}
}
