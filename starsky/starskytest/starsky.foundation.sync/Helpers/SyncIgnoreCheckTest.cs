using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.sync.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers
{
	[TestClass]
	public class SyncIgnoreCheckTest
	{
		[TestMethod]
		public void With_No_config()
		{
			var result = new SyncIgnoreCheck(new AppSettings{ SyncIgnore = new List<string>()}, new ConsoleWrapper()).Filter(
				"/test");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void ShouldIgnoreThisFolder()
		{
			var result = new SyncIgnoreCheck(new AppSettings
			{
				SyncIgnore = new List<string>{"/lost+found"}
			}, new ConsoleWrapper()).Filter(
				"/test");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void DirectHit()
		{
			var result = new SyncIgnoreCheck(new AppSettings
			{
				SyncIgnore = new List<string>{"/lost+found"}
			}, new ConsoleWrapper()).Filter(
				"/lost+found");
			Assert.IsTrue(result);
		}
				
		[TestMethod]
		public void ChildItemHit()
		{
			var result = new SyncIgnoreCheck(new AppSettings
			{
				SyncIgnore = new List<string>{"/lost+found"}
			}, new ConsoleWrapper()).Filter(
				"/lost+found/test.jpg");
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void ShouldIgnoreThisFolder_NoConsoleHit()
		{
			var fakeConsole = new FakeConsoleWrapper();
			new SyncIgnoreCheck(new AppSettings
			{
				Verbose = true,
				SyncIgnore = new List<string>{"/lost+found"}
			}, fakeConsole).Filter(
				"/test");
			Assert.AreEqual(0, fakeConsole.WrittenLines.Count);
		}
		
		[TestMethod]
		public void DirectHit_ConsoleHit()
		{
			var fakeConsole = new FakeConsoleWrapper();
			new SyncIgnoreCheck(new AppSettings
			{
				SyncIgnore = new List<string>{"/lost+found"},
				Verbose = true
			}, fakeConsole).Filter(
				"/lost+found");
			Assert.AreEqual(1, fakeConsole.WrittenLines.Count);
		}
	}
}
