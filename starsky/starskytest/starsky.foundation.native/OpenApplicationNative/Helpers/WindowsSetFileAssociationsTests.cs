using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.OpenApplicationNative.Helpers;
using starskytest.FakeCreateAn.CreateFakeStarskyExe;

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
