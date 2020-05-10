using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.writemeta.Services;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	[TestClass]
	public class ExifToolTest
	{

		[TestMethod]
		[ExpectedException(typeof(Win32Exception))]
		public async Task ExifToolNotFoundException()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "Z://Non-exist",
			};

			var fakeStorage = new FakeIStorage(new List<string>{"/"}, 
				new List<string>{"/test.jpg"}, 
				new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			
			await new ExifTool(new FakeSelectorStorage(fakeStorage), appSettings)
				.WriteTagsAsync("/test.jpg","-Software=\"Qdraw 2.0\"");
		}
	}
}
