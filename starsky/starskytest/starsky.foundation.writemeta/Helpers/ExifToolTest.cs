using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Helpers
{
	[TestClass]
	public class ExifToolTest
	{

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public async Task ExifTool_NotFound_Exception()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			
			await new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings, new FakeIWebLogger())
				.WriteTagsAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
		}
		
		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public async Task ExifTool_WriteTagsThumbnailAsync_NotFound_Exception()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			
			await new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings, new FakeIWebLogger())
				.WriteTagsThumbnailAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
		}

		[TestMethod]
		public async Task ExifTool_RenameThumbnailByStream_Length26()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			
			var result = await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
				.RenameThumbnailByStream(new KeyValuePair<string, bool>("OLDHASH",true),new MemoryStream());

			Assert.AreEqual(26,result.Length);
		}
		
		[TestMethod]
		public async Task ExifTool_RenameThumbnailByStream_Fail()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			
			var result = await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
				.RenameThumbnailByStream(new KeyValuePair<string, bool>("OLDHASH",false),new MemoryStream());

			Assert.AreEqual(0,result.Length);
		}
	}
}
