using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.ViewModels;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.feature.webhtmlpublish.ViewModels;

[TestClass]
public class WebHtmlViewModelTest
{
	[TestMethod]
	public void WebHtmlViewModel1()
	{
		var model = new WebHtmlViewModel
		{
			AppSettings = new AppSettings(),
			CurrentProfile = new AppSettingsPublishProfiles(),
			Profiles = new List<AppSettingsPublishProfiles>(),
			Base64ImageArray = Array.Empty<string>(),
			FileIndexItems=	new List<FileIndexItem>()
		};
		
		Assert.IsNotNull(model.AppSettings);
		Assert.IsNotNull(model.CurrentProfile);
		Assert.IsNotNull(model.Profiles);
		Assert.IsNotNull(model.Base64ImageArray);
		Assert.IsNotNull(model.FileIndexItems);
	}
}
