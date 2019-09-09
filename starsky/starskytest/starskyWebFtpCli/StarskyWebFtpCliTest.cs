using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Interfaces;
using starskycore.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using starskywebftpcli.Services;

namespace starskytest.starskyWebFtpCli
{
	[TestClass]
	public class StarskyWebFtpCliTest
	{
		private readonly AppSettings _appSettings;
		private readonly IStorage _storage;

		public StarskyWebFtpCliTest()
		{
			_appSettings = new AppSettings
			{
				WebFtp = "ftp://test:test@testmedia.be",
				Name = "test",
				PublishProfiles = new List<AppSettingsPublishProfiles>
				{
					new AppSettingsPublishProfiles
					{
						Folder = "large",
						SourceMaxWidth = 1,
						ContentType = TemplateContentType.Jpeg,
						Copy = true
					}
				}
			};
			var newImage = CreateAnImage.Bytes;

			var exampleConfig = "{\"Name\":\"0001-readonly\",\"Slug\":\"0001-readonly\",\"Export\":\"20190908145825\",\"Version\":\"0.1.5.9\"}";
			var exampleConfigBytes = Encoding.ASCII.GetBytes(exampleConfig);
			_storage = new FakeIStorage(new List<string>{"/","/large"},new List<string>{"/test.jpg","/_settings.json"},new List<byte[]>{newImage,exampleConfigBytes});
			var t = _storage.GetAllFilesInDirectory("/").ToList();
			
		}
		
		[TestMethod]
		public void StarskyWebHtmlCli_CreateListOfRemoteDirectoriesTest()
		{
			var item  = new FtpService(_appSettings, _storage).CreateListOfRemoteDirectories().ToList();
			Assert.AreEqual("ftp://testmedia.be//test",item[1]);
			Assert.AreEqual("ftp://testmedia.be//test/large/",item[2]);
		}

		[TestMethod]
		public void StarskyWebHtmlCli_CreateListOfRemoteFilesTest()
		{
			var item  = new FtpService(_appSettings, _storage).CreateListOfRemoteFiles().ToList();
			Assert.AreEqual("/test.jpg",item.FirstOrDefault());
		}

	}
}
