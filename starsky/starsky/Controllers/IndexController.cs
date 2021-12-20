using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskycore.ViewModels;

namespace starsky.Controllers
{
    [Authorize]
    public class IndexController : Controller
    {
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;

        public IndexController(IQuery query, AppSettings appSettings)
        {
            _query = query;
            _appSettings = appSettings;
        }
        
	    /// <summary>
	    /// The database-view of a directory
	    /// </summary>
	    /// <param name="f">subPath</param>
	    /// <param name="colorClass">filter on colorClass (use int)</param>
	    /// <param name="collections">to combine files with the same name before the extension</param>
	    /// <param name="hidedelete">ignore deleted files</param>
	    /// <param name="sort">how to orderBy, defaults to fileName</param>
	    /// <returns></returns>
	    /// <response code="200">returns a list of items from the database</response>
	    /// <response code="404">subPath not found in the database</response>
	    /// <response code="401">User unauthorized</response>
	    [HttpGet("/api/index")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(ArchiveViewModel),200)]
		[ProducesResponseType(404)]
	    [ProducesResponseType(401)]
	    public IActionResult Index(
            string f = "/", 
            string colorClass = null,
            bool collections = true,
            bool hidedelete = true,
            SortType sort = SortType.FileName
            )
        {
            
            // Used in Detail and Index View => does not hide this single item
            var colorClassActiveList = new FileIndexItem().GetColorClassList(colorClass);

            var subPath = PathHelper.PrefixDbSlash(f);
            subPath = PathHelper.RemoveLatestSlash(subPath);
            if ( string.IsNullOrEmpty(subPath) ) subPath = "/";

            // First check if it is a single Item
            var singleItem = _query.SingleItem(subPath, colorClassActiveList,collections,hidedelete, sort);
            // returns no object when it a directory
            
            if (singleItem?.IsDirectory == false)
            {
	            singleItem.IsReadOnly = _appSettings.IsReadOnly(singleItem.FileIndexItem.ParentDirectory);
                return Json(singleItem);
            }

            var fileIndexItems = SortHelper.Helper(
	            _query.DisplayFileFolders(subPath, colorClassActiveList,
		            collections, hidedelete), sort).ToList();
            var fileIndexItemsWithoutCollections = _query.DisplayFileFolders(
	            subPath, null, false, hidedelete).ToList();
            
            // (singleItem.IsDirectory) or not found
            var directoryModel = new ArchiveViewModel
            {
                FileIndexItems = fileIndexItems,
                ColorClassActiveList = 	colorClassActiveList,
                RelativeObjects = _query.GetNextPrevInFolder(subPath), // Args are not shown in this view
                Breadcrumb = Breadcrumbs.BreadcrumbHelper(subPath),
                SearchQuery = subPath.Split("/").LastOrDefault(),
                SubPath = subPath,
                CollectionsCount = fileIndexItemsWithoutCollections.
	                Count(p => p.IsDirectory == false),
                // when change colorclass selection you should see all options
                ColorClassUsage = fileIndexItemsWithoutCollections
	                .Select( p => p.ColorClass).Distinct()
	                .OrderBy(p => (int) (p)).ToList(),
                IsReadOnly =  _appSettings.IsReadOnly(subPath),
                Collections = collections,
            };

            if (singleItem == null)
            {
                // For showing a new database
                var queryIfFolder = _query.GetObjectByFilePath(subPath);

                // For showing a new database
                if ( f == "/" && queryIfFolder == null ) return Json(directoryModel);

                if (queryIfFolder == null) // used to have: singleItem?.FileIndexItem.FilePath == null &&
                {
                    Response.StatusCode = 404;
                    return Json("not found");
                }
            }

            return Json(directoryModel);
        }

    }
}
