using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public sealed class OverlayImageFactoryTest
{
	private static OverlayImageFactory Factory()
	{
		return new OverlayImageFactory(new FakeSelectorStorage(new FakeIStorage()),
			new AppSettings());
	}

	[TestMethod]
	public void Create_IsSkia()
	{
		Assert.IsInstanceOfType<OverlayImageSkia>(Factory().Create());
	}

	[TestMethod]
	public void OverlayImage_UsesFactory()
	{
		var overlayImage = new OverlayImage(Factory());
		var path = overlayImage.FilePathOverlayImage("TesT.Jpg",
			new AppSettingsPublishProfiles { Append = "_a" });
		Assert.AreEqual("test_a.jpg", path);
	}
}
