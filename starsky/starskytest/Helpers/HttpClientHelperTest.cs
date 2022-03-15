using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Helpers
{
	[TestClass]
	public class HttpClientHelperTest
	{
		[TestMethod]
		public async Task Download_HttpClientHelperBadDomainDownload()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var storageProvider = serviceProvider.GetRequiredService<IStorage>();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			// use only whitelisted domains
			var path = Path.Combine(new AppSettings().TempFolder, "pathToNOTdownload.txt");
			var output = await httpClientHelper.Download("http://mybadurl.cn",path);
			Assert.AreEqual(false,output);
		}
		
		[TestMethod]
		public async Task Download_HttpClientHelper_404NotFoundTest()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			// there is an file written
			var path = Path.Combine(new CreateAnImage().BasePath, "file.txt");
			var output = await httpClientHelper.Download("https://download.geonames.org/404",path);
			Assert.AreEqual(false,output);
		}
		
		[TestMethod]
		public async Task Download_HttpClientHelper_HTTP_Not_Download()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			// http is not used anymore
			var path = Path.Combine(new AppSettings().TempFolder, "pathToNOTdownload.txt");
			var output = await httpClientHelper.Download("http://qdraw.nl",path);
			Assert.AreEqual(false,output);
		}
		
		[TestMethod]
		public async Task Download_HttpClientHelper_Download()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			var storageProvider = serviceProvider.GetRequiredService<IStorage>();

			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			// there is an file written
			var path = Path.Combine(new CreateAnImage().BasePath, "file.txt");
			var output = await httpClientHelper.Download("https://qdraw.nl/test",path);
			
			Assert.AreEqual(true,output);
			
			Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,storageProvider.IsFolderOrFile(path));
			storageProvider.FileDelete(path);
		}

		[TestMethod]
		public async Task Download_HttpClientHelper_Download_HttpRequestException()
		{
			// > next HttpRequestException
			var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpRequestException("should fail"));
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);
			
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());
			var output = await httpClientHelper.Download("https://qdraw.nl/test","/sdkflndf");
			Assert.IsFalse(output);
		}

		[TestMethod]
		[ExpectedException(typeof(EndOfStreamException))]
		public async Task Download_HttpClientHelper_Download_NoStorage()
		{
			await new HttpClientHelper(new FakeIHttpProvider(), null, new FakeIWebLogger()).Download("t","T");
		}
		
		[TestMethod]
		public async Task ReadString_HttpClientHelper_ReadString()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			var storageProvider = serviceProvider.GetRequiredService<IStorage>();

			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			var output = await httpClientHelper.ReadString("https://qdraw.nl/test");
			
			Assert.AreEqual(true,output.Key);
		}
		
		
		[TestMethod]
		public async Task ReadString_HttpClientHelper_ReadString_HttpRequestException()
		{
			// > next HttpRequestException
			var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpRequestException("should fail"));
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);
			
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());
			var output = await httpClientHelper.ReadString("https://qdraw.nl/test");
			Assert.IsFalse(output.Key);
		}

		[TestMethod]
		public async Task ReadString_HttpClientHelper_HTTP_Not_ReadString()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			// http is not used anymore
			var output = await httpClientHelper.ReadString("http://qdraw.nl");
			Assert.AreEqual(false,output.Key);
		}
		
		[TestMethod]
		public async Task ReadString_HttpClientHelper_404NotFound_ReadString_Test()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			var output = await httpClientHelper.ReadString("https://download.geonames.org/404");
			Assert.AreEqual(false,output.Key);
		}
				
		[TestMethod]
		public async Task PostString_HttpClientHelper()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			var output = await httpClientHelper
				.PostString("https://qdraw.nl/test", new StringContent(string.Empty));
			
			Assert.AreEqual(true,output.Key);
		}
		
		[TestMethod]
		public async Task PostString_HttpClientHelper_VerboseFalse()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			var fakeLogger = new FakeIWebLogger();
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory,fakeLogger);

			await httpClientHelper
				.PostString("https://qdraw.nl/test", new StringContent(string.Empty),false);
			
			Assert.IsFalse(fakeLogger.TrackedInformation.Any(p => p.Item2.Contains("PostString")));
			Assert.IsFalse(fakeLogger.TrackedInformation.Any(p => p.Item2.Contains("HttpClientHelper")));
		}
		
		[TestMethod]
		public async Task PostString_HttpRequestException()
		{
			// > next HttpRequestException
			var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpRequestException("should fail"));
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);
			
			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());
			var output = await httpClientHelper
				.PostString("https://qdraw.nl/test", new StringContent(string.Empty));
			Assert.IsFalse(output.Key);
		}

		[TestMethod]
		public async Task PostString_HTTP_Not_ReadString()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			// http is not used anymore
			var output = await httpClientHelper
				.PostString("http://qdraw.nl", new StringContent(string.Empty));
			Assert.AreEqual(false,output.Key);
		}
		
		[TestMethod]
		public async Task PostString_404NotFound_Test()
		{
			var fakeHttpMessageHandler = new FakeHttpMessageHandler();
			var httpClient = new HttpClient(fakeHttpMessageHandler);
			var httpProvider = new HttpProvider(httpClient);

			var services = new ServiceCollection();
			services.AddSingleton<IStorage, FakeIStorage>();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

			var output = await httpClientHelper
				.PostString("https://download.geonames.org/404", new StringContent(string.Empty));
			Assert.AreEqual(false,output.Key);
		}


	}
}
