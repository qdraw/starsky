using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.ViewModels;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.feature.webhtmlpublish.ViewModels;

[TestClass]
public sealed class WebHtmlViewModelTest
{
	[TestMethod]
	public void WebHtmlViewModel1()
	{
		var model = new WebHtmlViewModel
		{
			ItemName = "test",
			AppSettings = new AppSettings(),
			CurrentProfile = new AppSettingsPublishProfiles(),
			Profiles = [],
			Base64ImageArray = [],
			FileIndexItems = []
		};

		Assert.AreEqual("test", model.ItemName);
		Assert.AreEqual("Starsky", model.AppSettings.Name);
		Assert.AreEqual(TemplateContentType.None, model.CurrentProfile.ContentType);
		Assert.AreEqual(0, model.Profiles.Count);
		Assert.AreEqual(0, model.Base64ImageArray.Length);
		Assert.AreEqual(0, model.FileIndexItems.Count);
	}
}
