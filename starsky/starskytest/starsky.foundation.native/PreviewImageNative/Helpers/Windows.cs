using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.native.PreviewImageNative.Helpers
{
	[TestClass]
	public class ShellThumbnailExtractionWindowsTest
	{
		[TestMethod]
		public void Test()
		{
			ShellThumbnailExtractionWindows.GenerateThumbnail(
				"C:\\testcontent\\20221029_101722_DSC05623.jpg", "C:\\testcontent\\test.jpg", 150, 150);
		}
	}
}
