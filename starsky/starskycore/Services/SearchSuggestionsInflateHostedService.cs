using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.database.Data;
using starskycore.Models;

namespace starskycore.Services
{
	public class SearchSuggestionsInflateHostedService : IHostedService
	{
		private readonly AppSettings _appSettings;
		private readonly IMemoryCache _memoryCache;
		private readonly IServiceScopeFactory _scopeFactory;
		
		public SearchSuggestionsInflateHostedService(IServiceScopeFactory scopeFactory,
			IMemoryCache memoryCache, AppSettings appSettings = null)
		{
			_scopeFactory = scopeFactory;
			_memoryCache = memoryCache;
			_appSettings = appSettings;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				new SearchSuggestionsService(dbContext, _memoryCache, _appSettings).Inflate();
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			// nope
			return Task.CompletedTask;
		}
	}
}

