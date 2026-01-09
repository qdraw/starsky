using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyAdminCli.Models;
using starskyAdminCli.Services;

namespace starskytest.starskyAdminCli.Services;

[TestClass]
public class DropboxSetupTest
{
	[TestMethod]
	public void GetConfigSnippetTest()
	{
		var result = DropboxSetup.GetConfigSnippet("test", "secret",
			new DropboxTokenResponse { RefreshToken = "refresh-token" });

		Assert.AreEqual(
			"{\n  \"app\": {  \n    \"CloudImport\" :{\n      \"providers\": [\n        " +
			"{\n          \"id\": \"dropbox-import-example-id\",\n          " +
			"\"enabled\": true,\n          \"provider\": \"Dropbox\",\n          " +
			"\"remoteFolder\": \"/Camera Uploads\",\n          " +
			"\"syncFrequencyMinutes\": 0,\n          " +
			"\"syncFrequencyHours\": 0,\n          " +
			"\"deleteAfterImport\": false,\n          " +
			"\"extensions\": [],\n          " +
			"\"credentials\": {\n            " +
			"\"refreshToken\": \"refresh-token\",\n            " +
			"\"appKey\": \"test\",\n            " +
			"\"appSecret\": \"secret\"\n          " +
			"}\n        }\n      ]\n    }\n  } \n}",
			result);
	}
}
