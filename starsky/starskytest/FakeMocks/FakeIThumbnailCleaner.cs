using System.Collections.Generic;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIThumbnailCleaner : IThumbnailCleaner
	{
		public List<bool> Inputs { get; set; } = new List<bool>();

		public void CleanAllUnusedFiles()
		{
			Inputs.Add(true);
		}
	}
}
