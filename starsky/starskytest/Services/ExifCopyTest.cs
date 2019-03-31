using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		private AppSettings _appSettings;

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
				new FakeIStorage(folderPaths, inputSubPaths, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}, new List<string> {"?"});
	        
			var fakeReadMeta = new ReadMeta(storage);
			var fakeExifTool = new FakeExifTool(storage,_appSettings);
			var helperResult = new ExifCopy(storage, fakeExifTool, fakeReadMeta).CopyExifPublish("/test.jpg", "/test2");
			Assert.AreEqual(true,helperResult.Contains("HistorySoftwareAgent"));
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
