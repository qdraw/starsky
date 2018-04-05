using System;
using System.Collections.Generic;
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

            // Remove Files if exist // +++ RAW file
            var fullFilePath = FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            var toDeletePaths =
                new List<string>
                {
                    fullFilePath,
                    fullFilePath.Replace(".jpg", ".arw"), 
                    fullFilePath.Replace(".jpg", ".dng"),
                    fullFilePath.Replace(".jpg", ".ARW"), 
                    fullFilePath.Replace(".jpg", ".DNG")
                };

            foreach (var toDelPath in toDeletePaths)
            {
                if (System.IO.File.Exists(toDelPath))
                {
                    System.IO.File.Delete(toDelPath);
                }
            }
            // End Remove files
            
            _query.RemoveItem(item);

            return Json(item);
        }

        public IActionResult Thumbnail(
            string f, 
            bool isSingleitem = false, 
            bool json = false,
            bool retryThumbnail = false)
        {
            // f is Hash
            // isSingleItem => detailview
            // Retry thumbnail => is when you press reset thumbnail
            // json, => to don't waste the users bandwith.
            
            var sourcePath = _query.GetItemByHash(f);

            if (sourcePath == null) return NotFound("not in index");

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + f + ".jpg";

            // If File is corrupt delete it;
            if(System.IO.File.Exists(thumbPath)) {
                if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
                {
                    if (!retryThumbnail)
                    {
                        Console.WriteLine("image is corrupt !corrupt! ");
                        return NoContent();
                    }
                    System.IO.File.Delete(thumbPath);
                }
            }

            if (!System.IO.File.Exists(thumbPath) &&
                System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(sourcePath)))
            {
                if (!isSingleitem)
                {
                    return NotFound("Photo exist in database but " +
                                    "isSingleItem flag is Missing");
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
            
            // When using the api to check using javascript
            if (json) return Json("OK");

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
