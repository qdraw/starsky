using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;

namespace starsky.Controllers
{
    [Authorize]
    public class IndexController : Controller
    {
        
        private readonly IQuery _query;
        private readonly AppSettings _appsettings;

        public IndexController(IQuery query, AppSettings appsettings = null)
        {
            _query = query;
            _appsettings = appsettings;
        }
        
	    /// <summary>
	    /// The database-view of a directory
	    /// </summary>
	    /// <param name="f">subPath</param>
	    /// <param name="colorClass">filter on colorClass (use int)</param>
	    /// <param name="json">to not show as webpage</param>
	    /// <param name="collections">to combine files with the same name before the extension</param>
	    /// <param name="hidedelete">ignore deleted files</param>
	    /// <returns></returns>
	    /// <response code="200">returns a list of items from the database</response>
	    /// <response code="404">subpath not found in the database</response>
	    /// <response code="401">User unauthorized</response>
	    [HttpGet("/api/index")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(ArchiveViewModel),200)]
		[ProducesResponseType(404)]
        public IActionResult Index(
            string f = "/", 
            string colorClass = null,
            bool json = true,
            bool collections = true,
            bool hidedelete = true
            )
        {
            f = PathHelper.PrefixDbSlash(f);
            
            // Trick for avoiding spaces for behind proxy
            f = f.Replace("$20", " ");
            
            // Used in Detail and Index View => does not hide this single item
            var colorClassFilterList = new FileIndexItem().GetColorClassList(colorClass);
            var subpath = _query.SubPathSlashRemove(f);

            // First check if it is a single Item
            var singleItem = _query.SingleItem(subpath, colorClassFilterList,collections);
            // returns no object when it a directory
            
            if (singleItem?.IsDirectory == false)
            {
	            var fileHashWithExt = singleItem.FileIndexItem.FileHash;
	            if ( singleItem.FileIndexItem.ImageFormat == ExtensionRolesHelper.ImageFormat.jpg )
		            fileHashWithExt += ".jpg";
	            var fileHashThumbnailHttpUrl = SingleItemThumbnailHttpUrl(fileHashWithExt);

	            var infoHttpUrl = SingleItemInfoHttpUrl(singleItem.FileIndexItem.FilePath,collections);

                AddHttp2SingleFile(fileHashThumbnailHttpUrl,infoHttpUrl);
                
                if (json) return Json(singleItem);
                return View("~/Views/V1/Detailview.cshtml", singleItem);
            }
            
            // (singleItem.IsDirectory) or not found
            var directoryModel = new ArchiveViewModel
            {
                FileIndexItems = _query.DisplayFileFolders(subpath,colorClassFilterList,collections,hidedelete),
                ColorClassFilterList = 	colorClassFilterList,
                RelativeObjects = _query.GetNextPrevInFolder(subpath), // Args are not shown in this view
                Breadcrumb = Breadcrumbs.BreadcrumbHelper(subpath),
                SearchQuery = subpath.Split("/").LastOrDefault(),
                SubPath = subpath,
                CollectionsCount = _query.DisplayFileFolders(
	                subpath,null,false,hidedelete).Count(p => !p.IsDirectory),
                ColorClassUsage = _query.DisplayFileFolders(
	                subpath,null,false,hidedelete).Select( p => p.ColorClass).Distinct().ToList(),
                IsReadOnly = true // default values is updated in later point
            };

            if (singleItem == null)
            {
                // For showing a new database
                var queryIfFolder = _query.GetObjectByFilePath(subpath);

                // For showing a new database
                switch (f)
                {
                    case "/" when !json && queryIfFolder == null:
                        return View("~/Views/V1/index.cshtml",directoryModel);
                    case "/" when queryIfFolder == null:
                        return Json(directoryModel);
                }

                if (queryIfFolder == null) // used to have: singleItem?.FileIndexItem.FilePath == null &&
                {
                    Response.StatusCode = 404;
                    if (json) return Json("not found");
                    return View("~/Views/V1/Error.cshtml");
                }
            }
            
            // now update
            if (_appsettings != null) directoryModel.IsReadOnly = _appsettings.IsReadOnly(subpath);

            if (json) return Json(directoryModel);
            return View("~/Views/V1/index.cshtml",directoryModel);
        }

        // For returning the Url of the webpage, this has a dependency
        public string SingleItemThumbnailHttpUrl(string fileHash)
        {
            // when using a unit test appSettings will be null
            if (_appsettings == null || !_appsettings.AddHttp2Optimizations) return string.Empty;
            return Url.Action("Thumbnail", "Api", new {f = fileHash});
        }
	    
	    // For returning the Url of the webpage, this has a dependency
	    public string SingleItemInfoHttpUrl(string infoSubPath, bool collections)
	    {
		    // when using a unit test appSettings will be null
		    if (_appsettings == null || !_appsettings.AddHttp2Optimizations) return string.Empty;
		    
		    var infoApiBase = Url.Action("Info", "Api", new {f = infoSubPath, collections});
		    infoApiBase = infoApiBase.Replace("+", "%2B");
		    return infoApiBase; 
	    }

        // Feature to Add Http2 push to the response headers
        public void AddHttp2SingleFile(string fileHashThumbnailHttpUrl, string infoHttpUrl)
        {
            if (_appsettings == null || !_appsettings.AddHttp2Optimizations) return;

	        // HTTP2 push
            Response.Headers["Link"] =
                "<" + fileHashThumbnailHttpUrl  +
                "?issingleitem=True>; rel=preload; as=image"; 
            Response.Headers["Link"] += ",";
            Response.Headers["Link"] += "<"
                                        + infoHttpUrl +
                                        ">; rel=preload; crossorigin=\"use-credentials\"; as=fetch";
        }


    }
}
