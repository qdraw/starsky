using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherHelpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
{
	[TestClass]
	public class SyncWatcherConnectorTest
	{
		[TestMethod]
		public void Sync_CheckInput()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, new FakeIWebSocketConnectionsService());
			syncWatcherPreflight.Sync(
				new Tuple<string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), WatcherChangeTypes.Changed));

			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}

		[TestMethod]
		public void Sync_CheckInput_Socket()
		{
			var sync = new FakeISynchronize(new List<FileIndexItem>{new FileIndexItem("/test")});
			var websockets = new FakeIWebSocketConnectionsService();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, websockets);
			syncWatcherPreflight.Sync(
				new Tuple<string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), WatcherChangeTypes.Changed));

			Assert.IsTrue(websockets.FakeSendToAllAsync[0].Contains("filePath\":\"/test\""));
			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}
	}
}
