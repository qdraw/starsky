using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Streaming;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Helpers
{
	[TestClass]
	public sealed class FileStreamingHelperTest
	{
		private readonly AppSettings _appSettings;

		public FileStreamingHelperTest()
		{
			// Add a dependency injection feature
			var services = new ServiceCollection();
			// Inject Config helper
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			// random config
			var newImage = new CreateAnImage();
			var dict = new Dictionary<string, string?>
			{
				{ "App:StorageFolder", newImage.BasePath },
				{ "App:ThumbnailTempFolder", newImage.BasePath },
				{ "App:Verbose", "true" }
			};
			// Start using dependency injection
			var builder = new ConfigurationBuilder();
			// Add random config to dependency injection
			builder.AddInMemoryCollection(dict);
			// build config
			var configuration = builder.Build();
			// inject config as object to a service
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
			// build the service
			var serviceProvider = services.BuildServiceProvider();
			// get the service
			_appSettings = serviceProvider.GetRequiredService<AppSettings>();
		}

		[TestMethod]
		[ExpectedException(typeof(FileLoadException))]
		public async Task StreamFileExeption()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["token"] = "fake_token_here"; //Set header

			await FileStreamingHelper.StreamFile(httpContext.Request, _appSettings,
				new FakeSelectorStorage(new FakeIStorage()));
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidDataException))]
		public async Task StreamFilemultipart()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["token"] = "fake_token_here"; //Set header
			httpContext.Request.ContentType = "multipart/form-data";

			await FileStreamingHelper.StreamFile(httpContext.Request, _appSettings,
				new FakeSelectorStorage(new FakeIStorage()));
		}

		[TestMethod]
		public async Task FileStreamingHelperTest_FileStreamingHelper_StreamFile_imagejpeg()
		{
			var createAnImage = new CreateAnImage();

			FileStream requestBody = new FileStream(createAnImage.FullFilePath, FileMode.Open);
			_appSettings.TempFolder = createAnImage.BasePath;

			var streamSelector =
				new FakeSelectorStorage(new StorageHostFullPathFilesystem(new FakeIWebLogger()));
			var formValueProvider = await FileStreamingHelper.StreamFile("image/jpeg",
				requestBody, _appSettings, streamSelector);

			Assert.AreNotEqual(null, formValueProvider.ToString());
			await requestBody.DisposeAsync();

			Assert.IsNotNull(formValueProvider.FirstOrDefault());

			// Clean
			streamSelector.Get(SelectorStorage.StorageServices.HostFilesystem)
				.FileDelete(formValueProvider.FirstOrDefault()!);
		}

		[TestMethod]
		public void FileStreamingHelper_HeaderFileName_normalStringTest()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["filename"] = "2018-07-20 20.14.52.jpg"; //Set header
			var result = FileStreamingHelper.HeaderFileName(httpContext.Request);
			Assert.AreEqual("2018-07-20-201452.jpg", result);
		}

		[TestMethod]
		public void FileStreamingHelper_HeaderFileName_Uppercase()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["filename"] = "UPPERCASE.jpg"; //Set header
			var result = FileStreamingHelper.HeaderFileName(httpContext.Request);
			Assert.AreEqual("UPPERCASE.jpg", result);
		}

		[TestMethod]
		public void FileStreamingHelper_HeaderFileName_base64StringTest()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["filename"] =
				"MjAxOC0wNy0yMCAyMC4xNC41Mi5qcGc="; //Set header
			var result = FileStreamingHelper.HeaderFileName(httpContext.Request);
			Assert.AreEqual("2018-07-20-201452.jpg", result);
		}

		[TestMethod]
		public void FileStreamingHelper_HeaderFileName_base64StringTest_Uppercase()
		{
			var httpContext = new DefaultHttpContext(); // or mock a `HttpContext`
			httpContext.Request.Headers["filename"] = "VVBQRVJDQVNFLkpQRw=="; //Set header
			var result = FileStreamingHelper.HeaderFileName(httpContext.Request);
			Assert.AreEqual("UPPERCASE.JPG", result);
		}

		/// <summary>
		/// @see: https://github.com/dotnet/aspnetcore/blob/main/src/Http/WebUtilities/test/MultipartReaderTests.cs
		/// </summary>
		private const string TwoPartBody =
			"--9051914041544843365972754266\r\n" +
			"Content-Disposition: form-data; name=\"text\"\r\n" +
			"\r\n" +
			"text default\r\n" +
			"--9051914041544843365972754266\r\n" +
			"Content-Disposition: form-data; name=\"file1\"; filename=\"a.txt\"\r\n" +
			"Content-Type: text/plain\r\n" +
			"\r\n" +
			"Content of a.txt.\r\n" +
			"\r\n" +
			"--9051914041544843365972754266--\r\n";

		private static MemoryStream MakeStream(string text)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(text));
		}

		private const string Boundary = "9051914041544843365972754266";


		[TestMethod]
		public async Task FileStreamingHelper_MultipartRequestHelper()
		{
			var contentType = $"multipart/form-data; boundary=\"{Boundary}\"";

			// string contentType, Stream requestBody, AppSettings appSettings, 
			// ISelectorStorage selectorStorage, string headerFileName = null

			var stream = MakeStream(TwoPartBody);
			var storage = new FakeIStorage();
			storage.CreateDirectory(_appSettings.TempFolder);
			var streamSelector = new FakeSelectorStorage(storage);

			await FileStreamingHelper.StreamFile(contentType, stream, _appSettings, streamSelector);

			var tempPath = storage.GetAllFilesInDirectoryRecursive(_appSettings.TempFolder)
				.FirstOrDefault();
			Assert.IsTrue(tempPath?.EndsWith("a.txt"));
		}
	}
}
