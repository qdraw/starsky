using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.thumbnailgeneration.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIThumbnailCleaner : IThumbnailCleaner
{
	public List<bool> Inputs { get; set; } = new();
	public List<string> Files { get; set; } = new();

	public Task<List<string>> CleanAllUnusedFilesAsync(int chunkSize = 50)
	{
		Inputs.Add(true);
		return Task.FromResult(Files);
	}

	public void CleanAllUnusedFiles()
	{
		Inputs.Add(true);
	}
}
