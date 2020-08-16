using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
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
			
			await new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings)
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
			
			await new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings)
				.WriteTagsThumbnailAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
		}
	}
}
