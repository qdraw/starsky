using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport.Clients;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.cloudimport.Clients;

[TestClass]
public class DropboxCloudImportRefreshTokenTest
{
	private const string TokenUrl = "https://api.dropbox.com/oauth2/token";

	[TestMethod]
	public async Task ExchangeRefreshTokenAsync_ReturnsAccessTokenAndExpiry()
	{
		var dict = new Dictionary<string, KeyValuePair<bool, string>>
		{
			{
				TokenUrl, new KeyValuePair<bool, string>(true,
					"{\"access_token\":\"abc123\",\"expires_in\":3600}")
			}
		};
		var fakeHttp = new FakeIHttpClientHelper(new FakeIStorage(), dict);
		var service = new DropboxCloudImportRefreshToken(fakeHttp);
		var (token, expires) = await service.ExchangeRefreshTokenAsync("refresh", "key", "secret");
		Assert.AreEqual("abc123", token);
		Assert.AreEqual(3600, expires);
	}

	[TestMethod]
	public async Task ExchangeRefreshTokenAsync_UsesDefaultExpiryIfMissing()
	{
		var dict = new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ TokenUrl, new KeyValuePair<bool, string>(true, "{\"access_token\":\"abc123\"}") }
		};
		var fakeHttp = new FakeIHttpClientHelper(new FakeIStorage(), dict);
		var service = new DropboxCloudImportRefreshToken(fakeHttp);
		var (token, expires) = await service.ExchangeRefreshTokenAsync("refresh", "key", "secret");
		Assert.AreEqual("abc123", token);
		Assert.AreEqual(14400, expires);
	}

	[TestMethod]
	public async Task ExchangeRefreshTokenAsync_ThrowsOnHttpFailure()
	{
		var dict = new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ TokenUrl, new KeyValuePair<bool, string>(false, "error") }
		};
		var fakeHttp = new FakeIHttpClientHelper(new FakeIStorage(), dict);
		var service = new DropboxCloudImportRefreshToken(fakeHttp);
		await Assert.ThrowsExactlyAsync<HttpRequestException>(async () =>
		{
			await service.ExchangeRefreshTokenAsync("refresh", "key", "secret");
		});
	}

	[TestMethod]
	public async Task ExchangeRefreshTokenAsync_ThrowsOnMissingAccessToken()
	{
		var dict = new Dictionary<string, KeyValuePair<bool, string>>
		{
			{ TokenUrl, new KeyValuePair<bool, string>(true, "{\"expires_in\":3600}") }
		};
		var fakeHttp = new FakeIHttpClientHelper(new FakeIStorage(), dict);
		var service = new DropboxCloudImportRefreshToken(fakeHttp);
		await Assert.ThrowsExactlyAsync<HttpRequestException>(async () =>
		{
			await service.ExchangeRefreshTokenAsync("refresh", "key", "secret");
		});
	}
}
