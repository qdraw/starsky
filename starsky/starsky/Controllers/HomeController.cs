using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;

namespace starsky.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUpdate _updateStatusContent;

        public HomeController(IUpdate updateStatusContent)
        {
            _updateStatusContent = updateStatusContent;
        }

        public IActionResult Index(string f = "/")
        {
            var model = _updateStatusContent.GetObjectItems(f);
            var firstItem = model.FirstOrDefault();

            if (firstItem != null && !firstItem.IsFolder && model.Count() == 1)
            {
                return View("SingleItem", firstItem);
            }
            return View(model);
        }

        public IActionResult Thumbnail(string f)
        {
            var path = _updateStatusContent.GetItemByHash(f);
            //path = Files.PathToFull(path);
            if (path == null) return BadRequest();

            path = AppSettingsProvider.ThumbnailTempFolder + f + ".jpg";

            var image = System.IO.File.OpenRead(path);
            return File(image, "image/jpeg");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult SyncFiles()
        {
            return Json(_updateStatusContent.SyncFiles());
        }

        //public IActionResult Update()
        //{
        //    var item = new FileIndexItem();
        //    item.FileName = "item";
        //    item.FilePath = "i";
        //    _updateStatusContent.AddItem(item);
        //    return Json(item);
        //}

        public IActionResult GetFolder(string p = "/")
        {
            var i = _updateStatusContent.GetChildFolders(p);

            return Json(i);
        }

        public IActionResult GetFilesInFolder(string p = "/")
        {
            var i = _updateStatusContent.GetFilesInFolder(p);

            return Json(i);
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
