using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.http.Services;

namespace starskytest.starsky.foundation.http.Services;

[TestClass]
public sealed class CurlPostHelperTest
{
	[TestMethod]
	public async Task PostAsync_NotHttps_ReturnsNull()
	{
		var result = await new CurlPostHelper().PostAsync("http://overpass-api.de", "data=test",
			"text/plain", null);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task PostAsync_CurlNotInstalled_ReturnsNull()
	{
		var helper = new CurlPostHelper("starsky-curl-does-not-exist");

		var result = await helper.PostAsync("https://overpass-api.de", "data=test",
			"text/plain", null);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void BuildArguments_BodyGoesThroughStdinAndUrlIsLast()
	{
		var arguments = CurlPostHelper.BuildArguments(
			"https://overpass-api.de/api/interpreter?x=--insecure",
			"application/x-www-form-urlencoded", "Starsky-App");

		var bodyIndex = arguments.IndexOf("--data-binary");
		Assert.AreEqual("@-", arguments[bodyIndex + 1]);
		Assert.AreEqual("Starsky-App", arguments[arguments.IndexOf("--user-agent") + 1]);
		Assert.AreEqual("Content-Type: application/x-www-form-urlencoded",
			arguments[arguments.IndexOf("--header") + 1]);
		// the url is the value of --url, so it can never be read as a switch
		Assert.AreEqual("--url", arguments[^2]);
		Assert.AreEqual("https://overpass-api.de/api/interpreter?x=--insecure", arguments[^1]);
		Assert.IsFalse(arguments.Contains("--insecure"));
	}

	[TestMethod]
	public void BuildArguments_WithoutUserAgent_NoUserAgentSwitch()
	{
		var arguments = CurlPostHelper.BuildArguments("https://qdraw.nl", "text/plain", null);

		Assert.IsFalse(arguments.Contains("--user-agent"));
	}

	[TestMethod]
	public async Task ParseOutput_BodyAndStatusCode()
	{
		var result = CurlPostHelper.ParseOutput("{\"elements\":[]}\n200");

		Assert.IsNotNull(result);
		Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
		Assert.AreEqual("{\"elements\":[]}", await result.Content.ReadAsStringAsync());
	}

	[TestMethod]
	public async Task ParseOutput_MultiLineBody_KeepsAllLines()
	{
		var result = CurlPostHelper.ParseOutput("{\n \"elements\": []\n}\n406");

		Assert.IsNotNull(result);
		Assert.AreEqual(HttpStatusCode.NotAcceptable, result.StatusCode);
		Assert.AreEqual("{\n \"elements\": []\n}", await result.Content.ReadAsStringAsync());
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("no status")]
	[DataRow("\n000")]
	[DataRow("body\nnot-a-number")]
	public void ParseOutput_NoConnection_ReturnsNull(string output)
	{
		Assert.IsNull(CurlPostHelper.ParseOutput(output));
	}
}
