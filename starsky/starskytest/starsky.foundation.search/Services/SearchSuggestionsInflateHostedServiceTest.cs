using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.search.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.search.Services;

[TestClass]
public sealed class SearchSuggestionsInflateHostedServiceTest
{
	private readonly ApplicationDbContext _dbContext;
	private readonly IMemoryCache _memoryCache;
	private readonly IServiceScopeFactory _scopeFactory;

	public SearchSuggestionsInflateHostedServiceTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		_memoryCache = provider.GetRequiredService<IMemoryCache>();

		var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase()
			.BuildServiceProvider();

		var services = new ServiceCollection();
		services
			.AddDbContext<ApplicationDbContext>(b =>
				b.UseInMemoryDatabase(nameof(SearchSuggestionsInflateHostedService))
					.UseInternalServiceProvider(efServiceProvider));

		var serviceProvider = services.BuildServiceProvider();

		_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		_dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
	}

	[TestMethod]
	public async Task Inflate_Test()
	{
		// Minimal 10 items
		var testContent = new FileIndexItem { Tags = "test, testung" };
		_dbContext.FileIndex.AddRange(new List<FileIndexItem>
		{
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent,
			testContent
		});
		await _dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		await new SearchSuggestionsInflateHostedService(_scopeFactory, _memoryCache,
			new FakeIWebLogger(),
			new AppSettings()).StartAsync(new CancellationToken());

		var allSuggestions = await new SearchSuggestionsService(_dbContext,
				_memoryCache, new FakeIWebLogger(), new AppSettings())
			.GetAllSuggestions();

		var result = allSuggestions.FirstOrDefault(p => p.Key == "test");
		if ( result.Key != null )
		{
			Assert.Fail("No test item found");
		}
	}

	public TestContext TestContext { get; set; }
}
