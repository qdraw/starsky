using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.UpdateCheck.Services;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.Services;

[TestClass]
public class SpecificVersionReleaseInfoTests
{
	[TestMethod]
	public async Task SpecificVersionMessage_NoCache()
	{
		// Arrange
		var serviceProvider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();

		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";

		var memoryCache = serviceProvider.GetService<IMemoryCache>();

		var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			// Mocking the response from the fake HTTP provider
			{
				"https://" + SpecificVersionReleaseInfo.SpecificVersionReleaseInfoUrl,
				new StringContent(example)
			}
		});

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());

		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper, null, memoryCache,
				new FakeIWebLogger());

		// Act
		var result = await specificVersionReleaseInfo.SpecificVersionMessage("0.6.0");

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual("Content", result);
	}

	[TestMethod]
	public async Task SpecificVersionMessage_Cache()
	{
		// Arrange
		var serviceProvider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();

		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";

		var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
		memoryCache.Set(SpecificVersionReleaseInfo.GetSpecificVersionReleaseInfoCacheName, example);

		var fakeIHttpProvider = new FakeIHttpProvider(); // return not found if not in cache

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());

		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, memoryCache, new FakeIWebLogger());

		// Act
		var result = await specificVersionReleaseInfo.SpecificVersionMessage("0.6.0");

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual("Content", result);
	}

	[TestMethod]
	public async Task SpecificVersionMessage_CacheEnabledGetValue()
	{
		// Arrange
		var serviceProvider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();

		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";

		var memoryCache = serviceProvider.GetService<IMemoryCache>();

		var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			// Mocking the response from the fake HTTP provider
			{
				"https://" + SpecificVersionReleaseInfo.SpecificVersionReleaseInfoUrl,
				new StringContent(example)
			}
		});

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());

		var specificVersionReleaseInfo = // memory cache enabled
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, memoryCache,
				new FakeIWebLogger());

		// Act
		var result = await specificVersionReleaseInfo.SpecificVersionMessage("0.6.0");

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual("Content", result);
	}

	[TestMethod]
	public async Task SpecificVersionMessage_CacheEnabledGetValue_SetInCache()
	{
		// Arrange
		var serviceProvider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();

		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";

		var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

		var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			// Mocking the response from the fake HTTP provider
			{
				"https://" + SpecificVersionReleaseInfo.SpecificVersionReleaseInfoUrl,
				new StringContent(example)
			}
		});

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());

		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, memoryCache,
				new FakeIWebLogger());

		// Act
		await specificVersionReleaseInfo.SpecificVersionMessage("v0.6.0");

		// Assert
		var result =
			memoryCache.Get<string>(SpecificVersionReleaseInfo
				.GetSpecificVersionReleaseInfoCacheName);

		Assert.IsNotNull(result);
		Assert.AreEqual(example, result);
	}

	[TestMethod]
	public async Task SpecificVersionMessage_NullString()
	{
		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(
				new FakeIHttpClientHelper(new FakeIStorage(),
					new Dictionary<string, KeyValuePair<bool, string>>()),
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		// Act
		var result = await specificVersionReleaseInfo.SpecificVersionMessage("null");
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public async Task SpecificVersionMessage_NullValue()
	{
		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(
				new FakeIHttpClientHelper(new FakeIStorage(),
					new Dictionary<string, KeyValuePair<bool, string>>()),
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		// Act
		var result = await specificVersionReleaseInfo.SpecificVersionMessage(null);
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ParseTest_String_Empty()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());

		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		var result = specificVersionReleaseInfo.Parse(string.Empty, "0.6.0");
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ParseTest_VersionNotFound()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";


		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		var result = specificVersionReleaseInfo.Parse(example, "0.1.0");
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ParseTest_VersionFound()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";


		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		var result = specificVersionReleaseInfo.Parse(example, "0.6.0-beta.0");
		Assert.AreEqual("Content", result);
	}

	[TestMethod]
	public void ParseTest_InvalidJson()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		const string example =
			"{\n    \"0.6.0\" : {\n        --";


		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		var result = specificVersionReleaseInfo.Parse(example, "0.6.0-beta.0");
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ParseTest_LanguageKeyNotFound()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		const string example =
			"{\n    \"0.6.0\" : {\n  }\n}";

		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		var result = specificVersionReleaseInfo.Parse(example, "0.6.0");
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ParseTest_VersionFound_HtmlLink()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"[Link text Here](https://link-url-here.org)\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";


		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, null,
				new FakeIWebLogger());

		var result = specificVersionReleaseInfo.Parse(example, "0.6.0");
		Assert.AreEqual(
			"<a target=\"_blank\" ref=\"nofollow\" href=\"https://link-url-here.org\">Link text Here</a>",
			result);
	}

	[TestMethod]
	public void ConvertMarkdownLinkToHtml_Test()
	{
		var markdownLinkToHtml = SpecificVersionReleaseInfo.ConvertMarkdownLinkToHtml(
			"Content [Link text Here](https://link-url-here.org) " +
			"[link1](https://example.com)");

		Assert.AreEqual(
			"Content <a target=\"_blank\" ref=\"nofollow\" href=\"https://link-url-here.org\">Link text Here</a> " +
			"<a target=\"_blank\" ref=\"nofollow\" href=\"https://example.com\">link1</a>",
			markdownLinkToHtml);
	}

	[TestMethod]
	public async Task QuerySpecificVersionInfo_GetResult()
	{
		const string example =
			"{\n    \"0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";

		var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
		{
			// Mocking the response from the fake HTTP provider
			{
				"https://" + SpecificVersionReleaseInfo.SpecificVersionReleaseInfoUrl,
				new StringContent(example)
			}
		});

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper, null, null,
				new FakeIWebLogger());

		var result = await specificVersionReleaseInfo.QuerySpecificVersionInfo();

		Assert.AreEqual(example, result);
	}

	[TestMethod]
	public async Task QuerySpecificVersionInfo_NotFound()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper, null, null,
				new FakeIWebLogger());

		var result = await specificVersionReleaseInfo.QuerySpecificVersionInfo();

		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void Parse_NullVersion()
	{
		var fakeIHttpProvider = new FakeIHttpProvider();

		var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, null, new FakeIWebLogger());
		var specificVersionReleaseInfo =
			new SpecificVersionReleaseInfo(httpClientHelper, null, null,
				new FakeIWebLogger());

		var result = specificVersionReleaseInfo.Parse("-", null);
		Assert.AreEqual(string.Empty, result);
	}
}
