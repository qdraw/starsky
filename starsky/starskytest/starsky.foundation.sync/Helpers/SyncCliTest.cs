using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Services;
using starsky.foundation.sync.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers
{
	[TestClass]
	public class SyncCliTest
	{
		[TestMethod]
		public async Task Sync_NoArgs_Assume_DefaultCheck()
		{
			var fakeSync = new FakeISynchronize();
			await new SyncCli(fakeSync, new AppSettings(), new ConsoleWrapper(), 
				new FakeSelectorStorage()).Sync(
				new List<string>{""}.ToArray());
		
			Assert.AreEqual("/", fakeSync.Inputs[0].Item1);
		}
		
		[TestMethod]
		public async Task Sync_NoArgs_NotFound()
		{
			var fakeSync = new FakeISynchronize();
			var console = new FakeConsoleWrapper();
			await new SyncCli(fakeSync, new AppSettings(), console, 
				new FakeSelectorStorage()).Sync(
				new List<string>{""}.ToArray());
		
			Assert.IsTrue(console.WrittenLines.Any(p => p.Contains("Not Found")));
		}
		
		[TestMethod]
		public async Task Sync_NoArgs_ShouldNotSee_NotFound()
		{
			var fakeSync = new FakeISynchronize(new List<FileIndexItem>{new FileIndexItem("/")});
			var console = new FakeConsoleWrapper();
			await new SyncCli(fakeSync, new AppSettings(), console, 
				new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}))).Sync(
				new List<string>{""}.ToArray());

			var t = console.WrittenLines.Any(p => p.Contains("Not Found"));
			Assert.IsFalse(t);
		}
		
		[TestMethod]
		public async Task Sync_Disable_I_Param()
		{
			var fakeSync = new FakeISynchronize();
			await new SyncCli(fakeSync, new AppSettings(), new ConsoleWrapper(), 
				new FakeSelectorStorage()).Sync(
				new List<string>{"-i", "false"}.ToArray());

			Assert.AreEqual(0, fakeSync.Inputs.Count);
		}
		
		[TestMethod]
		public async Task Sync_Help()
		{
			var console = new FakeConsoleWrapper();
			await new SyncCli(new FakeISynchronize(), new AppSettings(), console, 
				new FakeSelectorStorage()).Sync(
				new List<string>{"-h"}.ToArray());

			Assert.IsTrue(console.WrittenLines[0].Contains("Help"));
		}
		
		[TestMethod]
		public async Task Sync_MinusP_FullPath()
		{
			var fakeSync = new FakeISynchronize();
			var appSettings = new AppSettings();
			await new SyncCli(fakeSync, appSettings, new ConsoleWrapper(), 
				new FakeSelectorStorage()).Sync(
				new List<string>{"-p", Path.Combine(appSettings.StorageFolder, "test")}.ToArray());

			Assert.AreEqual("/test", fakeSync.Inputs[0].Item1);
		}
		
		[TestMethod]
		public async Task Sync_MinusS_SubPath()
		{
			var fakeSync = new FakeISynchronize();
			var appSettings = new AppSettings();
			await new SyncCli(fakeSync, appSettings, new ConsoleWrapper(), 
				new FakeSelectorStorage()).Sync(
				new List<string>{"-s", "/test"}.ToArray());

			Assert.AreEqual("/test", fakeSync.Inputs[0].Item1);
		}

		[TestMethod]
		public async Task Sync_MinusG_Relative()
		{
			var fakeSync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncCli = new SyncCli(fakeSync, appSettings, new ConsoleWrapper(),
				new FakeSelectorStorage());
			await syncCli.Sync(
				new List<string>{"-g", "0"}.ToArray());

			var subPathRelative = new StructureService(new FakeIStorage(),appSettings.Structure)
				.ParseSubfolders(0);
			
			Assert.AreEqual(subPathRelative, fakeSync.Inputs[0].Item1);
		}
	}
}
