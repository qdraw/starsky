using System;
using System.Collections.Generic;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIThumbnailService : IThumbnailService
	{
		public List<Tuple<string, string>> Inputs { get; set; } = new List<Tuple<string, string>>();
		
		public bool CreateThumb(string subPath)
		{
			Inputs.Add(new Tuple<string, string>(subPath, null));
			return true;
		}

		public bool CreateThumb(string subPath, string fileHash)
		{
			Inputs.Add(new Tuple<string, string>(subPath, fileHash));
			return true;
		}
	}
}
