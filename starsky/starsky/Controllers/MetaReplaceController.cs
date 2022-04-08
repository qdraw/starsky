using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.Controllers
{
	[Authorize]
	public class MetaReplaceController : Controller
	{
		private readonly IMetaReplaceService _metaReplaceService;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly IWebLogger _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly INotificationQuery _notificationQuery;

		public MetaReplaceController(IMetaReplaceService metaReplaceService,  IUpdateBackgroundTaskQueue queue, 
			IWebSocketConnectionsService connectionsService, IWebLogger logger, IServiceScopeFactory scopeFactory, 
			INotificationQuery notificationQuery)
		{
			_scopeFactory = scopeFactory;
			_metaReplaceService = metaReplaceService;
			_bgTaskQueue = queue;
			_connectionsService = connectionsService;
			_logger = logger;
			_notificationQuery = notificationQuery;
		}

		/// <summary>
		/// Search and Replace text in meta information 
		/// </summary>
		/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
		/// <param name="fieldName">name of fileIndexItem field e.g. Tags</param>
		/// <param name="search">text to search for</param>
		/// <param name="replace">replace [search] with this text</param>
		/// <param name="collections">enable collections</param>
		/// <returns>list of changed files (IActionResult Replace)</returns>
		/// <response code="200">Initialized replace job</response>
		/// <response code="404">item(s) not found</response>
		/// <response code="401">User unauthorized</response>
		[HttpPost("/api/replace")]
		[ProducesResponseType(typeof(List<FileIndexItem>),200)]
		[ProducesResponseType(typeof(List<FileIndexItem>),404)]
		[Produces("application/json")]
		public async Task<IActionResult> Replace(string f, string fieldName, string search,
			string replace, bool collections = true)
		{
			var stopwatch = StopWatchLogger.StartUpdateReplaceStopWatch();

			var fileIndexResultsList = await _metaReplaceService
				.Replace(f, fieldName, search, replace, collections);
		    
			var resultsOkOrDeleteList = fileIndexResultsList.Where(
				p => p.Status == FileIndexItem.ExifStatus.Ok || 
				     p.Status == FileIndexItem.ExifStatus.Deleted).ToList();
			
			var changedFileIndexItemName = resultsOkOrDeleteList.
				ToDictionary(item => item.FilePath, item => new List<string> {fieldName});

			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async _ =>
			{
				var metaUpdateService = _scopeFactory.CreateScope()
					.ServiceProvider.GetRequiredService<IMetaUpdateService>();
				await metaUpdateService
					.UpdateAsync(changedFileIndexItemName, resultsOkOrDeleteList,
						null, collections, false, 0);
			});
			
			// before sending not founds
			new StopWatchLogger(_logger).StopUpdateReplaceStopWatch("update", f,collections, stopwatch);
			
			// When all items are not found
			if (!resultsOkOrDeleteList.Any())
			{
				return NotFound(fileIndexResultsList);
			}
			
			// Push direct to socket when update or replace to avoid undo after a second
			var webSocketResponse =
				new ApiNotificationResponseModel<List<FileIndexItem>>(resultsOkOrDeleteList, ApiNotificationType.Replace);
			await _connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
			await _notificationQuery.AddNotification(webSocketResponse);

			return Json(fileIndexResultsList);
		}
	}
}
