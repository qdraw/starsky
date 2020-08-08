using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Services
{
	[TestClass]
	public class FtpServiceTest
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _storage;

		public FtpServiceTest()
		{
			_appSettings = new AppSettings
			{
				WebFtp = "ftp://test:test@testmedia.be",
			};
			_storage = new FakeIStorage(new List<string>{"/","//large","/large"});
		}
		
		[TestMethod]
		public void CreateListOfRemoteDirectories_default()
		{
			var item  = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(),
					new WebRequestAbstraction())
				.CreateListOfRemoteDirectories("/", "item-name", 
					new Dictionary<string, bool>() ).ToList();
			
			Assert.AreEqual("ftp://testmedia.be/",item[0]);
			Assert.AreEqual("ftp://testmedia.be//item-name",item[1]);
		}
		
		[TestMethod]
		public void CreateListOfRemoteDirectories_default_useCopyContent()
		{
			var item  = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(),
					new WebRequestAbstraction())
				.CreateListOfRemoteDirectories("/", "item-name", 
					new Dictionary<string, bool>{{"large/test.jpg",true}} ).ToList();
			
			// start with index 2
			Assert.AreEqual("ftp://testmedia.be//item-name//large",item[2]);
		}
		
		[TestMethod]
		public void CreateListOfRemoteFilesTest()
		{
			var copyContent = new Dictionary<string,bool>
			{
				{"/test.jpg",true}
			};
			var item  = new FtpService(_appSettings, _storage, new FakeConsoleWrapper(),
				new WebRequestAbstraction()).CreateListOfRemoteFiles(copyContent).ToList();
			
			Assert.AreEqual("//test.jpg",item.FirstOrDefault());
		}
	}
}
