using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers
{
	[TestClass]
	public sealed class CopyPublishedContentTest
	{
		[TestMethod]
		public void CopyContent_Test()
		{
			var contentPath = CopyPublishedContent.GetContentFolder();

			var fakeStorage = new FakeIStorage(new List<string> { contentPath },
				new List<string> { Path.Combine(contentPath, "copy.jsx") },
				new List<byte[]> { Array.Empty<byte>() });

			var service = new CopyPublishedContent(new ToCreateSubfolder(fakeStorage),
				new FakeSelectorStorage(fakeStorage));

			var content = service.CopyContent(
				new AppSettingsPublishProfiles(), "/");

			Assert.IsTrue(content["copy.jsx"]);
		}

		[TestMethod]
		public void CopyContent_Null()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new CopyPublishedContent(null!, null!));
		}
	}
}
