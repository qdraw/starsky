using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;

namespace starsky.Controllers
{
	[Authorize]
	public sealed class MemoryCacheDebugController : Controller
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IWebLogger _logger;

		public MemoryCacheDebugController(IMemoryCache memoryCache, IWebLogger logger)
		{
			_memoryCache = memoryCache;
			_logger = logger;
		}

		/// <summary>
		/// View data from the memory cache - use to debug
		/// </summary>
		/// <returns></returns>
		[HttpGet("/api/memory-cache-debug")]
		public IActionResult MemoryCacheDebug()
		{
			var result = new Dictionary<string, object>();
			foreach ( var key in _memoryCache.GetKeys<string>() )
			{
				_memoryCache.TryGetValue(key, out var data);
				try
				{
					result.Add(key, JsonSerializer.Serialize(data));
				}
				catch ( JsonException exception )
				{
					_logger.LogError(exception, $"[MemoryCacheDebug] Json {key} has failed (catch-ed)");
					result.Add(key, "[ERROR] Has failed parsing");
				}
			}
			return Json(result);
		}
	}
}
