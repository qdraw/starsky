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

        // Used for end2end test
        [HttpGet]
        [HttpHead]
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

        private bool _isReadOnly(string f)
        {
            if (AppSettingsProvider.ReadOnlyFolders == null) return false;
            
            var result = AppSettingsProvider.ReadOnlyFolders.FirstOrDefault(f.Contains);
            return result != null;
        }
        
        [HttpPost]
        public IActionResult Update(string tags, string colorClass, string captionAbstract, string f = "dbStylePath")
        {
            if (_isReadOnly(f)) return NotFound("read only");
            
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

            if (captionAbstract != null)
            {
                updateModel.CaptionAbstract = captionAbstract;
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
            singleItem.FileIndexItem.Description = exifToolResults.CaptionAbstract;
            singleItem.FileIndexItem.ColorClass = exifToolResults.ColorClass;
            _query.UpdateItem(singleItem.FileIndexItem);

            // Rename Thumbnail
            new Thumbnail().RenameThumb(oldHashCode, singleItem.FileIndexItem.FileHash);


            return Json(exifToolResults);
        }

        public IActionResult Info(string f = "dbStyleFilepath")
        {
            if (f.Contains("?t=")) return NotFound("please use &t= instead of ?t=");
            
            if (_isReadOnly(f)) return NotFound("read only");
            
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));

            var getExiftool = ExifTool.Info(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            return Json(getExiftool);
        }

        [HttpDelete]
        public IActionResult Delete(string f = "dbStyleFilepath")
        {
            if (_isReadOnly(f)) return NotFound("afbeelding is in lees-only mode en kan niet worden verwijderd");

            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath).FileIndexItem;

            //  Remove Files if exist and RAW file
            var fullFilePath = FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            var toDeletePaths =
                new List<string>
                {
                    fullFilePath,
                    fullFilePath.Replace(".jpg", ".arw"), 
                    fullFilePath.Replace(".jpg", ".dng"),
                    fullFilePath.Replace(".jpg", ".ARW"), 
                    fullFilePath.Replace(".jpg", ".DNG"),
                    fullFilePath.Replace(".jpg", ".xmp"),
                    fullFilePath.Replace(".jpg", ".XMP")
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

            // When a file is corrupt show error + Delete
            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
            {
                if (!retryThumbnail)
                {
                    Console.WriteLine("image is corrupt");
                    return NoContent();
                }
                System.IO.File.Delete(thumbPath);
            }


            if (!System.IO.File.Exists(thumbPath) &&
                System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(sourcePath)))
            {
                if (!isSingleitem)
                {
                    return NotFound("Photo exist in database but " +
                                    "isSingleItem flag is Missing");
                }
                FileStream fs1 = System.IO.File.OpenRead(FileIndexItem.DatabasePathToFilePath(sourcePath));
                return File(fs1, "image/jpeg");
            }

            if (!System.IO.File.Exists(thumbPath) && 
                !System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(sourcePath)))
            {
                return NotFound("There is no thumbnail image and no source image");
            }
            
            // When using the api to check using javascript
            if (json) return Json("OK");

            FileStream fs = System.IO.File.OpenRead(thumbPath);
            return File(fs, "image/jpeg");
        }

        public IActionResult DownloadPhoto(string f, bool isThumbnail = true){
            // f = filePath
            if (f.Contains("?isthumbnail")) return NotFound("please use &isthumbnail= instead of ?isthumbnail=");

            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index " + f);

            var sourceFullPath = FileIndexItem.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            if (!System.IO.File.Exists(sourceFullPath))
                return NotFound("source image missing " + sourceFullPath );

            // Return full image
            if (!isThumbnail)
            {
                FileStream fs = System.IO.File.OpenRead(sourceFullPath);
                return File(fs, "image/jpeg");
            }

            // Return Thumbnail
            
            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + singleItem.FileIndexItem.FileHash + ".jpg";

            // If File is corrupt delete it
            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
            {
                System.IO.File.Delete(thumbPath);
            }

            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.notfound)
            {
                try
                {
                    var searchItem = new FileIndexItem
                    {
                        FilePath = FileIndexItem.FullPathToDatabaseStyle(sourceFullPath),
                        FileHash = FileHash.GetHashCode(sourceFullPath)
                    };
                    
                    Services.Thumbnail.CreateThumb(searchItem);

                    FileStream fs2 = System.IO.File.OpenRead(thumbPath);
                    return File(fs2, "image/jpeg");

                }
                catch (FileNotFoundException)
                {
                    return NotFound("Thumb base folder not found");
                }
            }

            var getExiftool = ExifTool.Info(sourceFullPath);
            ExifTool.Update(getExiftool, sourceFullPath);

            FileStream fs1 = System.IO.File.OpenRead(thumbPath);
            return File(fs1, "image/jpeg");
        }
    }
}
