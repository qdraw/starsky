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
		private readonly IExifTool _exifTool;
		private AppSettings _appSettings;

		public ExifCopyTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IExifTool, FakeExifTool>();    
            
			// build the service
			var serviceProvider = services.BuildServiceProvider();
            
			_exifTool = serviceProvider.GetRequiredService<IExifTool>();
            
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
			var helperResult = new ExifCopy(storage, _exifTool, fakeReadMeta).CopyExifPublish("/test.jpg", "/test2");
			Assert.AreEqual(true,helperResult.Contains("HistorySoftwareAgent"));
		}
	}
}
