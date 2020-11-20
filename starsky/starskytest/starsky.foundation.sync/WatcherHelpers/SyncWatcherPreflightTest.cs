using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherHelpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
{
	[TestClass]
	public class SyncWatcherPreflightTest
	{
		[TestMethod]
		public void Sync_CheckInput()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherPreflight(new AppSettings(), sync);
			syncWatcherPreflight.Sync(
				new Tuple<string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), WatcherChangeTypes.Changed));

			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}
	}
}
