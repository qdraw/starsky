using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Services;
using starsky.foundation.thumbnailgeneration.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Helpers
{
	[TestClass]
	public class ThumbnailCliTest
	{
		[TestMethod]
		public void Thumbnail_NoArgs_Assume_DoNothing()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
				new FakeIThumbnailService(), new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
			
			thumbnailService.Thumbnail(new string[0]);
			
			Assert.AreEqual("Done!", fakeConsole.WrittenLines[0]);
		}
		
		[TestMethod]
		public void Thumbnail_Enable_T_Param_AssumeHome()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var fakeIThumbnailService = new FakeIThumbnailService();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
				fakeIThumbnailService, new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
			
			thumbnailService.Thumbnail(new []{"-t", "true"});
			
			Assert.AreEqual("/", fakeIThumbnailService.Inputs[0].Item1);
		}
		
		[TestMethod]
		public void Thumbnail_Help()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
				new FakeIThumbnailService(), new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
			
			thumbnailService.Thumbnail(new []{"-h"});
			
			Assert.IsTrue(fakeConsole.WrittenLines[0].Contains("Help"));
		}
		
		[TestMethod]
		public void Thumbnail_Disable_T_Param()
		{
			var fakeThumbnail = new FakeIThumbnailService();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});

			var thumbnailService = new ThumbnailCli(new AppSettings(), new ConsoleWrapper(),
				fakeThumbnail, new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
		
			thumbnailService.Thumbnail(new []{"-t", "false"});

			Assert.AreEqual(0, fakeThumbnail.Inputs.Count);
		}
		
		[TestMethod]
		public void Thumbnail_MinusP_FullPath()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var fakeIThumbnailService = new FakeIThumbnailService();
			var appSettings = new AppSettings();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
				fakeIThumbnailService, new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
			
			thumbnailService.Thumbnail(new []{"-t","true", "-p", Path.Combine(appSettings.StorageFolder, "test")});
			
			Assert.AreEqual("/test", fakeIThumbnailService.Inputs[0].Item1);
		}
		
		[TestMethod]
		public void Thumbnail_MinusS_SubPath()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var fakeIThumbnailService = new FakeIThumbnailService();
			var appSettings = new AppSettings();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
				fakeIThumbnailService, new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
			
			thumbnailService.Thumbnail(new []{"-t","true", "-s", "/test"});
			
			Assert.AreEqual("/test", fakeIThumbnailService.Inputs[0].Item1);
		}
		
		[TestMethod]
		public void Thumbnail_MinusS_SubPath_Direct()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var fakeIThumbnailService = new FakeIThumbnailService();
			var appSettings = new AppSettings();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
				fakeIThumbnailService, new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
			
			thumbnailService.Thumbnail(new []{"-t","true", "-s", "/test.jpg"});
			
			Assert.AreEqual("/test.jpg", fakeIThumbnailService.Inputs[0].Item1);
			Assert.IsNotNull(fakeIThumbnailService.Inputs[0].Item2);
		}
		
		[TestMethod]
		public void Thumbnail_MinusG_Relative()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var fakeIThumbnailService = new FakeIThumbnailService();
			var appSettings = new AppSettings();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var thumbnailService = new ThumbnailCli(appSettings, fakeConsole,
				fakeIThumbnailService, new FakeIThumbnailCleaner(),
				new FakeSelectorStorage(storage));
			
			thumbnailService.Thumbnail(new []{"-t","true", "-g", "0"});
			
			var subPathRelative = new StructureService(new FakeIStorage(),appSettings.Structure)
				.ParseSubfolders(0);
			
			Assert.AreEqual(subPathRelative, fakeIThumbnailService.Inputs[0].Item1);
		}

		[TestMethod]
		public void Thumbnail_MinusX_CleanAllUnusedFiles()
		{
			var fakeConsole = new FakeConsoleWrapper();
			var storage = new FakeIStorage(new List<string> {"/"}, new List<string> {"/test.jpg"});
			var fakeIThumbnailCleaner = new FakeIThumbnailCleaner();
			var thumbnailService = new ThumbnailCli(new AppSettings(), fakeConsole,
				new FakeIThumbnailService(), fakeIThumbnailCleaner,
				new FakeSelectorStorage(storage));
			thumbnailService.Thumbnail(new []{"--clean","true"});

			Assert.IsTrue(fakeIThumbnailCleaner.Inputs[0]);
		}
	}
}
