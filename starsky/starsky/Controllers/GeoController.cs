using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.geolookup.Models;
using starsky.feature.geolookup.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;
using starsky.Helpers;

namespace starsky.Controllers;

[Authorize]
public sealed class GeoController : Controller
{
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IMemoryCache? _cache;
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public GeoController(IUpdateBackgroundTaskQueue queue,
		ISelectorStorage selectorStorage,
		IMemoryCache? memoryCache, IWebLogger logger, IServiceScopeFactory serviceScopeFactory)
	{
		_bgTaskQueue = queue;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_cache = memoryCache;
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;
	}

	/// <summary>
	///     Get Geo sync status
	/// </summary>
	/// <param name="f">sub path folders</param>
	/// <returns>status of geo sync</returns>
	/// <response code="200">the current status</response>
	/// <response code="404">cache service is missing</response>
	[HttpGet("/api/geo/status")]
	[ProducesResponseType(typeof(GeoCacheStatus), 200)] // "cache service is missing"
	[ProducesResponseType(typeof(string), 404)] // "Not found"
	[Produces("application/json")]
	public IActionResult Status(
		string f = "/")
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		if ( _cache == null )
		{
			return NotFound("cache service is missing");
		}

		return Json(new GeoCacheStatusService(_cache).Status(f));
	}


	/// <summary>
	///     Reverse lookup for Geo Information and/or add Geo location based on a GPX file within the same
	///     directory
	/// </summary>
	/// <param name="f">subPath only folders</param>
	/// <param name="index">-i in cli</param>
	/// <param name="overwriteLocationNames"> -a in cli</param>
	/// <returns></returns>
	/// <response code="200">event is fired</response>
	/// <response code="404">sub path not found in the database</response>
	/// <response code="401">User unauthorized</response>
	[HttpPost("/api/geo/sync")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(string), 404)] // event is fired
	[ProducesResponseType(typeof(string), 200)] // "Not found"
	public async Task<IActionResult> GeoSyncFolder(
		string f = "/",
		bool index = true,
		bool overwriteLocationNames = false
	)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		if ( _iStorage.IsFolderOrFile(f) == FolderOrFileModel.FolderOrFileTypeList.Deleted )
		{
			return NotFound("Folder location is not found");
		}


		await _bgTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			MetaData = f,
			TraceParentId = Activity.Current?.Id,
			PriorityLane = ProcessTaskQueue.PriorityLaneUpdate,
			JobType = ControllerBackgroundJobTypes.GeoSync,
			PayloadJson = JsonSerializer.Serialize(new GeoSyncBackgroundPayload
			{
				SubPath = f, Index = index, OverwriteLocationNames = overwriteLocationNames
			})
		});

		return Json("job started");
	}
}
