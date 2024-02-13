using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starsky.foundation.storage.Services;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.native.OpenApplicationNative.Helpers
{
	[TestClass]
	public class WindowsSetFileAssociationsTests
	{
		[TestMethod]
		public void EnsureAssociationsSet()
		{
			var filePath = new CreateFakeStarskyExe().FullFilePath;
			WindowsSetFileAssociations.EnsureAssociationsSet(
				new FileAssociation
				{
					Extension = ".starsky",
					ProgId = "starskytest",
					FileTypeDescription = "Starsky Test File",
					ExecutableFilePath = filePath
				});
		}
	}
}
