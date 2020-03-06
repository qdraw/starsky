using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Helpers
{
	[TestClass]
	public class HttpClientHelperTest
	{
		[TestMethod]
		public async Task HttpClientHelperBadDomainDownload()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var httpClientHelper = new HttpClientHelper(httpProvider, new FakeSelectorStorage(new FakeIStorage()));

			// use only whitelisted domains
			var path = Path.Combine(new AppSettings().TempFolder, "pathToNOTdownload.txt");
			var output = await httpClientHelper.Download("http://mybadurl.cn",path);
			Assert.AreEqual(false,output);
		}
		
		[TestMethod]
		public async Task HttpClientHelper_404NotFoundTest()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var httpClientHelper = new HttpClientHelper(httpProvider, new FakeSelectorStorage(new FakeIStorage()));

			// there is an file written
			var path = Path.Combine(new CreateAnImage().BasePath, "file.txt");
			var output = await httpClientHelper.Download("https://download.geonames.org/404",path);
			Assert.AreEqual(false,output);
		}
		
		[TestMethod]
		public async Task HttpClientHelper_HTTP_Not_Download()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var httpClientHelper = new HttpClientHelper(httpProvider, new FakeSelectorStorage(new FakeIStorage()));

			// http is not used anymore
			var path = Path.Combine(new AppSettings().TempFolder, "pathToNOTdownload.txt");
			var output = await httpClientHelper.Download("http://qdraw.nl",path);
			Assert.AreEqual(false,output);
		}
		
		[TestMethod]
		public async Task HttpClientHelper_Download()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var storageProvider = new FakeIStorage();
			var httpClientHelper = new HttpClientHelper(httpProvider, new FakeSelectorStorage(storageProvider));

			// there is an file written
			var path = Path.Combine(new CreateAnImage().BasePath, "file.txt");
			var output = await httpClientHelper.Download("https://qdraw.nl/test",path);
			
			Assert.AreEqual(true,output);
			
			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,storageProvider.IsFolderOrFile(path));
			storageProvider.FileDelete(path);
		}


	}
}
