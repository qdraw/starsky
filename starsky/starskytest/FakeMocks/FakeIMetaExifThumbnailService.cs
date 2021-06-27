using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.metathumbnail.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIMetaExifThumbnailService : IMetaExifThumbnailService
	{
		public List<(string, string)> Input { get; set; } =
			new (string, string)[0].ToList();
		
		public Task<bool> AddMetaThumbnail(IEnumerable<(string, string)> subPathsAndHash)
		{
			Input.AddRange(subPathsAndHash);
			return Task.FromResult(true);
		}

		public Task<bool> AddMetaThumbnail(string subPath)
		{
			Input.Add((subPath, null));
			return Task.FromResult(true);
		}

		public Task<bool> AddMetaThumbnail(string subPath, string fileHash)
		{
			Input.Add((subPath, fileHash));
			return Task.FromResult(true);
		}
	}
}
