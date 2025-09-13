using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Structure;
using starsky.foundation.sync.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public sealed class SyncCliTest
{
	[TestMethod]
	public async Task Sync_NoArgs_Assume_DefaultCheck()
	{
		var fakeSync = new FakeISynchronize();
		await new SyncCli(fakeSync, new AppSettings(), new ConsoleWrapper(),
			new FakeSelectorStorage(), new FakeIWebLogger()).Sync(
			[""]);

		Assert.AreEqual("/", fakeSync.Inputs[0].Item1);
	}

	[TestMethod]
	public async Task Sync_NoArgs_NotFound()
	{
		var fakeSync = new FakeISynchronize();
		var console = new FakeConsoleWrapper();
		await new SyncCli(fakeSync, new AppSettings(), console,
			new FakeSelectorStorage(), new FakeIWebLogger()).Sync(
			[""]);

		Assert.IsTrue(console.WrittenLines.Exists(p => p.Contains("Not Found")));
	}

	[TestMethod]
	public async Task Sync_NoArgs_ShouldNotSee_NotFound()
	{
		var fakeSync = new FakeISynchronize(new List<FileIndexItem> { new("/") });
		var console = new FakeConsoleWrapper();
		await new SyncCli(fakeSync, new AppSettings(), console,
			new FakeSelectorStorage(new FakeIStorage(new List<string> { "/" })),
			new FakeIWebLogger()).Sync(
			[""]);

		var t = console.WrittenLines.Exists(p => p.Contains("Not Found"));
		Assert.IsFalse(t);
	}

	[TestMethod]
	public async Task Sync_Disable_I_Param()
	{
		var fakeSync = new FakeISynchronize();
		await new SyncCli(fakeSync, new AppSettings(), new ConsoleWrapper(),
			new FakeSelectorStorage(), new FakeIWebLogger()).Sync(
			["-i", "false"]);

		Assert.IsEmpty(fakeSync.Inputs);
	}

	[TestMethod]
	public async Task Sync_Help()
	{
		var console = new FakeConsoleWrapper();
		await new SyncCli(new FakeISynchronize(), new AppSettings(), console,
			new FakeSelectorStorage(), new FakeIWebLogger()).Sync(
			["-h"]);

		Assert.Contains("Help", console.WrittenLines[0]);
	}

	[TestMethod]
	public async Task Sync_MinusP_FullPath()
	{
		var fakeSync = new FakeISynchronize();
		var appSettings = new AppSettings();
		await new SyncCli(fakeSync, appSettings, new ConsoleWrapper(),
			new FakeSelectorStorage(), new FakeIWebLogger()).Sync(
			["-p", Path.Combine(appSettings.StorageFolder, "test")]);

		Assert.AreEqual("/test", fakeSync.Inputs[0].Item1);
	}

	[TestMethod]
	public async Task Sync_MinusS_SubPath()
	{
		var fakeSync = new FakeISynchronize();
		var appSettings = new AppSettings();
		await new SyncCli(fakeSync, appSettings, new ConsoleWrapper(),
			new FakeSelectorStorage(), new FakeIWebLogger()).Sync(
			["-s", "/test"]);

		Assert.AreEqual("/test", fakeSync.Inputs[0].Item1);
	}

	[TestMethod]
	public async Task Sync_MinusG_Relative()
	{
		var fakeSync = new FakeISynchronize();
		var appSettings = new AppSettings();
		var syncCli = new SyncCli(fakeSync, appSettings, new ConsoleWrapper(),
			new FakeSelectorStorage(), new FakeIWebLogger());
		await syncCli.Sync(
			["-g", "0"]);

		var subPathRelative = new StructureService(new FakeSelectorStorage(),
				appSettings.Structure,
				new FakeIWebLogger())
			.ParseSubfolders(0);

		Assert.AreEqual(subPathRelative, fakeSync.Inputs[0].Item1);
	}

	[TestMethod]
	public void GetStopWatchTextMinMinutes_WithMinutes()
	{
		var stopWatch = Stopwatch.StartNew();
		stopWatch.Stop();
		var text = SyncCli.GetStopWatchText(stopWatch, 0);
		Assert.Contains("(in sec:", text);
		Assert.Contains("min", text); // TRUE
	}

	[TestMethod]
	public void GetStopWatchTextMinMinutes_WithoutMinutes()
	{
		var stopWatch = Stopwatch.StartNew();
		stopWatch.Stop();
		var text = SyncCli.GetStopWatchText(stopWatch, 999);
		Assert.Contains("(in sec:", text);
		Assert.DoesNotContain("min", text); // FALSE
	}
}
