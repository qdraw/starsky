using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Extensions
{
	public static class EntityFrameworkExtensions
	{
		const string CacheKey = "TestConnection";
		/// <summary>
		/// Test the connection if this is mysql
		/// </summary>
		/// <param name="context">database context</param>
		/// <param name="logger">logger</param>
		/// <param name="cache">store in cache</param>
		/// <returns>bool, true if connection is there</returns>
		/// <exception cref="ArgumentNullException">When AppSettings is null</exception>
		public static bool TestConnection(this DbContext context, IWebLogger logger, IMemoryCache? cache = null)
		{
			if ( cache != null && cache.TryGetValue(CacheKey, out bool cacheValue) )
			{
				return cacheValue;
			}
			
			try
			{
				if ( context?.Database == null ) return false;
				context.Database.CanConnect();
			}
			catch ( MySqlException e)
			{
				logger.LogInformation($"[TestConnection] WARNING >>> \n{e}\n <<<");
				return false;
			}
			cache?.Set(CacheKey, true, TimeSpan.FromMinutes(1));
			
			return true;
		}
	}
}
