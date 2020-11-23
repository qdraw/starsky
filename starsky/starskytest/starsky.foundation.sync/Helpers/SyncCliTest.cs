using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
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

			var subPathRelative = syncCli.GetSubPathRelative(0);
			
			Assert.AreEqual(subPathRelative, fakeSync.Inputs[0].Item1);
		}

		[TestMethod]
		public void GetSubPathRelativeNull()
		{
			var subPathRelative = new SyncCli(null,null,null,null).GetSubPathRelative(null);
			Assert.IsNull(subPathRelative);
		}
		
		[TestMethod]
		public void GetSubPathRelativeToday()
		{
			var subPathRelative = new SyncCli(null,new AppSettings
			{
				Structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext"
			},null,new FakeSelectorStorage()).GetSubPathRelative(0);
			
			Assert.IsTrue(subPathRelative.Contains(DateTime.UtcNow.ToString("yyyy_MM_dd")));
		}
	}
}
