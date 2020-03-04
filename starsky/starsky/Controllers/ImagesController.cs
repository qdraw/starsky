using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starsky.Controllers
{
	public class ImagesController : Controller
	{
		private ISelectorStorage _selectorStorage;

		public ImagesController(ISelectorStorage selectorStorage)
		{
			_selectorStorage = selectorStorage;
		}
		[HttpGet("/api/images")]
		public IActionResult Thumbnail()
		{
			var result = _selectorStorage.Get(SelectorStorage.StorageServices.SubPath).ExistFolder("/__starsky");
			
			// var result = new StorageSelector(_serviceProvider)
			// 	.Select(StorageSelector.StorageServices.SubPath)
			// 	.ExistFolder("/__starsky");
			return Json(result);
		}
		
	}

	// 	/// <summary>
 //        /// Http Endpoint to get full size image or thumbnail
 //        /// </summary>
 //        /// <param name="f">one single file</param>
 //        /// <param name="isSingleitem">true = load orginal</param>
 //        /// <param name="json">text as output</param>
 //        /// <returns>thumbnail or status</returns>
 //        /// <response code="200">returns content of the file or when json is true, "OK"</response>
 //        /// <response code="404">item not found on disk</response>
 //        /// <response code="409">Conflict, you did try get for example a thumbnail of a raw file</response>
 //        /// <response code="209">"Thumbnail is not ready yet"</response>
 //        /// <response code="401">User unauthorized</response>
 //        [HttpGet("/api/images/{f}")]
 //        [ProducesResponseType(200)] // file
 //        [ProducesResponseType(404)] // not found
 //        [ProducesResponseType(409)] // raw
 //        [ProducesResponseType(209)] // "Thumbnail is not ready yet"
 //        [IgnoreAntiforgeryToken]
 //        [AllowAnonymous] // <=== ALLOW FROM EVERYWHERE
 //        [ResponseCache(Duration = 29030400)] // 4 weeks
 //        public IActionResult Thumbnail(
 //            string f, 
 //            bool isSingleitem = false, 
 //            bool json = false)
 //        {
 //            // f is Hash
 //            // isSingleItem => detailView
 //            // Retry thumbnail => is when you press reset thumbnail
 //            // json, => to don't waste the users bandwidth.
 //
	//         // For serving jpeg files
	//         f = FilenamesHelper.GetFileNameWithoutExtension(f);
	//         
	//         // Restrict the fileHash to letters and digits only
	//         // I/O function calls should not be vulnerable to path injection attacks
	//         if (!Regex.IsMatch(f, "^[a-zA-Z0-9_]+$") )
	//         {
	// 	        return BadRequest();
	//         }
	//         
 //            var thumbPath = _appSettings.ThumbnailTempFolder + f + ".jpg";
 //
 //            if (FilesHelper.IsFolderOrFile(thumbPath) == FolderOrFileModel.FolderOrFileTypeList.File)
 //            {
 //
 //                // thumbs are always in jpeg
 //                FileStream fs = System.IO.File.OpenRead(thumbPath);
 //                return File(fs, "image/jpeg");
 //            }
 //
	//         
	//         
 //        }
	// }
}
