using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.export.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.worker.Interfaces;
using starskycore.Helpers;

namespace starsky.Controllers
{
	[Authorize]
	public sealed class ExportController : Controller
	{
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IStorage _hostFileSystemStorage;
		private readonly IExport _export;

		public ExportController( IUpdateBackgroundTaskQueue queue,
			ISelectorStorage selectorStorage, IExport export)
		{
			_bgTaskQueue = queue;
			_hostFileSystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_export = export;
		}

		/// <summary>
		/// Export source files to an zip archive
		/// </summary>
		/// <param name="f">subPath to files</param>
		/// <param name="collections">enable files with the same name (before the extension)</param>
		/// <param name="thumbnail">export thumbnails</param>
		/// <returns>name of a to generate zip file</returns>
		/// <response code="200">the name of the to generated zip file</response>
		/// <response code="404">files not found</response>
		[HttpPost("/api/export/create-zip")]
		[ProducesResponseType(typeof(string),200)] // "zipHash"
		[ProducesResponseType(typeof(List<FileIndexItem>),404)] // "Not found"
		[Produces("application/json")]
		public async Task<IActionResult> CreateZip(string f, bool collections = true, bool thumbnail = false)
		{
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);
			var (zipOutputName, fileIndexResultsList) = await _export.PreflightAsync(inputFilePaths, collections, thumbnail);
			
			// When all items are not found
			// allow read only
			if (fileIndexResultsList.TrueForAll(p => p.Status != FileIndexItem.ExifStatus.Ok) )
				return NotFound(fileIndexResultsList);
			
			// NOT covered: when try to export for example image thumbnails of xml file
				
			// Creating a zip is a background task
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
			{
				await _export.CreateZip(fileIndexResultsList, thumbnail, zipOutputName);
			}, zipOutputName);
			
			// for the rest api
			return Json(zipOutputName);
		}

		/// <summary>
		/// Get the exported zip, but first call 'createZip'
		/// use for example this url: /export/zip/TNA995920129.zip
		/// TNA995920129 is from 'createZip'
		/// </summary>
		/// <param name="f">zip hash e.g. TNA995920129</param>
		/// <param name="json">true to get OK instead of a zip file</param>
		/// <returns>Not ready or the zip-file</returns>
		/// <response code="200">if json is true return 'OK', else the zip file</response>
		/// <response code="206">Not ready generating the zip, please wait</response>
		[HttpGet("/api/export/zip/{f}.zip")]
		[ProducesResponseType(200)] // "zip file"
		[ProducesResponseType(206)] // "Not Ready"
		public ActionResult Status(string f, bool json = false)
		{
			var (status, sourceFullPath) = _export.StatusIsReady(f);
			switch ( status )
			{
				case null:
					return NotFound("Path is not found");
				case false:
					Response.StatusCode = 206;
					return Json("Not Ready");
			}

			if ( json ) return Json("OK");
			
			var fs = _hostFileSystemStorage.ReadStream(sourceFullPath);
			// Return the right mime type
			return File(fs, MimeHelper.GetMimeTypeByFileName(sourceFullPath));
		}
	}
}
