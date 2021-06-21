using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.metathumbnail.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIMetaExifThumbnailService : IMetaExifThumbnailService
	{
		public Task<bool> AddMetaThumbnail(IEnumerable<(string, string)> subPathsAndHash)
		{
			return Task.FromResult(true);
		}

		public Task<bool> AddMetaThumbnail(string subPath)
		{
			return Task.FromResult(true);
		}

		public Task<bool> AddMetaThumbnail(string subPath, string fileHash)
		{
			return Task.FromResult(true);
		}
	}
}
