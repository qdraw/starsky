using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.geolookup.Services;
using starskytest.FakeMocks;
using VerifyMSTest;

namespace starskytest.starsky.feature.geolookup.Services;

[TestClass]
public class NominatimProxyServiceTest : VerifyBase
{
	[TestMethod]
	public async Task ReverseAsync_ReturnsExpectedJson_Verify()
	{
		const string expectedJson = "{\"place_id\":143283526}";
		var fakeHttp = new FakeIHttpClientHelper(null!,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://nominatim.openstreetmap.org/reverse?format=json&" +
					"lat=52.38230014307398&lon=4.941273642639544&addressdetails=1",
					new KeyValuePair<bool, string>(true, expectedJson)
				}
			});
		var service = new NominatimProxyService(fakeHttp);
		var result = await service.ReverseAsync(52.38230014307398, 4.941273642639544);
		Assert.Contains("lat=52.38230014307398", fakeHttp.UrlsCalled[0]);
		Assert.Contains("lon=4.941273642639544", fakeHttp.UrlsCalled[0]);

		await Verify(result);
	}

	[TestMethod]
	public async Task ReverseAsync_ReturnsExpectedJson_Amsterdam_Verify()
	{
		const string expectedJson = "{\"place_id\":143283526," +
		                            "\"licence\":\"Data © OpenStreetMap contributors, ODbL 1.0." +
		                            " https://osm.org/copyright\",\"osm_type\":\"way\"," +
		                            "\"osm_id\":1262616283,\"lat\":\"52.3857326\"," +
		                            "\"lon\":\"4.9368839\",\"class\":\"man_made\"," +
		                            "\"type\":\"works\",\"place_rank\":30," +
		                            "\"importance\":8.268177309829888e-05," +
		                            "\"addresstype\":\"man_made\",\"name\":" +
		                            "\"Ketjen\",\"display_name\":" +
		                            "\"Ketjen, Nieuwendammerkade, Noord, Amsterdam, " +
		                            "Noord-Holland, Nederland, 1022 AB, Nederland\"," +
		                            "\"address\":{\"man_made\":\"Ketjen\",\"road\":" +
		                            "\"Nieuwendammerkade\",\"suburb\":\"Noord\"," +
		                            "\"city\":\"Amsterdam\",\"municipality\":" +
		                            "\"Amsterdam\",\"state\":\"Noord-Holland\"," +
		                            "\"ISO3166-2-lvl4\":\"NL-NH\",\"country\":" +
		                            "\"Nederland\",\"postcode\":\"1022 AB\"," +
		                            "\"country_code\":\"nl\"}," +
		                            "\"boundingbox\":[\"52.3839244\",\"52.3872288\"," +
		                            "\"4.9313591\",\"4.9433754\"]}";

		var fakeHttp = new FakeIHttpClientHelper(null!,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://nominatim.openstreetmap.org/reverse?format=json&" +
					"lat=52.38230014307398&lon=4.941273642639544&addressdetails=1",
					new KeyValuePair<bool, string>(true, expectedJson)
				}
			});
		var service = new NominatimProxyService(fakeHttp);
		var result = await service.ReverseAsync(52.38230014307398, 4.941273642639544);
		Assert.Contains("lat=52.38230014307398", fakeHttp.UrlsCalled[0]);
		Assert.Contains("lon=4.941273642639544", fakeHttp.UrlsCalled[0]);

		await Verify(result);
	}

	[TestMethod]
	public async Task ReverseAsync_InvalidRequest_Verify()
	{
		const string expectedJson = "{\"place_id\":143283526}";
		var fakeHttp = new FakeIHttpClientHelper(null!,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://nominatim.openstreetmap.org/reverse?format=json&" +
					"lat=52.38230014307398&lon=4.941273642639544&addressdetails=1",
					new KeyValuePair<bool, string>(false, expectedJson)
				}
			});
		var service = new NominatimProxyService(fakeHttp);
		var result = await service.ReverseAsync(52.38230014307398, 4.941273642639544);
		Assert.Contains("lat=52.38230014307398", fakeHttp.UrlsCalled[0]);
		Assert.Contains("lon=4.941273642639544", fakeHttp.UrlsCalled[0]);

		await Verify(result);
	}

	[TestMethod]
	public async Task ReverseAsync_InvalidLocation_Verify()
	{
		var service = new NominatimProxyService(new FakeIHttpClientHelper(null!,
			new Dictionary<string, KeyValuePair<bool, string>>()));
		var result = await service.ReverseAsync(523898, 49414);
		await Verify(result);
	}

	[TestMethod]
	public async Task ReverseAsync_ReturnsInvalidJson_Verify()
	{
		const string expectedJson = "{\"invalid_json___";
		var fakeHttp = new FakeIHttpClientHelper(null!,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://nominatim.openstreetmap.org/reverse?format=json&" +
					"lat=52.38230014307398&lon=4.941273642639544&addressdetails=1",
					new KeyValuePair<bool, string>(true, expectedJson)
				}
			});
		var service = new NominatimProxyService(fakeHttp);
		var result = await service.ReverseAsync(52.38230014307398, 4.941273642639544);
		Assert.Contains("lat=52.38230014307398", fakeHttp.UrlsCalled[0]);
		Assert.Contains("lon=4.941273642639544", fakeHttp.UrlsCalled[0]);

		await Verify(result);
	}
}
