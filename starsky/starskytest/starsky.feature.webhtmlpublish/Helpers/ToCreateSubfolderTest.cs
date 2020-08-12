using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Helpers
{
	[TestClass]
	public class ToCreateSubfolderTest
	{
		[TestMethod]
		public void Create()
		{
			var iStorage = new FakeIStorage();
			new ToCreateSubfolder(iStorage).Create(new AppSettingsPublishProfiles{Folder = "test"},"/parent" );
			
			Assert.IsTrue(iStorage.ExistFolder("/parent/test/"));
		}
	}
}
