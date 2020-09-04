using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskycore.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class PublishController : Controller
	{
		private readonly IWebHtmlPublishService _publishService;
		private readonly IPublishPreflight _publishPreflight;
		private readonly IMetaInfo _metaInfo;
		private readonly AppSettings _appSettings;
		private readonly IStorage _hostStorage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;

		public PublishController(AppSettings appSettings, IPublishPreflight publishPreflight, 
			IWebHtmlPublishService publishService, IMetaInfo metaInfo, ISelectorStorage selectorStorage,
			IBackgroundTaskQueue queue)
		{
			_appSettings = appSettings;
			_publishPreflight = publishPreflight;
			_publishService = publishService;
			_metaInfo = metaInfo;
			_hostStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
			_bgTaskQueue = queue;
		}

		/// <summary>
		/// Get all publish profiles
		/// To see the entire config check appSettings
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
		/// Publish
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
		[ProducesResponseType(typeof(void), 401)]
		[HttpPost("/api/publish/create")]
		[Produces("application/json")]
		public async Task<IActionResult> PublishCreate(string f, string itemName, 
			string publishProfileName, bool force = false)
		{
			var inputFilePaths = PathHelper.SplitInputFilePaths(f).ToList();
			var info = _metaInfo.GetInfo(inputFilePaths, false);
			if (info.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
				return NotFound(info);

			var slugItemName = _appSettings.GenerateSlug(itemName);
			var location = Path.Combine(_appSettings.TempFolder,slugItemName );
			
			if ( CheckIfNameExist(slugItemName) )
			{
				if ( !force ) return Conflict($"name {slugItemName} exist");
				ForceCleanPublishFolderAndZip(location);
			}

			// Creating Publish is a background task
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				var renderCopyResult = await _publishService.RenderCopy(info, 
					publishProfileName, itemName, location);
				await _publishService.GenerateZip(_appSettings.TempFolder, itemName, 
					renderCopyResult,true );	
			});
			
			// Get the zip 	by	[HttpGet("/export/zip/{f}.zip")]
			return Json(slugItemName);
		}
		
		/// <summary>
		/// To give the user UI feedback when submitting the itemName
		/// True is not to continue with this name
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
			return Json(CheckIfNameExist(_appSettings.GenerateSlug(itemName)));
		}

		private bool CheckIfNameExist(string slugItemName)
		{
			var location = Path.Combine(_appSettings.TempFolder,slugItemName );
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
				_hostStorage.FileDelete(location+ ".zip");
			}
		}
	}
}
