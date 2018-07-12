using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starsky.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly IQuery _query;

        public HomeController(IQuery query)
        {
            _query = query;
        }

        [HttpGet]
        [HttpHead]
        public IActionResult Index(
            string f = "/", 
            string colorClass = null,
            bool json = false
            )
        {

            // Trick for avoiding spaces for behind proxy
            f = f.Replace("$20", " ");
            
            // Used in Detail and Index View => does not hide this single item
            var colorClassFilterList = new FileIndexItem().GetColorClassList(colorClass);
            var subpath = _query.SubPathSlashRemove(f);
            
            var model = new ArchiveViewModel {FileIndexItems = _query.DisplayFileFolders(subpath,colorClassFilterList)};
            model.RelativeObjects = new RelativeObjects();
            model.Breadcrumb = Breadcrumbs.BreadcrumbHelper(subpath);
            
            var singleItem = _query.SingleItem(subpath,colorClassFilterList);

            if (!model.FileIndexItems.Any())
            {
                // is directory is emthy 
                var queryIfFolder = _query.GetObjectByFilePath(subpath);
                
                if (singleItem?.FileIndexItem.FilePath == null && queryIfFolder == null)
                {
                    if (f == "/" && !json) return View(model);
                    if (f == "/") return Json(model);

                    Response.StatusCode = 404;
                    if (json) return Json("not found");
                    return View("Error");
                }
            }

            if (singleItem?.FileIndexItem.FilePath != null)
            {
                if (json) return Json(singleItem);
                return View("SingleItem", singleItem);
            }
            
            model.SearchQuery = subpath.Split("/").LastOrDefault();                
            model.RelativeObjects = _query.GetNextPrevInFolder(subpath);
            
            if (json) return Json(model);
            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
