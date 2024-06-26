using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.UpdateCheck.Models;
using starsky.feature.health.UpdateCheck.Services;
using starsky.foundation.http.Services;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.Services
{
	[TestClass]
	public sealed class CheckForUpdatesHelperTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public CheckForUpdatesHelperTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<ISelectorStorage, FakeSelectorStorage>();
			var serviceProvider = services.BuildServiceProvider();
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		private const string ExamplePublicReleases = "[\n  {\n   " +
		                                             " \"url\": \"https://api.github.com/repos/qdraw/starsky/releases/33322244\",\n " +
		                                             "   \"assets_url\": \"https://api.github.com/repos/qdraw/starsky/releases/33322244/assets\",\n" +
		                                             "    \"upload_url\": \"https://uploads.github.com/repos/qdraw/starsky/releases/33322244/" +
		                                             "assets{?name,label}\",\n    \"html_url\": \"https://github.com/qdraw/starsky/releases/" +
		                                             "tag/vtest__remove_this_version\",\n    \"id\": 33322244,\n    \"node_id\": \"MDc6UmVsZW" +
		                                             "FzZTMzMzIyMjQ0\",\n    \"tag_name\": \"vtest__remove_this_version\",\n    \"target_comm" +
		                                             "itish\": \"69bfc64264f1f2ea5e299e30bb6236c90c8c92de\",\n    \"name\": \"Release " +
		                                             "vtest__remove_this_version\",\n    \"draft\": false,\n    \"prerelease\": false,\n " +
		                                             "   \"created_at\": \"2020-11-01T14:40:08Z\",\n    \"published_at\": \"2020-11-01T14:57:43Z\",\n " +
		                                             "   \"tarball_url\": \"https://api.github.com/repos/qdraw/starsky/tarball/vtest__remove_this_version\",\n" +
		                                             "    \"zipball_url\": \"https://api.github.com/repos/qdraw/starsky/zipball/vtest__remove_this_version\"\n  }," +
		                                             "\n  {\n    \"url\": \"https://api.github.com/repos/qdraw/starsky/releases/33308345\",\n  " +
		                                             "  \"assets_url\": \"https://api.github.com/repos/qdraw/starsky/releases/33308345/assets\",\n  " +
		                                             "  \"upload_url\": \"https://uploads.github.com/repos/qdraw/starsky/releases/33308345/assets{?name,label}\",\n " +
		                                             "   \"html_url\": \"https://github.com/qdraw/starsky/releases/tag/v0.4.0-beta.1\",\n  " +
		                                             "  \"id\": 33308345,\n    \"node_id\": \"MDc6UmVsZWFzZTMzMzA4MzQ1\",\n   " +
		                                             " \"tag_name\": \"v0.4.0-beta.1\",\n    \"target_commitish\": " +
		                                             "\"0eaa9f3cb40c67750b80f367c7d01c545de54e3d\",\n   " +
		                                             " \"name\": \"Release v0.4.0-beta.1\",\n    \"draft\": false,\n   " +
		                                             " \"prerelease\": true,\n    \"created_at\": \"2020-10-31T17:49:19Z\",\n  " +
		                                             "  \"published_at\": \"2020-10-31T18:14:27Z\",\n    \"tarball_url\":" +
		                                             " \"https://api.github.com/repos/qdraw/starsky/tarball/v0.4.0-beta.1\",\n  " +
		                                             "  \"zipball_url\": \"https://api.github.com/repos/qdraw/starsky/zipball/v0.4.0-beta.1\"\n  }\n]\n";


		[TestMethod]
		public async Task QueryIsUpdateNeeded()
		{
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "v0.9");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ CheckForUpdates.GithubStarskyReleaseApi, new StringContent(replace) },
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var results = await new CheckForUpdates(httpClientHelper,
				new AppSettings(), null).QueryIsUpdateNeededAsync();

			Assert.AreEqual("v0.9", results?.FirstOrDefault()?.TagName);
			Assert.AreEqual("v0.4.0-beta.1", results?[1].TagName);
			Assert.IsFalse(results?[0].PreRelease);
			Assert.IsTrue(results?[1].PreRelease);

			Assert.AreEqual(2, results?.Count);
		}

		[TestMethod]
		public async Task QueryIsUpdateNeeded_Mirror()
		{
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "v0.9");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ CheckForUpdates.GithubStarskyReleaseMirrorApi, new StringContent(replace) },
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var results = await new CheckForUpdates(httpClientHelper,
				new AppSettings(), null).QueryIsUpdateNeededAsync();

			Assert.AreEqual("v0.9", results?.FirstOrDefault()?.TagName);
			Assert.AreEqual("v0.4.0-beta.1", results?[1].TagName);
			Assert.IsFalse(results?[0].PreRelease);
			Assert.IsTrue(results?[1].PreRelease);

			Assert.AreEqual(2, results?.Count);
		}

		[TestMethod]
		public async Task QueryIsUpdateNeeded_NotFound()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>());
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var results = await new CheckForUpdates(httpClientHelper,
				new AppSettings(), null).QueryIsUpdateNeededAsync();

			Assert.AreEqual(0, results?.Count);
		}

		[TestMethod]
		public void Parse_CurrentVersionIsNewer()
		{
			var (key, value) = CheckForUpdates.Parse(
				new List<ReleaseModel>
				{
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.1" }
				}, "0.2");

			Assert.AreEqual(UpdateStatus.CurrentVersionIsLatest, key);
			Assert.AreEqual("0.1.0", value);
		}

		[TestMethod]
		public void Parse_CurrentVersionIsNewer_Multiple()
		{
			var (key, value) = CheckForUpdates.Parse(
				new List<ReleaseModel>
				{
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.1.5" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.1" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.0.1" }
				}, "0.2");

			Assert.AreEqual(UpdateStatus.CurrentVersionIsLatest, key);
			Assert.AreEqual("0.1.5", value);
		}

		[TestMethod]
		public void Parse_CurrentVersionIsNewer_Multiple2()
		{
			var (key, value) = CheckForUpdates.Parse(
				new List<ReleaseModel>
				{
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.11" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.10" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.9" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.8" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.7" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.6" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.5" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.4" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.3" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.2" },
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.6.1" }
				}, "0.2");

			Assert.AreEqual(UpdateStatus.NeedToUpdate, key);
			Assert.AreEqual("0.6.11", value);
		}

		[TestMethod]
		public void Parse_CurrentVersionIsOlder()
		{
			var (key, value) = CheckForUpdates.Parse(
				new List<ReleaseModel>
				{
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v0.9" }
				}, "0.8");

			Assert.AreEqual(UpdateStatus.NeedToUpdate, key);
			Assert.AreEqual("0.9.0", value);
		}

		[TestMethod]
		public void Parse_wrongTagName()
		{
			var (key, _) = CheckForUpdates.Parse(
				new List<ReleaseModel>
				{
					new ReleaseModel { Draft = false, PreRelease = false, TagName = "v_test" }
				}, "0.8");

			Assert.AreEqual(UpdateStatus.InputNotValid, key);
		}

		[TestMethod]
		public void Parse_wrongTagName_ButDidntStartWithV()
		{
			var (key, _) = CheckForUpdates.Parse(
				new List<ReleaseModel>
				{
					new ReleaseModel
					{
						Draft = false, PreRelease = false, TagName = "nothing_here"
					}
				}, "0.8");

			Assert.AreEqual(UpdateStatus.NoReleasesFound, key);
		}

		[TestMethod]
		public async Task IsUpdateNeeded_CheckForUpdates_disabled()
		{
			var appSettings = new AppSettings { CheckForUpdates = false };
			var (key, _) = await new CheckForUpdates(null!,
				appSettings, null).IsUpdateNeeded();

			Assert.AreEqual(UpdateStatus.Disabled, key);
		}

		[TestMethod]
		public async Task IsUpdateNeeded_CacheIsFilled()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "v0.9");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ CheckForUpdates.GithubStarskyReleaseApi, new StringContent(replace) },
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			await new CheckForUpdates(httpClientHelper,
				new AppSettings(), memoryCache).IsUpdateNeeded();

			Assert.IsNotNull(memoryCache);

			memoryCache.TryGetValue(CheckForUpdates.QueryCheckForUpdatesCacheName,
				out var cacheResult);
			var results = ( ( List<ReleaseModel>? )cacheResult );

			Assert.IsNotNull(results);
			Assert.AreEqual("v0.9", results.FirstOrDefault()?.TagName);
			Assert.AreEqual("v0.4.0-beta.1", results[1].TagName);
			Assert.IsFalse(results[0].PreRelease);
			Assert.IsTrue(results[1].PreRelease);
		}

		[TestMethod]
		public async Task IsUpdateNeeded_GetFromCache()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			Assert.IsNotNull(memoryCache);

			memoryCache.Set(CheckForUpdates.QueryCheckForUpdatesCacheName, new List<ReleaseModel>
			{
				new ReleaseModel
				{
					TagName = "v0.4.0", // should start with 'v'
					PreRelease = false,
					Draft = false
				}
			});

			var (key, value) = await new CheckForUpdates(null!,
				new AppSettings(), memoryCache).IsUpdateNeeded("0.4.0");

			Assert.IsNotNull(value);
			Assert.AreEqual(UpdateStatus.CurrentVersionIsLatest, key);
		}

		[TestMethod]
		public async Task IsUpdateNeeded_Disabled2_CacheIsNull()
		{
			var (key, value) = await new CheckForUpdates(null!,
				null, null).IsUpdateNeeded();

			Assert.IsNotNull(value);
			Assert.AreEqual(UpdateStatus.Disabled, key);
		}

		[TestMethod]
		public async Task IsUpdateNeeded_CacheExistButDisableInSettings()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "test");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ CheckForUpdates.GithubStarskyReleaseApi, new StringContent(replace) },
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var (key, value) = await new CheckForUpdates(httpClientHelper,
				new AppSettings { AddMemoryCache = false }, memoryCache).IsUpdateNeeded();

			Assert.IsNotNull(value);
			Assert.AreEqual(UpdateStatus.NoReleasesFound, key);
		}

		[TestMethod]
		public async Task IsUpdateNeeded_CacheIsNull()
		{
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "test");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{ CheckForUpdates.GithubStarskyReleaseApi, new StringContent(replace) },
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory,
				new FakeIWebLogger());

			var (key, value) = await new CheckForUpdates(httpClientHelper,
				new AppSettings { AddMemoryCache = true }, null).IsUpdateNeeded();

			Assert.IsNotNull(value);
			Assert.AreEqual(UpdateStatus.NoReleasesFound, key);
		}

		[TestMethod]
		public void Parse_WithNullReleaseModelList_ReturnsNoReleasesFound()
		{
			// Arrange
			const string currentVersion = "1.0.0"; // Provide a valid version

			// Act
			var (key, value) = CheckForUpdates.Parse(null, currentVersion);

			// Assert
			Assert.AreEqual(UpdateStatus.NoReleasesFound, key);
			Assert.AreEqual(string.Empty, value);
		}

		[TestMethod]
		public void Parse_WithNullOrderedReleaseModelList_ReturnsNoReleasesFound()
		{
			// Arrange
			IEnumerable<ReleaseModel> releaseModelList = new List<ReleaseModel>(); // An empty list
			const string currentVersion = "1.0.0"; // Provide a valid version

			// Act
			var (key, value) = CheckForUpdates.Parse(releaseModelList, currentVersion);

			// Assert
			Assert.AreEqual(UpdateStatus.NoReleasesFound, key);
			Assert.AreEqual(string.Empty, value);
		}

		[TestMethod]
		public void Parse_WithNullFirstReleaseModel_ReturnsNoReleasesFound()
		{
			// Arrange
			IEnumerable<ReleaseModel> releaseModelList = new List<ReleaseModel>
			{
				// List with no valid releases (Draft and PreRelease)
			};
			const string currentVersion = "1.0.0"; // Provide a valid version

			// Act
			var (key, value) = CheckForUpdates.Parse(releaseModelList, currentVersion);

			// Assert
			Assert.AreEqual(UpdateStatus.NoReleasesFound, key);
			Assert.AreEqual(string.Empty, value);
		}


		[TestMethod]
		public void Parse_WithNullFirstReleaseModel_Returns_EmptyReleaseModel()
		{
			// Arrange
			IEnumerable<ReleaseModel> releaseModelList = new List<ReleaseModel>
			{
				new ReleaseModel { TagName = null! }
			};
			const string currentVersion = "1.0.0"; // Provide a valid version

			// Act
			var (key, value) = CheckForUpdates.Parse(releaseModelList, currentVersion);

			// Assert
			Assert.AreEqual(UpdateStatus.NoReleasesFound, key);
			Assert.AreEqual(string.Empty, value);
		}

		[TestMethod]
		public async Task CheckForUpdatesHelperTestSample()
		{
			var dir = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			Assert.IsNotNull(dir);
			var path = Path.Combine(dir, "starsky.feature.health", "ExampleData",
				"CheckForUpdatesHelperTestSample.json");

			var readStream =
				new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(path);
			var json = await StreamToStringHelper.StreamToStringAsync(readStream);
			var model = JsonSerializer.Deserialize<List<ReleaseModel>>(json,
				DefaultJsonSerializer.CamelCase);

			var (key, value) = CheckForUpdates.Parse(model, "0.5.13");

			Assert.AreEqual(UpdateStatus.NeedToUpdate, key);
			Assert.AreEqual("0.5.14", value);
		}
	}
}
