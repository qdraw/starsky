using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.writemeta.Services;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.starsky.foundation.writemeta.Services
{
	[TestClass]
	public sealed class ExifCopyTest
	{
		private readonly AppSettings _appSettings;

		public ExifCopyTest()
		{
			// get the service
			_appSettings = new AppSettings();
		}
		
		[TestMethod]
		public async Task ExifToolCmdHelper_CopyExifPublish()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.jpg"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes.ToArray()});
	        
			var fakeReadMeta = new ReadMeta(storage, _appSettings, null, new FakeIWebLogger());
			var fakeExifTool = new FakeExifTool(storage,_appSettings);
			var helperResult = await new ExifCopy(storage, storage, fakeExifTool, 
				fakeReadMeta, new FakeIThumbnailQuery()).CopyExifPublish("/test.jpg", "/test2");
			Assert.AreEqual(true,helperResult.Contains("HistorySoftwareAgent"));
		}

		[TestMethod]
		public async Task ExifToolCmdHelper_XmpSync()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.dng"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes.ToArray()});

			var fakeReadMeta = new ReadMeta(storage, _appSettings, 
				null, new FakeIWebLogger());
			var fakeExifTool = new FakeExifTool(storage,_appSettings);
			var helperResult = await new ExifCopy(storage, 
				storage, fakeExifTool, fakeReadMeta, new FakeIThumbnailQuery()).XmpSync("/test.dng");
			Assert.AreEqual("/test.xmp",helperResult);

		}

		[TestMethod]
		public async Task ExifToolCmdHelper_XmpCreate()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.dng"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes.ToArray()});

			var fakeReadMeta = new ReadMeta(storage, _appSettings,
				null, new FakeIWebLogger());
			var fakeExifTool = new FakeExifTool(storage,_appSettings);
			
			new ExifCopy(storage, storage, fakeExifTool, fakeReadMeta, new FakeIThumbnailQuery())
				.XmpCreate("/test.xmp");
			var result = await StreamToStringHelper.StreamToStringAsync(storage.ReadStream("/test.xmp"));
			Assert.AreEqual("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Starsky'>\n" +
			                "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>\n</rdf:RDF>\n</x:xmpmeta>",result);
		}

		[TestMethod]
		public async Task ExifToolCmdHelper_TestForFakeExifToolInjection()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.dng"};
			
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes.ToArray()});
			
			var readMeta = new ReadMeta(storage, _appSettings,
				null, new FakeIWebLogger());
			var fakeExifTool = new FakeExifTool(storage,_appSettings);

			await new ExifCopy(storage, storage, fakeExifTool, readMeta, new FakeIThumbnailQuery())
				.XmpSync("/test.dng");
			
			Assert.AreEqual(true,storage.ExistFile("/test.xmp"));
			var xmpContentReadStream = storage.ReadStream("/test.xmp");
			var xmpContent = await StreamToStringHelper.StreamToStringAsync(xmpContentReadStream);
			
			// Those values are injected by fakeExifTool
			Assert.AreEqual(true,xmpContent.Contains("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Image::ExifTool 11.30'>"));
			Assert.AreEqual(true,xmpContent.Contains("<rdf:li>test</rdf:li>"));

		}

	}
}
