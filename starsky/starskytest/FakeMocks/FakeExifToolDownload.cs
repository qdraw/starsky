using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.writemeta.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeExifToolDownload : IExifToolDownload
	{
		public List<bool> Called { get; set; } = new List<bool>();
		public Task<bool> DownloadExifTool(bool isWindows)
		{
			Called.Add(true);
			return Task.FromResult(true);
		}
	}
}
