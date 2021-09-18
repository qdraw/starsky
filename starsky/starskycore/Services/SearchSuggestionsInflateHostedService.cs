using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using starsky.foundation.database.Data;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starskycore.Services
{
	[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public class SearchSuggestionsInflateHostedService : IHostedService
	{
		private readonly AppSettings _appSettings;
		private readonly IMemoryCache _memoryCache;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IWebLogger _logger;

		public SearchSuggestionsInflateHostedService(IServiceScopeFactory scopeFactory,
			IMemoryCache memoryCache, IWebLogger logger, AppSettings appSettings = null)
		{
			_scopeFactory = scopeFactory;
			_memoryCache = memoryCache;
			_logger = logger;
			_appSettings = appSettings;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				await new SearchSuggestionsService(dbContext, _memoryCache, _logger, _appSettings).Inflate();
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			// nope
			return Task.CompletedTask;
		}
	}
}

