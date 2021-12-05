using System.Collections.Generic;
using System.Threading.Tasks;
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

		public Task<List<string>> CleanAllUnusedFilesAsync(int chunkSize = 50)
		{
			Inputs.Add(true);
			return Task.FromResult(new List<string>());
		}

		public Task<List<string>> CleanAllUnusedFilesAsync()
		{
			throw new System.NotImplementedException();
		}
	}
}
