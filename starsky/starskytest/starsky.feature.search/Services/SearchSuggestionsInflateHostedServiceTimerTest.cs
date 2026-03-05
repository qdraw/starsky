using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.search.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.search.Services;

[TestClass]
public class SearchSuggestionsInflateHostedServiceTimerTest : DatabaseTest
{
	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Timer_Should_Trigger_At_Least_Twice()
	{
		await RetryHelper.DoAsync(Test, TimeSpan.FromSeconds(2));
		return;

		async Task<bool> Test()
		{
			var scope = scopeFactory.CreateScope();
			var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			for ( var i = 0; i <= 11; i++ )
			{
				await dbContext.FileIndex.AddAsync(new FileIndexItem { Tags = "test" },
					CancellationToken.None);
			}

			await dbContext.SaveChangesAsync(CancellationToken.None);

			var logger = new FakeIWebLogger();
			var appSettings = new AppSettings();

			var hostedService =
				new SearchSuggestionsInflateHostedService(scopeFactory, memoryCache, logger,
					appSettings) { Interval = new TimeSpan(0, 0, 1) };

			await hostedService.StartAsync(CancellationToken.None);
			await WaitForLogCountAsync(logger, 2, TimeSpan.FromSeconds(4));
			await hostedService.StopAsync(CancellationToken.None);

			if ( memoryCache.TryGetValue(nameof(SearchSuggestionsService),
				    out var objectFileFolders) )
			{
				var allSuggestions = objectFileFolders as List<KeyValuePair<string, int>> ?? [];
				var result = allSuggestions.FirstOrDefault(p => p.Key == "test");
				Assert.AreEqual("test", result.Key);
			}
			else
			{
				Assert.Fail("Cache was not set");
			}

			Assert.HasCount(2, logger.TrackedDebug.Where(p =>
				p.Item2?.Contains("Cache inflated successfully") == true));
			return true;
		}
	}

	private static async Task WaitForLogCountAsync(FakeIWebLogger logger, int expectedCount,
		TimeSpan timeout)
	{
		var stopAt = DateTime.UtcNow + timeout;
		while ( DateTime.UtcNow < stopAt )
		{
			var count = logger.TrackedDebug.Count(p =>
				p.Item2?.Contains("Cache inflated successfully") == true);
			if ( count >= expectedCount )
			{
				return;
			}

			await Task.Delay(100, CancellationToken.None);
		}
	}
}
