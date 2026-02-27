using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.webftppublish.Interfaces;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.platform.Helpers.Slug;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers;

[Authorize]
public class PublishRemoteController(
	ISelectorStorage selectorStorage,
	IPublishPreflight publishPreflight,
	AppSettings appSettings,
	IRemotePublishService remotePublishService) : Controller
{
	private readonly IStorage _hostStorage =
		selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);

	/// <summary>
	///     Publish generated zip to remote destination (FTP or LocalFileSystem)
	/// </summary>
	/// <param name="itemName">itemName used in /api/publish/create</param>
	/// <param name="publishProfileName">publishProfileName used in /api/publish/create</param>
	/// <returns>true on success</returns>
	/// <response code="200">upload succeeded</response>
	/// <response code="400">profiles are invalid or remote type not supported</response>
	/// <response code="404">zip not found</response>
	/// <response code="401">User unauthorized</response>
	[HttpPost("/api/publish-remote/remote")]
	[Produces("application/json")]
	[ProducesResponseType(typeof(bool), 200)]
	[ProducesResponseType(typeof(string), 400)]
	[ProducesResponseType(typeof(string), 404)]
	[ProducesResponseType(typeof(void), 401)]
	public async Task<IActionResult> PublishFtpAsync(string itemName,
		string publishProfileName)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		var (isValidProfile, preflightErrors) =
			publishPreflight.IsProfileValid(publishProfileName);
		if ( !isValidProfile )
		{
			return BadRequest(preflightErrors);
		}

		if ( !remotePublishService.IsPublishEnabled(publishProfileName) )
		{
			return BadRequest("FTP publishing disabled for publish profile");
		}

		var slugItemName = GenerateSlugHelper.GenerateSlug(itemName, true);
		var zipFullPath = Path.Combine(appSettings.TempFolder, slugItemName + ".zip");

		if ( !_hostStorage.ExistFile(zipFullPath) )
		{
			return NotFound("Publish zip not found");
		}

		var manifest = await remotePublishService.IsValidZipOrFolder(zipFullPath);
		if ( manifest == null )
		{
			return BadRequest("Publish zip is invalid");
		}

		var publishResult = remotePublishService.Run(zipFullPath, publishProfileName,
			manifest.Slug, manifest.Copy);
		if ( !publishResult )
		{
			return BadRequest("Publishing failed");
		}

		return Json(true);
	}
}
