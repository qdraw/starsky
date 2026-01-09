using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;
using starskyAdminCli.Models;
using starskyAdminCli.Services;
using starskytest.FakeMocks;

namespace starskytest.starskyAdminCli.Services;

[TestClass]
public class DropboxSetupTest
{
	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Setup_ShouldRunWithoutErrors()
	{
		var console = new FakeConsoleWrapper([
			"test-app-key", // Dropbox App Key
			"test-app-secret", // Dropbox App Secret
			"test-access-code", // Access code
			""
		]);

		var httpClientHelper = new FakeIHttpClientHelper(
			new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://api.dropbox.com/oauth2/token",
					new KeyValuePair<bool, string>(true, "{\"refresh_token\":\"refresh-token\"}")
				}
			}
		);

		var dropboxSetup = new DropboxSetup(console, httpClientHelper);
		var result = await dropboxSetup.Setup().TimeoutAfter(5000);

		// Assert that the console output contains expected prompts and config
		Assert.IsTrue(result);
		Assert.IsTrue(console.WrittenLines.Exists(x => x.Contains("Dropbox Setup:")));
		Assert.IsTrue(console.WrittenLines.Exists(x =>
			x.Contains("Go to: https://www.dropbox.com/developers/apps/create")));
		Assert.IsTrue(console.WrittenLines.Exists(x =>
			x.Contains("Merge this with an existing appsettings.json:")));
		Assert.IsTrue(console.WrittenLines.Exists(x => x.Contains("refresh-token")));
	}

	[TestMethod]
	public void GetConfigSnippetTest()
	{
		var result = DropboxSetup.GetConfigSnippet("test", "secret",
			new DropboxTokenResponse { RefreshToken = "refresh-token" });

		var expected =
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
			"}\n        }\n      ]\n    }\n  } \n}";

		if ( new AppSettings().IsWindows )
		{
			expected = expected.Replace("\n", "\r\n");
			result = result.Replace("\n", "\r\n");
		}

		Assert.AreEqual(expected, result);
	}
}
