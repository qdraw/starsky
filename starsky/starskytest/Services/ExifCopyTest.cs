using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.writemeta.Services;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Services
{
	[TestClass]
	public class ExifCopyTest
	{
		private readonly AppSettings _appSettings;

		public ExifCopyTest()
		{
			// get the service
			_appSettings = new AppSettings();
		}
		
		[TestMethod]
		public void ExifToolCmdHelper_CopyExifPublish()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.jpg"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
	        
			var fakeReadMeta = new ReadMeta(storage);
			var fakeExifTool = new FakeExifTool(storage,_appSettings);
			var helperResult = new ExifCopy(storage, storage, fakeExifTool, fakeReadMeta).CopyExifPublish("/test.jpg", "/test2");
			Assert.AreEqual(true,helperResult.Contains("HistorySoftwareAgent"));
		}

		[TestMethod]
		public void ExifToolCmdHelper_XmpSync()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.dng"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});

			var fakeReadMeta = new ReadMeta(storage);
			var fakeExifTool = new FakeExifTool(storage,_appSettings);
			var helperResult = new ExifCopy(storage, storage, fakeExifTool, fakeReadMeta).XmpSync("/test.dng");
			Assert.AreEqual("/test.xmp",helperResult);

		}

		[TestMethod]
		public void ExifToolCmdHelper_XmpCreate()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.dng"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});

			var fakeReadMeta = new ReadMeta(storage);
			var fakeExifTool = new FakeExifTool(storage,_appSettings);
			
			new ExifCopy(storage, storage, fakeExifTool, fakeReadMeta).XmpCreate("/test.xmp");
			var result = new PlainTextFileHelper().StreamToString(storage.ReadStream("/test.xmp"));
			Assert.AreEqual("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Starsky'>\n" +
			                "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>\n</rdf:RDF>\n</x:xmpmeta>",result);
		}

//		[TestMethod]
//		public void ExifToolCmdHelper_TestForFakeExifToolInjection()
//		{
//			var folderPaths = new List<string>{"/"};
//			var inputSubPaths = new List<string>{"/test.dng"};
//			
//			var storage = new FakeIStorage(folderPaths, inputSubPaths, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}, new List<string> {"?"});
//			
//			var readMeta =  new ReadMeta(storage);
//
//			new ExifCopy(storage, new FakeExifTool(storage,_appSettings), readMeta).XmpSync("/test.dng");
//			
//			Assert.AreEqual(true,storage.ExistFile("/test.xmp"));
//			var xmpContentReadStream = storage.ReadStream("/test.xmp");
//			var xmpContent = new PlainTextFileHelper().StreamToString(xmpContentReadStream);
//			
//			// Those values are injected by fakeExifTool
//			Assert.AreEqual(true,xmpContent.Contains("<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Image::ExifTool 11.30'>"));
//			Assert.AreEqual(true,xmpContent.Contains("<rdf:li>test</rdf:li>"));
//
//		}

	}
}
