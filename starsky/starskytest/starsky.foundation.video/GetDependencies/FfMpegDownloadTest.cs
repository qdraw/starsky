using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegDownloadTest
{
	[TestMethod]
	public async Task DownloadFfMpegTest()
	{
		var sut = new FfMpegDownload(
			new HttpClientHelper(new HttpProvider(new HttpClient()),
				new StorageHostFullPathFilesystem(new FakeIWebLogger()), new FakeIWebLogger()),
			new AppSettings(), new FakeIWebLogger());

		await sut.DownloadFfMpeg();

		Assert.Fail();
	}
}
