using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.rename.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.project.web.ViewModels;

namespace starsky.Controllers;

[Authorize]
public sealed class DiskController : Controller
{
	private readonly IWebSocketConnectionsService _connectionsService;
	private readonly IStorage _iStorage;
	private readonly INotificationQuery _notificationQuery;
	private readonly IQuery _query;

	public DiskController(IQuery query, ISelectorStorage selectorStorage,
		IWebSocketConnectionsService connectionsService, INotificationQuery notificationQuery)
	{
		_query = query;
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_connectionsService = connectionsService;
		_notificationQuery = notificationQuery;
	}

	/// <summary>
	///     Make a directory (-p)
	/// </summary>
	/// <param name="f">subPaths split by dot comma</param>
	/// <returns>list of changed files IActionResult Mkdir</returns>
	/// <response code="200">create the item on disk and in db</response>
	/// <response code="409">A conflict, Directory already exist</response>
	/// <response code="400">missing path</response>
	/// <response code="401">User unauthorized</response>
	[HttpPost("/api/disk/mkdir")]
	[ProducesResponseType(typeof(List<SyncViewModel>), 200)]
	[ProducesResponseType(typeof(List<SyncViewModel>), 409)]
	[ProducesResponseType(typeof(List<SyncViewModel>), 400)]
	[ProducesResponseType(typeof(string), 401)]
	[Produces("application/json")]
	public async Task<IActionResult> Mkdir(string f)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		var inputFilePaths = PathHelper.SplitInputFilePaths(f).ToList();
		if ( inputFilePaths.Count == 0 )
		{
			Response.StatusCode = 400;
			return Json(new List<SyncViewModel>());
		}

		var syncResultsList = new List<SyncViewModel>();

		foreach ( var subPath in inputFilePaths.Select(PathHelper.RemoveLatestSlash) )
		{
			var toAddStatus = new SyncViewModel
			{
				FilePath = subPath, Status = FileIndexItem.ExifStatus.Ok
			};

			// if it exists make sure the item is added to the database
			await AddIfExistItemAsync(subPath);

			if ( _iStorage.ExistFolder(subPath) )
			{
				toAddStatus.Status = FileIndexItem.ExifStatus.OperationNotSupported;
				syncResultsList.Add(toAddStatus);
				continue;
			}

			// add to fs
			_iStorage.CreateDirectory(subPath);

			syncResultsList.Add(toAddStatus);
		}

		// When all items are not found
		if ( syncResultsList.TrueForAll(p => p.Status != FileIndexItem.ExifStatus.Ok) )
		{
			Response.StatusCode = 409; // A conflict, Directory already exist
		}

		await SyncMessageToSocket(syncResultsList, ApiNotificationType.Mkdir);

		return Json(syncResultsList);

		async Task AddIfExistItemAsync(string subPath)
		{
			if ( !await _query.ExistsAsync(subPath) )
			{
				await _query.AddItemAsync(new FileIndexItem(subPath)
				{
					IsDirectory = true, ImageFormat = ExtensionRolesHelper.ImageFormat.directory
				});
			}
		}
	}

	/// <summary>
	///     Update other users with a message from SyncViewModel
	/// </summary>
	/// <param name="syncResultsList">SyncViewModel</param>
	/// <param name="type">optional debug name</param>
	/// <returns>Completed send of Socket SendToAllAsync</returns>
	private async Task SyncMessageToSocket(IEnumerable<SyncViewModel> syncResultsList,
		ApiNotificationType type = ApiNotificationType.Unknown)
	{
		var list = syncResultsList.Select(t => new FileIndexItem(t.FilePath)
		{
			Status = t.Status, IsDirectory = true
		}).ToList();

		var webSocketResponse = new ApiNotificationResponseModel<
			List<FileIndexItem>>(list, type);

		await _notificationQuery.AddNotification(webSocketResponse);
		await _connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
	}

	/// <summary>
	///     Rename file/folder and update it in the database
	/// </summary>
	/// <param name="f">from subPath</param>
	/// <param name="to">to subPath</param>
	/// <param name="collections">is collections bool</param>
	/// <param name="currentStatus">default is to not included files that are removed in result </param>
	/// <returns>list of details form changed files (IActionResult Rename)</returns>
	/// <response code="200">the item including the updated content</response>
	/// <response code="404">item not found in the database or on disk</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(typeof(List<FileIndexItem>), 200)]
	[ProducesResponseType(typeof(List<FileIndexItem>), 404)]
	[HttpPost("/api/disk/rename")]
	[Produces("application/json")]
	public async Task<IActionResult> Rename([Required] string f, string to, bool collections = true,
		bool currentStatus = true)
	{
		if ( string.IsNullOrEmpty(f) || !ModelState.IsValid )
		{
			return BadRequest("No input files");
		}

		var rename = await new RenameService(_query, _iStorage).Rename(f, to, collections);

		// When all items are not found
		if ( rename.TrueForAll(p => p.Status != FileIndexItem.ExifStatus.Ok) )
		{
			return NotFound(rename);
		}

		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(rename,
				ApiNotificationType.Rename);

		await _notificationQuery.AddNotification(webSocketResponse);
		await _connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);

		return Json(currentStatus
			? rename.Where(p => p.Status
			                    != FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList()
			: rename);
	}
}
