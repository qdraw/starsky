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
			"{\n    \"v0.6.0\" : {\n        \"en\": \"Content\"\n    },\n    \"v0.6.0-beta.0\" : {\n        \"en\": \"Content\"\n    }\n}";

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
}
