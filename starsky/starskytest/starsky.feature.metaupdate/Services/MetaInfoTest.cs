using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Services
{
	[TestClass]
	public sealed class MetaInfoTest
	{
		[TestMethod]
		public async Task FileNotInIndex()
		{
			var metaInfo = new MetaInfo(new FakeIQuery(), new AppSettings(),
				new FakeSelectorStorage(), null!, new FakeIWebLogger());
			var test = await metaInfo.GetInfoAsync(new List<string> { "/test" }, false);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,
				test.FirstOrDefault()?.Status);
		}

		[TestMethod]
		public async Task NotFoundSourceMissing()
		{
			var metaInfo = new MetaInfo(
				new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test") }),
				new AppSettings(),
				new FakeSelectorStorage(), null!, new FakeIWebLogger());
			var test = await metaInfo.GetInfoAsync(new List<string> { "/test" }, false);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
				test.FirstOrDefault()?.Status);
		}

		[TestMethod]
		public async Task ExtensionNotSupported_ExifWriteNotSupported()
		{
			var metaInfo = new MetaInfo(
				new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test") }),
				new AppSettings(),
				new FakeSelectorStorage(new FakeIStorage(new List<string>(),
					new List<string> { "/test" })), null!, new FakeIWebLogger());
			var test = await metaInfo.GetInfoAsync(new List<string> { "/test" }, false);
			Assert.AreEqual(FileIndexItem.ExifStatus.ExifWriteNotSupported,
				test.FirstOrDefault()?.Status);
		}

		[TestMethod]
		public async Task GetInfo_XmpFile()
		{
			var metaInfo = new MetaInfo(
				new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test.xmp") }),
				new AppSettings(),
				new FakeSelectorStorage(new FakeIStorage(new List<string>(),
					new List<string> { "/test.xmp" },
					new List<byte[]> { FakeCreateAn.CreateAnXmp.Bytes.ToArray() })), null,
				new FakeIWebLogger());
			var test = await metaInfo.GetInfoAsync(new List<string> { "/test.xmp" }, false);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, test.FirstOrDefault()?.Status);
		}

		[TestMethod]
		public async Task GetInfo_JpegFile_OkStatus()
		{
			var metaInfo = new MetaInfo(
				new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test.jpg") }),
				new AppSettings(),
				new FakeSelectorStorage(new FakeIStorage(new List<string>(),
					new List<string> { "/test.jpg" },
					new List<byte[]> { FakeCreateAn.CreateAnImage.Bytes.ToArray() })), null,
				new FakeIWebLogger());
			var test = await metaInfo.GetInfoAsync(new List<string> { "/test.jpg" }, false);

			Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg,
				test.FirstOrDefault()?.ImageFormat);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, test.FirstOrDefault()?.Status);
		}

		[TestMethod]
		public async Task GetInfo_JpegFile_LastWriteDate()
		{
			var metaInfo = new MetaInfo(
				new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test.jpg") }),
				new AppSettings(),
				new FakeSelectorStorage(new FakeIStorage(new List<string>(),
					new List<string> { "/test.jpg" },
					new List<byte[]> { FakeCreateAn.CreateAnImage.Bytes.ToArray() },
					new List<DateTime>
					{
						new DateTime(2000, 01, 01,
							01, 01, 01, kind: DateTimeKind.Local)
					})), null, new FakeIWebLogger());
			var test = await metaInfo.GetInfoAsync(new List<string> { "/test.jpg" }, false);

			Assert.AreEqual(new DateTime(2000, 01, 01,
				01, 01, 01, kind: DateTimeKind.Local), test.FirstOrDefault()?.LastEdited);
		}
	}
}
