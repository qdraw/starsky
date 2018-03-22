using System;
using System.Diagnostics;
using System.IO;
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
        [HttpGet]
        public IActionResult Update(string f = "path", string t = "", bool redirect = true)
        {
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index " +  f);

            if (string.IsNullOrWhiteSpace(t)) return BadRequest("tag label missing");

            var oldHashCode = _query.SingleItem(f).FileIndexItem.FileHash;

            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " + FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));

            var exifToolResult = ExifTool.SetExifToolKeywords(t, FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            if (exifToolResult == null) return BadRequest();

            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            item.FileHash = FileHash.CalcHashCode(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            item.AddToDatabase = DateTime.Now;
            item.Tags = exifToolResult;
            _query.UpdateItem(item);

            new Thumbnail().RenameThumb(oldHashCode, item.FileHash);

            // for using the api
            if (redirect)
            {
                return RedirectToAction("index", "home", new { f = f, t = exifToolResult });
            }
            return Json(item);
        }

        public IActionResult Info(string f = "dbStyleFilepath", string t = "")
        {
            if (f.Contains("?t=")) return NotFound("please use &t= instead of ?t=");
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (string.IsNullOrWhiteSpace(t)) return BadRequest("tag label missing");
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " + FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            var getExiftool = ExifTool.ReadExifToolKeywords(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            if (item.Tags == null) item.Tags = string.Empty;
            //if (item.Tags != getExiftool)
            //{
            //    Response.StatusCode = 205;
            //}
            item.Tags = getExiftool;
            return Json(item);
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

            if (!System.IO.File.Exists(thumbPath) && System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(sourcePath)))
            {
                if (!isSingleitem)
                {
                    return NotFound("could regenerate thumb");
                }

                try
                {
                    var searchItem = new FileIndexItem();
                    searchItem.FilePath = sourcePath;
                    searchItem.FileHash = f;
                    Services.Thumbnail.CreateThumb(searchItem);
                }
                catch (FileNotFoundException)
                {
                    return NotFound("Thumb base folder not found");

                }

            };

            if (!System.IO.File.Exists(thumbPath))
            {
                return NotFound("in cache but not in thumbdb");
            }

            FileStream fs = System.IO.File.OpenRead(thumbPath);
            return File(fs, "image/jpeg");

        }

        public IActionResult Error()
        {
            return View( new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
