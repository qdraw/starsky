using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.health.UpdateCheck.Models;
using starsky.feature.health.UpdateCheck.Services;
using starsky.foundation.http.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.health.Helpers
{
	[TestClass]
	public class CheckForUpdatesHelperTest
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
		public async Task QueryIsUpdateNeeded_newer()
		{
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "v0.9");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{CheckForUpdates.GithubApi, new StringContent(replace)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			// current is 1.0  - new is 0.9
			var results = await new CheckForUpdates(httpClientHelper, 
				new AppSettings(),null).QueryIsUpdateNeeded("1.0");
			
			Assert.AreEqual(UpdateStatus.CurrentVersionIsLatest,results.Key);
		}
		
		[TestMethod]
		public async Task QueryIsUpdateNeeded_eq()
		{
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "v0.9");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{CheckForUpdates.GithubApi, new StringContent(replace)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);


			// current is 0.9 - new is 0.9
			var results = await new CheckForUpdates(httpClientHelper, 
				new AppSettings(),null).QueryIsUpdateNeeded("0.9");
			
			Assert.AreEqual(UpdateStatus.CurrentVersionIsLatest,results.Key);
		}
		
		[TestMethod]
		public async Task QueryIsUpdateNeeded_lower()
		{
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "v0.9");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{CheckForUpdates.GithubApi, new StringContent(replace)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);


			// current is 0.8 - new is 0.9
			var results = await new CheckForUpdates(httpClientHelper, 
				new AppSettings(),null).QueryIsUpdateNeeded("0.8");
			
			Assert.AreEqual(UpdateStatus.NeedToUpdate,results.Key);
		}
		
		[TestMethod]
		public async Task QueryIsUpdateNeeded_wrongTagName()
		{
			var replace = ExamplePublicReleases.Replace("vtest__remove_this_version", "test");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{CheckForUpdates.GithubApi, new StringContent(replace)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			// current is 0.9 - new is 0.9
			var results = await new CheckForUpdates(httpClientHelper,
				new AppSettings(),null).QueryIsUpdateNeeded("0.9");
			
			Assert.AreEqual(UpdateStatus.NoReleasesFound,results.Key);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task QueryIsUpdateNeeded_CurrentVersionIsNull()
		{
			await new CheckForUpdates(null,null,null).QueryIsUpdateNeeded(null);
			// expect ArgumentNullException
		}
		
		[TestMethod]
		public async Task QueryIsUpdateNeeded_OnlyPreReleases()
		{
			var replace = ExamplePublicReleases.Replace("\"prerelease\": false", "\"prerelease\": true");
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{CheckForUpdates.GithubApi, new StringContent(replace)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			var results = await new CheckForUpdates(httpClientHelper, 
				new AppSettings(),null).QueryIsUpdateNeeded("0.9");
			
			Assert.AreEqual(UpdateStatus.NoReleasesFound,results.Key);
		}
		
		[TestMethod]
		public async Task QueryIsUpdateNeeded_EmptyList()
		{
			var fakeIHttpProvider = new FakeIHttpProvider(new Dictionary<string, HttpContent>
			{
				{CheckForUpdates.GithubApi, new StringContent("[]")},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			var results = await new CheckForUpdates(httpClientHelper, 
				new AppSettings(),null).QueryIsUpdateNeeded("0.9");
			
			Assert.AreEqual(UpdateStatus.NoReleasesFound,results.Key);
		}

		[TestMethod]
		public async Task IsUpdateNeeded_CheckForUpdates_disabled()
		{
			var appSettings = new AppSettings {CheckForUpdates = false};
			var results = await new CheckForUpdates(null, 
				appSettings,null).IsUpdateNeeded();
			
			Assert.AreEqual(UpdateStatus.Disabled,results.Key);
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
				{CheckForUpdates.GithubApi, new StringContent(replace)},
			});
			var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);

			await new CheckForUpdates(httpClientHelper, 
				new AppSettings(),memoryCache).IsUpdateNeeded();

			memoryCache.TryGetValue(CheckForUpdates.QueryCacheName, out var cacheResult);
			var status = (( KeyValuePair<UpdateStatus, string> ) cacheResult).Value;

			Assert.IsNotNull(status);
			Assert.AreEqual("0.9",status);
		}
		
		[TestMethod]
		public async Task IsUpdateNeeded_GetFromCache()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			memoryCache.Set(CheckForUpdates.QueryCacheName, new KeyValuePair<UpdateStatus, 
				string>(UpdateStatus.NeedToUpdate,"0.9"));
			
			var status = await new CheckForUpdates(null, 
				new AppSettings(),memoryCache).IsUpdateNeeded();

			Assert.IsNotNull(status);
			Assert.AreEqual("0.9",status.Value);
		}
		
		[TestMethod]
        public async Task IsUpdateNeeded_Disabled2()
        {
	        var results = await new CheckForUpdates(null, 
		        null,null).IsUpdateNeeded();
	  
	        Assert.IsNotNull(results);
	        Assert.AreEqual(UpdateStatus.Disabled,results.Key);
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
		        {CheckForUpdates.GithubApi, new StringContent(replace)},
	        });
	        var httpClientHelper = new HttpClientHelper(fakeIHttpProvider, _serviceScopeFactory);
	        
	        var results = await new CheckForUpdates(httpClientHelper, 
		        new AppSettings{AddMemoryCache = false},memoryCache).IsUpdateNeeded();
	  
	        Assert.IsNotNull(results);
	        Assert.AreEqual(UpdateStatus.NoReleasesFound,results.Key);
        }
	}
}
