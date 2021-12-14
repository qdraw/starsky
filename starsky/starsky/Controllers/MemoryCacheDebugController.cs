using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.platform.Extensions;

namespace starsky.Controllers
{
	[Authorize]
	public class MemoryCacheDebugController: Controller
	{
		private readonly IMemoryCache _memoryCache;

		public MemoryCacheDebugController(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}
		
		[HttpGet("/api/MemoryCacheDebug")]
		public IActionResult MemoryCacheDebug()
		{
			var result = new Dictionary<string, object>();
			foreach ( var key in _memoryCache.GetKeys<string>() )
			{
				_memoryCache.TryGetValue(key, out var data);
				result.Add(key, JsonSerializer.Serialize(data));
			}
			return Json(result);
		}
	}
}
