using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starsky.Controllers
{
    public class ApiController : Controller
    {
        private readonly IQuery _query;

        public ApiController(IQuery query)
        {
            _query = query;
        }

        [HttpGet]
        public IActionResult Folder(string f = "/",
            string colorClass = null)
        {
            // http://localhost:5000/api/folder?f=/2018/01/2018_01_01&colorClass=1,2

            var colorClassFilterList = new FileIndexItem().GetColorClassList(colorClass);

            var fileIndexItems = _query.DisplayFileFolders(f, colorClassFilterList);
            if (!fileIndexItems.Any())
            {
                // is directory is emthy 
                var queryIfFolder = _query.GetObjectByFilePath(f);

                if (queryIfFolder == null)
                {
                    Response.StatusCode = 404;
                    return View("Error");
                }
            }

            return Json(fileIndexItems);
        }

        // Used for end2end test
        [HttpGet]
        public IActionResult Env()
        {
            var model = new EnvViewModel
            {
                DatabaseType = AppSettingsProvider.DatabaseType,
                BasePath = AppSettingsProvider.BasePath,
                ExifToolPath = AppSettingsProvider.ExifToolPath,
                ThumbnailTempFolder = AppSettingsProvider.ThumbnailTempFolder,
            };
            if (AppSettingsProvider.DatabaseType != AppSettingsProvider.DatabaseTypeList.Mysql)
            {
                model.DbConnectionString = AppSettingsProvider.DbConnectionString;
            }

            return Json(model);
        }

        [HttpPost]
        public IActionResult Update(string tags, string colorClass, string f = "dbStylePath")
        {
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index " + f);
            var oldHashCode = singleItem.FileIndexItem.FileHash;
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));

            var updateModel = new ExifToolModel();

            Console.WriteLine("tags>>>>");
            Console.WriteLine(tags);
            if (tags != null)
            {
                updateModel.Tags = tags;
            }

            // Enum get always one value and no null
            singleItem.FileIndexItem.SetColorClass(colorClass);
            updateModel.ColorClass = singleItem.FileIndexItem.ColorClass;

            // Run ExifTool updater
            var exifToolResults = ExifTool.Update(updateModel,
                FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));

            // Update Database with results
            singleItem.FileIndexItem.FileHash =
                FileHash.GetHashCode(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            singleItem.FileIndexItem.AddToDatabase = DateTime.Now;
            singleItem.FileIndexItem.Tags = exifToolResults.Tags;
            singleItem.FileIndexItem.ColorClass = exifToolResults.ColorClass;
            _query.UpdateItem(singleItem.FileIndexItem);

            // Rename Thumbnail
            new Thumbnail().RenameThumb(oldHashCode, singleItem.FileIndexItem.FileHash);


            return Json(exifToolResults);
        }

//        [HttpPost]
//        [HttpGet]
//        public IActionResult UpdateTag(string f = "path", string t = "", bool redirect = true)
//        {
//            var singleItem = _query.SingleItem(f);
//            if (singleItem == null) return NotFound("not in index " +  f);
//
//            if (string.IsNullOrWhiteSpace(t)) return BadRequest("tag label missing");
//
//            var oldHashCode = _query.SingleItem(f).FileIndexItem.FileHash;
//
//            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
//                return NotFound("source image missing " + FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//
//            var exifToolResult = ExifTool.SetExifToolKeywords(t, FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//            if (exifToolResult == null) return BadRequest();
//
//            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;
//
//            item.FileHash = FileHash.CalcHashCode(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//            item.AddToDatabase = DateTime.Now;
//            item.Tags = exifToolResult;
//            _query.UpdateItem(item);
//
//            new Thumbnail().RenameThumb(oldHashCode, item.FileHash);
//
//            // for using the api
//            if (redirect)
//            {
//                return RedirectToAction("index", "home", new { f = f, t = exifToolResult });
//            }
//            return Json(item);
//        }

        public IActionResult Info(string f = "dbStyleFilepath")
        {
            if (f.Contains("?t=")) return NotFound("please use &t= instead of ?t=");
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            var getExiftool = ExifTool.Info(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            return Json(getExiftool);
        }

        [HttpDelete]
        public IActionResult Delete(string f = "dbStyleFilepath")
        {
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            System.IO.File.Delete(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            _query.RemoveItem(item);

            return Json(item);
        }

        public IActionResult Thumbnail(string f, bool isSingleitem = false)
        {
            var sourcePath = _query.GetItemByHash(f);

            if (sourcePath == null) return NotFound("not in index");

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + f + ".jpg";

            // If File is corrupt delete it;
            if(System.IO.File.Exists(thumbPath)) {
                if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
                {
                    System.IO.File.Delete(thumbPath);
                }
            }

            if (!System.IO.File.Exists(thumbPath) &&
                System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(sourcePath)))
            {
                if (!isSingleitem)
                {
                    return NotFound("could regenerate thumb");
                }

                try
                {
                    var searchItem = new FileIndexItem
                    {
                        FilePath = sourcePath,
                        FileHash = f
                    };
                    Services.Thumbnail.CreateThumb(searchItem);
                }
                catch (FileNotFoundException)
                {
                    return NotFound("Thumb base folder not found");
                }

            }

            if (!System.IO.File.Exists(thumbPath))
            {
                return NotFound("in cache but not in thumbdb");
            }

            FileStream fs = System.IO.File.OpenRead(thumbPath);
            return File(fs, "image/jpeg");
        }

        public IActionResult DownloadPhoto(string f, bool isThumbnail = true){
            // f = filePath
            
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");

            var sourcePath = FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            if (!System.IO.File.Exists(sourcePath))
                return NotFound("source image missing " + sourcePath );

            // Return full image
            if (!isThumbnail)
            {
                FileStream fs = System.IO.File.OpenRead(sourcePath);
                return File(fs, "image/jpeg");
            }

            // Return Thumbnail
         
            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + singleItem.FileIndexItem.FileHash + ".jpg";

            // If File is corrupt delete it;
            if(System.IO.File.Exists(thumbPath)) {
                if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
                {
                    System.IO.File.Delete(thumbPath);
                }
            }
            
            if (!System.IO.File.Exists(thumbPath)){
                return NotFound("Thumb not in cache");
            }
            var getExiftool = ExifTool.Info(sourcePath);
            ExifTool.Update(getExiftool, sourcePath);
            
            FileStream fs1 = System.IO.File.OpenRead(thumbPath);
            return File(fs1, "image/jpeg");
                
        }

}
}
