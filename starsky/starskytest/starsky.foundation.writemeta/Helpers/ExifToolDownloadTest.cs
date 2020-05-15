using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;
using starsky.foundation.writemeta.Helpers;

namespace starskytest.starsky.foundation.writemeta.Helpers
{
	[TestClass]
	public class ExifToolDownloadTest
	{
		[TestMethod]
		public async Task TEst01()
		{
			await new ExifToolDownload(new HttpClientHelper(new HttpProvider(new HttpClient()), null))
				.DownloadExifToolForWindows();
			
		}
	}
}
