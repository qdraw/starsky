using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.worker.Interfaces;

namespace starsky.Controllers;

[Authorize]
public sealed class PublishController : Controller
{
	private readonly AppSettings _appSettings;
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IStorage _hostStorage;
	private readonly IMetaInfo _metaInfo;
	private readonly IPublishPreflight _publishPreflight;
	private readonly IWebHtmlPublishService _publishService;
	private readonly IWebLogger _webLogger;

	public PublishController(AppSettings appSettings, IPublishPreflight publishPreflight,
		IWebHtmlPublishService publishService, IMetaInfo metaInfo,
		ISelectorStorage selectorStorage,
		IUpdateBackgroundTaskQueue queue, IWebLogger webLogger)
	{
		_appSettings = appSettings;
		_publishPreflight = publishPreflight;
		_publishService = publishService;
		_metaInfo = metaInfo;
		_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_bgTaskQueue = queue;
		_webLogger = webLogger;
	}

	/// <summary>
	///     Get all publish profiles
	///     To see the entire config check appSettings
	/// </summary>
	/// <returns>list of publish profiles</returns>
	/// <response code="200">list of all publish profiles in the _appSettings scope</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/publish/")]
	[Produces("application/json")]
	public IActionResult PublishGet()
	{
		return Json(_publishPreflight.GetAllPublishProfileNames());
	}

	/// <summary>
	///     Publish
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <param name="itemName">itemName</param>
	/// <param name="publishProfileName">publishProfileName</param>
	/// <param name="force"></param>
	/// <returns>update json</returns>
	/// <response code="200">start with generation</response>
	/// <response code="409">item with the same name already exist</response>
	/// <response code="401">User unauthorized</response>
	[ProducesResponseType(typeof(string), 200)]
	[ProducesResponseType(typeof(string), 409)]
	[ProducesResponseType(typeof(List<string>), 400)]
	[ProducesResponseType(typeof(void), 401)]
	[HttpPost("/api/publish/create")]
	[Produces("application/json")]
	public async Task<IActionResult> PublishCreateAsync(string f, string itemName,
		string publishProfileName, bool force = false)
	{
		var (isValid, preflightErrors) = _publishPreflight.IsProfileValid(publishProfileName);
		if ( !isValid || !ModelState.IsValid )
		{
			return BadRequest(preflightErrors);
		}

		var inputFilePaths = PathHelper.SplitInputFilePaths(f).ToList();
		var info = await _metaInfo.GetInfoAsync(inputFilePaths, false);
		if ( info.TrueForAll(p =>
			    p.Status != FileIndexItem.ExifStatus.Ok &&
			    p.Status != FileIndexItem.ExifStatus.ReadOnly) )
		{
			return NotFound(info);
		}

		var slugItemName = GenerateSlugHelper.GenerateSlug(itemName, true);
		_webLogger.LogInformation($"[/api/publish/create] Press publish: " +
		                          $"{slugItemName} {f} {DateTime.UtcNow}");

		var location = Path.Combine(_appSettings.TempFolder, slugItemName);

		if ( CheckIfNameExist(slugItemName) )
		{
			if ( !force )
			{
				return Conflict($"name {slugItemName} exist");
			}

			ForceCleanPublishFolderAndZip(location);
		}

		// Creating Publish is a background task
		await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
		{
			var renderCopyResult = await _publishService.RenderCopy(info,
				publishProfileName, itemName, location);
			await _publishService.GenerateZip(_appSettings.TempFolder, itemName,
				renderCopyResult);
			_webLogger.LogInformation($"[/api/publish/create] done: " +
			                          $"{itemName} {DateTime.UtcNow}");
		}, publishProfileName + "_" + itemName);

		// Get the zip 	by	[HttpGet("/export/zip/{f}.zip")]
		return Json(slugItemName);
	}

	/// <summary>
	///     To give the user UI feedback when submitting the itemName
	///     True is not to continue with this name
	/// </summary>
	/// <returns>true= fail, </returns>
	/// <response code="200">boolean, true is not good</response>
	/// <response code="401">User unauthorized</response>
	[HttpGet("/api/publish/exist")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(bool), 200)]
	[ProducesResponseType(typeof(void), 401)]
	public IActionResult Exist(string itemName)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}
		
		return Json(CheckIfNameExist(GenerateSlugHelper.GenerateSlug(itemName)));
	}

	private bool CheckIfNameExist(string slugItemName)
	{
		if ( string.IsNullOrEmpty(slugItemName) )
		{
			return true;
		}

		var location = Path.Combine(_appSettings.TempFolder, slugItemName);
		return _hostStorage.ExistFolder(location) || _hostStorage.ExistFile(location + ".zip");
	}

	private void ForceCleanPublishFolderAndZip(string location)
	{
		if ( _hostStorage.ExistFolder(location) )
		{
			_hostStorage.FolderDelete(location);
		}

		if ( _hostStorage.ExistFile(location + ".zip") )
		{
			_hostStorage.FileDelete(location + ".zip");
		}
	}
}
