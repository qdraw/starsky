using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Helpers
{
	[TestClass]
	public sealed class ExifToolTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task ExifTool_NotFound_Exception()
		{
			var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist", };

			var fakeStorage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.jpg" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });

			await new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings,
					new FakeIWebLogger())
				.WriteTagsAsync("/test.jpg", "-Software=\"Qdraw 2.0\"");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task ExifTool_WriteTagsThumbnailAsync_NotFound_Exception()
		{
			var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist", };

			var fakeStorage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.jpg" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });

			await new ExifToolService(new FakeSelectorStorage(fakeStorage), appSettings,
					new FakeIWebLogger())
				.WriteTagsThumbnailAsync("/test.jpg", "-Software=\"Qdraw 2.0\"");
		}

		[TestMethod]
		public async Task ExifTool_RenameThumbnailByStream_Length26()
		{
			var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist", };

			var fakeStorage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.jpg" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });

			var result =
				await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
					.RenameThumbnailByStream("OLDHASH", new MemoryStream(), true);

			Assert.AreEqual(26, result.Length);
		}

		[TestMethod]
		public async Task ExifTool_RenameThumbnailByStream_Fail()
		{
			var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist", };

			var fakeStorage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.jpg" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });

			var result =
				await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
					.RenameThumbnailByStream("OLDHASH", new MemoryStream(), false);

			Assert.AreEqual(0, result.Length);
		}

		[TestMethod]
		public async Task ExifTool_RenameThumbnailByStream_NotDisposed_CanWrite()
		{
			var appSettings = new AppSettings { ExifToolPath = "Z://Non-exist", };

			var fakeStorage = new FakeIStorage(new List<string> { "/" },
				new List<string> { "/test.jpg" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });

			var stream = new MemoryStream();
			await new ExifTool(fakeStorage, fakeStorage, appSettings, new FakeIWebLogger())
				.RenameThumbnailByStream("OLDHASH", stream, true);

			Assert.IsTrue(stream.CanWrite);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void StreamToStreamRunner_ArgumentNullException()
		{
			_ = new StreamToStreamRunner(new AppSettings(), null!,
				new FakeIWebLogger());
		}

		[TestMethod]
		public async Task RunProcessAsync_RunChildObject_UnixOnly()
		{
			// Unix only
			var appSettings = new AppSettings { Verbose = true, ExifToolPath = "/bin/ls" };
			if ( appSettings.IsWindows || !File.Exists("/bin/ls") )
			{
				Assert.Inconclusive("This test if for Unix Only");
				return;
			}

			var runner = new StreamToStreamRunner(appSettings,
				new MemoryStream(Array.Empty<byte>()), new FakeIWebLogger());
			var result = await runner.RunProcessAsync(string.Empty, "test / unit test");

			await StreamToStringHelper.StreamToStringAsync(result, false);

			Assert.AreEqual(0, result.Length);
		}

		[TestMethod]
		public async Task WriteTagsAndRenameThumbnailAsync_Disposed()
		{
			var storage =
				new FakeIStorage(new ObjectDisposedException("disposed"));

			var exifTool = new ExifTool(storage,
				storage,
				new AppSettings(), new FakeIWebLogger());

			var exceptionMessage = string.Empty;
			try
			{
				await exifTool.WriteTagsAndRenameThumbnailAsync("test.jpg", null, "");
			}
			catch ( ObjectDisposedException e )
			{
				// Expected
				exceptionMessage = e.Message;
			}

			Assert.IsTrue(exceptionMessage.StartsWith("Cannot access a disposed object."));
			Assert.IsTrue(exceptionMessage.EndsWith("Object name: 'disposed'."));

			Assert.AreEqual(1, storage.ExceptionCount);
		}
	}
}
