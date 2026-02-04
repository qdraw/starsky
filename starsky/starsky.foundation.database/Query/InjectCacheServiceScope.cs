using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.database.Query;

public class InjectCacheServiceScope
{
	private readonly IMemoryCache? _cache;

	public InjectCacheServiceScope(IServiceScopeFactory? scopeFactory)
	{
		if ( scopeFactory == null )
		{
			return;
		}

		var scope = scopeFactory.CreateScope();
		_cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
	}

	internal IMemoryCache? Cache()
	{
		return _cache;
	}
}
