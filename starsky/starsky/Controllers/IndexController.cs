using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Models;
using starsky.foundation.query.Interfaces;
using starsky.foundation.query.Services;
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
	    [ProducesResponseType(401)]
	    public IActionResult Index(
            string f = "/", 
            string colorClass = null,
            bool collections = true,
            bool hidedelete = true
            )
        {
            f = PathHelper.PrefixDbSlash(f);
            
            // Trick for avoiding spaces for behind proxy
            f = f.Replace("$20", " ");
            
            // Used in Detail and Index View => does not hide this single item
            var colorClassActiveList = new FileIndexItem().GetColorClassList(colorClass);
            var subpath = _query.SubPathSlashRemove(f);

            // First check if it is a single Item
            var singleItem = _query.SingleItem(subpath, colorClassActiveList,collections);
            // returns no object when it a directory
            
            if (singleItem?.IsDirectory == false)
            {
                return Json(singleItem);
            }
            
            // (singleItem.IsDirectory) or not found
            var directoryModel = new ArchiveViewModel
            {
                FileIndexItems = _query.DisplayFileFolders(subpath,colorClassActiveList,collections,hidedelete),
                ColorClassActiveList = 	colorClassActiveList,
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
                if ( f == "/" && queryIfFolder == null ) return Json(directoryModel);

                if (queryIfFolder == null) // used to have: singleItem?.FileIndexItem.FilePath == null &&
                {
                    Response.StatusCode = 404;
                    return Json("not found");
                }
            }
            
            // now update
            if (_appsettings != null) directoryModel.IsReadOnly = _appsettings.IsReadOnly(subpath);

            return Json(directoryModel);
        }

    }
}
