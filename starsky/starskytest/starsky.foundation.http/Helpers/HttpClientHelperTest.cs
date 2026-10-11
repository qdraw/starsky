using System;
using System.IO;
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

namespace starskytest.starsky.foundation.http.Helpers;

[TestClass]
public sealed class HttpClientHelperTest
{
	[TestMethod]
	public async Task Download_HttpClientHelperBadDomainDownload()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		// use only whitelisted domains
		var path = Path.Combine(new AppSettings().TempFolder, "pathToNOT_download.txt");
		var output = await httpClientHelper.Download("https://mybadurl.cn", path);
		Assert.IsFalse(output);
	}

	[TestMethod]
	public async Task Download_HttpClientHelper_404NotFoundTest()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		// there is a file written
		var path = Path.Combine(new CreateAnImage().BasePath, "file.txt");
		var output = await httpClientHelper.Download("https://download.geonames.org/404", path);
		Assert.IsFalse(output);
	}

	[TestMethod]
	public async Task Download_HttpClientHelper_Ok_ReadString_Ctor()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var httpClientHelper =
			new HttpClientHelper(httpProvider, new FakeIStorage(), new FakeIWebLogger());

		var output = await httpClientHelper.ReadString("https://qdraw.nl/test");

		Assert.IsTrue(output.Key);
	}

	[TestMethod]
	public async Task Download_HttpClientHelper_HTTP_Not_Download()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		// http is not used anymore
		var path = Path.Combine(new AppSettings().TempFolder, "pathToNOT_download.txt");
		var output = await httpClientHelper.Download("http://qdraw.nl", path);
		Assert.IsFalse(output);
	}

	[TestMethod]
	public async Task Download_HttpClientHelper_Download()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var storageProvider = serviceProvider.GetRequiredService<IStorage>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		// there is a file written
		var path = Path.Combine(new CreateAnImage().BasePath, "file.txt");
		var output = await httpClientHelper.Download("https://qdraw.nl/test", path);

		Assert.IsTrue(output);

		Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,
			storageProvider.IsFolderOrFile(path));
		storageProvider.FileDelete(path);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Download_HttpClientHelper_Download_HttpRequestException()
	{
		// > next HttpRequestException
		var fakeHttpMessageHandler =
			new FakeHttpMessageHandler(new HttpRequestException("should fail"));
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());
		var output = await httpClientHelper.Download("https://qdraw.nl/test", "/random_path", 1);
		Assert.IsFalse(output);
	}

	[TestMethod]
	public async Task Download_HttpClientHelper_Download_NoStorage()
	{
		var sut = new HttpClientHelper(new FakeIHttpProvider(), null as IServiceScopeFactory,
			new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<EndOfStreamException>(() => sut
			.Download("https://qdraw.nl/t", "T"));
	}

	[TestMethod]
	public async Task Download_HttpClientHelper_InvalidUrl_ReturnsFalse()
	{
		var sut = new HttpClientHelper(new FakeIHttpProvider(), null as IServiceScopeFactory,
			new FakeIWebLogger());

		Assert.IsFalse(await sut.Download("t", "T"));
	}

	[TestMethod]
	public async Task ReadString_HttpClientHelper_ReadString()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		var output = await httpClientHelper.ReadString("https://qdraw.nl/test");

		Assert.IsTrue(output.Key);
	}


	[TestMethod]
	public async Task ReadString_HttpClientHelper_ReadString_HttpRequestException()
	{
		// > next HttpRequestException
		var fakeHttpMessageHandler =
			new FakeHttpMessageHandler(new HttpRequestException("should fail"));
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());
		var output = await httpClientHelper.ReadString("https://qdraw.nl/test");
		Assert.IsFalse(output.Key);
	}

	[TestMethod]
	public async Task ReadString_HttpClientHelper_HTTP_Not_ReadString()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		// http is not used anymore
		var output = await httpClientHelper.ReadString("http://qdraw.nl");
		Assert.IsFalse(output.Key);
	}

	[TestMethod]
	public async Task ReadString_HttpClientHelper_404NotFound_ReadString_Test()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		var output = await httpClientHelper.ReadString("https://download.geonames.org/404");
		Assert.IsFalse(output.Key);
	}

	[TestMethod]
	public async Task PostString_HttpClientHelper()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		var output = await httpClientHelper
			.PostString("https://qdraw.nl/test", new StringContent(string.Empty));

		Assert.IsTrue(output.Key);
	}

	[TestMethod]
	public async Task PostString_HttpClientHelper_VerboseFalse()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var fakeLogger = new FakeIWebLogger();
		var httpClientHelper = new HttpClientHelper(httpProvider, scopeFactory, fakeLogger);

		await httpClientHelper
			.PostString("https://qdraw.nl/test", new StringContent(string.Empty), verbose: false);

		Assert.IsFalse(
			fakeLogger.TrackedInformation.Exists(p => p.Item2?.Contains("PostString") == true));
		Assert.IsFalse(fakeLogger.TrackedInformation.Exists(p =>
			p.Item2?.Contains("HttpClientHelper") == true));
	}

	[TestMethod]
	public async Task PostString_HttpRequestException()
	{
		// > next HttpRequestException
		var fakeHttpMessageHandler =
			new FakeHttpMessageHandler(new HttpRequestException("should fail"));
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());
		var output = await httpClientHelper
			.PostString("https://qdraw.nl/test", new StringContent(string.Empty));
		Assert.IsFalse(output.Key);
	}

	[TestMethod]
	public async Task PostString_HTTP_Not_ReadString()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		// http is not used anymore
		var output = await httpClientHelper
			.PostString("http://qdraw.nl", new StringContent(string.Empty));
		Assert.IsFalse(output.Key);
	}

	[TestMethod]
	public async Task PostString_404NotFound_Test()
	{
		var fakeHttpMessageHandler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(fakeHttpMessageHandler);
		var httpProvider = new HttpProvider(new FakeIHttpClientFactory(httpClient));

		var services = new ServiceCollection();
		services.AddSingleton<IStorage, FakeIStorage>();
		services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var httpClientHelper =
			new HttpClientHelper(httpProvider, scopeFactory, new FakeIWebLogger());

		var output = await httpClientHelper
			.PostString("https://download.geonames.org/404", new StringContent(string.Empty));
		Assert.IsFalse(output.Key);
	}

	private sealed class ScriptedHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
		: FakeHttpMessageHandler
	{
		public override HttpResponseMessage Send(HttpRequestMessage request)
		{
			LastRequestMessage.Add(request);
			return respond(request);
		}
	}

	private static (HttpClientHelper helper, FakeIStorage storage, ScriptedHandler handler)
		CreateRestricted(Func<HttpRequestMessage, HttpResponseMessage> respond,
			long maxBytes = 1000)
	{
		var handler = new ScriptedHandler(respond);
		var provider = new HttpProvider(new FakeIHttpClientFactory(new HttpClient(handler)));
		var storage = new FakeIStorage();
		return (new HttpClientHelper(provider, storage, new FakeIWebLogger(), maxBytes), storage,
			handler);
	}

	private static HttpResponseMessage Redirect(string to)
	{
		var r = new HttpResponseMessage(System.Net.HttpStatusCode.Found);
		r.Headers.Location = new Uri(to);
		return r;
	}

	[TestMethod]
	public async Task Download_Restricted_RedirectToNotAllowedHost_ShouldNotFollow()
	{
		var (helper, storage, handler) = CreateRestricted(_ =>
			Redirect("https://169.254.169.254/latest/meta-data"));

		var result = await helper.Download("https://qdraw.nl/a", "/out.bin", 0,
			restricted: true);

		Assert.IsFalse(result);
		Assert.HasCount(1, handler.LastRequestMessage);
		Assert.IsFalse(storage.ExistFile("/out.bin"));
	}

	[TestMethod]
	public async Task Download_Restricted_RedirectToHttp_ShouldNotFollow()
	{
		var (helper, _, handler) = CreateRestricted(_ => Redirect("http://qdraw.nl/b"));

		var result = await helper.Download("https://qdraw.nl/a", "/out.bin", 0,
			restricted: true);

		Assert.IsFalse(result);
		Assert.HasCount(1, handler.LastRequestMessage);
	}

	[TestMethod]
	public async Task Download_Restricted_RedirectToAllowedHost_ShouldFollow()
	{
		var (helper, storage, handler) = CreateRestricted(request =>
			request.RequestUri!.AbsolutePath == "/a"
				? Redirect("https://media.qdraw.nl/b")
				: new HttpResponseMessage(System.Net.HttpStatusCode.OK)
				{
					Content = new ByteArrayContent([1, 2, 3])
				});

		var result = await helper.Download("https://qdraw.nl/a", "/out.bin", 0,
			restricted: true);

		Assert.IsTrue(result);
		Assert.HasCount(2, handler.LastRequestMessage);
		Assert.IsTrue(storage.ExistFile("/out.bin"));
	}

	[TestMethod]
	public async Task Download_Restricted_TooManyRedirects_ShouldReturnFalse()
	{
		var (helper, _, handler) = CreateRestricted(_ => Redirect("https://qdraw.nl/loop"));

		var result = await helper.Download("https://qdraw.nl/a", "/out.bin", 0,
			restricted: true);

		Assert.IsFalse(result);
		Assert.HasCount(HttpClientHelper.MaxRedirectHops + 1, handler.LastRequestMessage);
	}

	[TestMethod]
	public async Task Download_Restricted_ContentLengthTooLarge_ShouldReturnFalse()
	{
		var (helper, storage, _) = CreateRestricted(_ =>
			new HttpResponseMessage(System.Net.HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(new byte[2000])
			}, 1000);

		var result = await helper.Download("https://qdraw.nl/a", "/out.bin", 0,
			restricted: true);

		Assert.IsFalse(result);
		Assert.IsFalse(storage.ExistFile("/out.bin"));
	}

	[TestMethod]
	public async Task Download_Restricted_StreamTooLargeWithoutContentLength_ShouldReturnFalse()
	{
		var (helper, storage, _) = CreateRestricted(_ =>
			new HttpResponseMessage(System.Net.HttpStatusCode.OK)
			{
				// no Content-Length header: only enforced while streaming
				Content = new StreamContent(new NoLengthStream(new byte[2000]))
			}, 1000);

		var result = await helper.Download("https://qdraw.nl/a", "/out.bin", 0,
			restricted: true);

		Assert.IsFalse(result);
		Assert.IsFalse(storage.ExistFile("/out.bin"));
	}

	[TestMethod]
	public async Task Download_Restricted_WithinLimit_ShouldWrite()
	{
		var (helper, storage, _) = CreateRestricted(_ =>
			new HttpResponseMessage(System.Net.HttpStatusCode.OK)
			{
				Content = new ByteArrayContent(new byte[500])
			}, 1000);

		var result = await helper.Download("https://qdraw.nl/a", "/out.bin", 0,
			restricted: true);

		Assert.IsTrue(result);
		Assert.IsTrue(storage.ExistFile("/out.bin"));
	}

	[TestMethod]
	public async Task Download_InvalidUrlString_ShouldReturnFalse()
	{
		var (helper, _, handler) = CreateRestricted(_ =>
			new HttpResponseMessage(System.Net.HttpStatusCode.OK));

		var result = await helper.Download("not a url", "/out.bin", 0, restricted: true);

		Assert.IsFalse(result);
		Assert.IsEmpty(handler.LastRequestMessage);
	}

	[TestMethod]
	public async Task Download_DropboxUserContent_IsNoLongerAllowed()
	{
		var (helper, _, handler) = CreateRestricted(_ =>
			new HttpResponseMessage(System.Net.HttpStatusCode.OK));

		var result = await helper.Download("https://dl.dropboxusercontent.com/a", "/out.bin", 0);

		Assert.IsFalse(result);
		Assert.IsEmpty(handler.LastRequestMessage);
	}

	/// <summary>
	///     Stream that has no Length, so HttpContent can not report Content-Length
	/// </summary>
	private sealed class NoLengthStream(byte[] data) : Stream
	{
		private readonly MemoryStream _inner = new(data);
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count) =>
			_inner.Read(buffer, offset, count);

		public override void Flush() { }
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) =>
			throw new NotSupportedException();
	}
}
