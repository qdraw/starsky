using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;

namespace starsky.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        private readonly IQuery _query;
        private readonly IExiftool _exiftool;
        private readonly AppSettings _appSettings;

        public ApiController(IQuery query, IExiftool exiftool, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _query = query;
            _exiftool = exiftool;
        }
        

        // Used for end2end test
        [HttpGet]
        [HttpHead]
        [ResponseCache(Duration = 30 )]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous] /// <=================================
        public IActionResult Env()
        {
            return Json(_appSettings);
        }

        private bool _isReadOnly(string f)
        {
            if (_appSettings.ReadOnlyFolders == null) return false;
            
            var result = _appSettings.ReadOnlyFolders.FirstOrDefault(f.Contains);
            return result != null;
        }

        [HttpPost]
        public IActionResult Update(string tags, string colorClass,
            string captionAbstract, string f, string orientation, bool collections = true)
        {
            var detailView = _query.SingleItem(f,null,collections,false);
            if (detailView == null)
            {
                return NotFound("not in index " + f);
            }

            if (_isReadOnly(detailView.FileIndexItem.ParentDirectory)) return StatusCode(203, "read only");

            foreach (var collectionPath in detailView.FileIndexItem.CollectionPaths)
            {
                var fullPathCollection = _appSettings.DatabasePathToFilePath(collectionPath);
                Console.WriteLine(">> fullPathCollection" + fullPathCollection);
                
                //For the situation that the file is not on disk but the only one in the list
                if (!System.IO.File.Exists(fullPathCollection) 
                    && detailView.FileIndexItem.CollectionPaths.Count == 1)
                {
                    return NotFound("source image missing > "+ fullPathCollection);
                }
                // When there are more items in the list
                if (!System.IO.File.Exists(fullPathCollection))
                {
                    detailView.FileIndexItem.CollectionPaths.Remove(collectionPath);
                }
            }

            if (detailView.FileIndexItem.CollectionPaths.Count == 0)
            {
                return NotFound("source image missing");
            }

            // First create an update model
            var updateModel = new ExifToolModel();
            if (tags != null)
            {
                updateModel.Tags = tags;
            }

            if (captionAbstract != null)
            {
                updateModel.CaptionAbstract = captionAbstract;
            }

            // Parse ColorClass and add it
            detailView.FileIndexItem.SetColorClass(colorClass);
            updateModel.ColorClass = detailView.FileIndexItem.ColorClass;
            
            // Parse Rotation; do not add due errors with syncing
            var orientationEnum = new FileIndexItem().SetOrientation(orientation);
            if (orientationEnum != FileIndexItem.Rotation.DoNotChange)
            {
                // Anything else than do not change
                detailView.FileIndexItem.SetOrientation(orientation);
                updateModel.Orientation = detailView.FileIndexItem.Orientation;
            }
         
            

            var collectionFullPaths = _appSettings.DatabasePathToFilePath(detailView.FileIndexItem.CollectionPaths);
            var oldHashCodes = FileHash.GetHashCode(collectionFullPaths.ToArray());
                
            // Run as non-blocking task to avoid files not being updated or corrupt
            Task.Run(() => { _exiftool.Update(updateModel, collectionFullPaths); });

            var exifToolResultsList = new List<ExifToolModel>();
            for (int i = 0; i < detailView.FileIndexItem.CollectionPaths.Count; i++)
            {
                var singleItem = _query.SingleItem(detailView.FileIndexItem.CollectionPaths[i],null,false,false);
                
                //  var exifToolResult = _exiftool.Info(collectionFullPaths[i]);
                // for if exiftool does not anwer the request
                updateModel.SourceFile = Files.GetXmpSidecarFileWhenRequired(_appSettings.DatabasePathToFilePath(
                    detailView.FileIndexItem.FilePath), _appSettings.ExifToolXmpPrefix);;

                singleItem.FileIndexItem.Tags = updateModel.Tags;
                singleItem.FileIndexItem.Description = updateModel.CaptionAbstract;
                singleItem.FileIndexItem.ColorClass = updateModel.ColorClass;
                
                // Don't update this when it not has changed
                if (orientationEnum != FileIndexItem.Rotation.DoNotChange)
                {
                    singleItem.FileIndexItem.Orientation = updateModel.Orientation;
                }

                exifToolResultsList.Add(updateModel);

                singleItem.FileIndexItem.FileHash = FileHash.GetHashCode(collectionFullPaths[i]);
                // Rename Thumbnail
                new Thumbnail(_appSettings).RenameThumb(oldHashCodes[i], singleItem.FileIndexItem.FileHash);
                _query.UpdateItem(singleItem.FileIndexItem);
            }
            
            var getFullPathExifToolFileName = Files.GetXmpSidecarFileWhenRequired(_appSettings.DatabasePathToFilePath(
                detailView.FileIndexItem.FilePath), _appSettings.ExifToolXmpPrefix);
                
            return Json(exifToolResultsList.
                FirstOrDefault(p => p.SourceFile == getFullPathExifToolFileName));
        }   
        
        
//            var oldHashCodes = new List<string>();
//
//            var listOfSubPaths = new List<string> {f};
//            if (f.Contains(";"))
//            {
//                listOfSubPaths = ConfigRead.RemoveLatestDotComma(f).Split(";").ToList();
//            }
//
//            var singleItemList = new List<DetailView>();
//            foreach (var item in listOfSubPaths)
//            {
//                if (_isReadOnly(item)) return StatusCode(203,"read only");
//                var singleItem = _query.SingleItem(item);
//                if (singleItem == null) return NotFound("not in index " + item);
//                singleItemList.Add(singleItem);
//                
//                oldHashCodes.Add(singleItem.FileIndexItem.FileHash);
//                if (!System.IO.File.Exists(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
//                    return NotFound("source image missing " +
//                                    _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//            }
//
//            var updateModel = new ExifToolModel();
//            if (tags != null)
//            {
//                updateModel.Tags = tags;
//            }
//
//            if (captionAbstract != null)
//            {
//                updateModel.CaptionAbstract = captionAbstract;
//            }
//
//            var exiftoolPathsBuilder = new StringBuilder();
//            foreach (var singleItem in singleItemList)
//            {
//                // Enum get always one value and no null
//                singleItem.FileIndexItem.SetColorClass(colorClass);
//                updateModel.ColorClass = singleItem.FileIndexItem.ColorClass;
//
//                // Update Database with results
//                singleItem.FileIndexItem.FileHash =
//                    FileHash.GetHashCode(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//                singleItem.FileIndexItem.AddToDatabase = DateTime.Now;
//
//                exiftoolPathsBuilder.Append($"\"");
//                exiftoolPathsBuilder.Append(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)); 
//                exiftoolPathsBuilder.Append($"\"");
//                exiftoolPathsBuilder.Append($" "); // space
//                _query.UpdateItem(singleItem.FileIndexItem);
//            }
//            
//            // Run ExifTool updater
//            var exifToolResults = _exiftool.Update(updateModel,exiftoolPathsBuilder.ToString() );
//
//            for (int i = 0; i < singleItemList.Count; i++)
//            {
//                singleItemList[i].FileIndexItem.Tags = exifToolResults.Tags;
//                singleItemList[i].FileIndexItem.Description = exifToolResults.CaptionAbstract;
//                singleItemList[i].FileIndexItem.ColorClass = exifToolResults.ColorClass;
//                // Rename Thumbnail
//                new Thumbnail(_appSettings).RenameThumb(oldHashCodes[i], singleItemList[i].FileIndexItem.FileHash);
//            }


        [ResponseCache(Duration = 30, VaryByQueryKeys = new [] { "f" } )]
        public IActionResult Info(string f = "dbStyleFilepath")
        {
            if (f.Contains("?t=")) return NotFound("please use &t= instead of ?t=");
            
            if (_isReadOnly(f)) return StatusCode(203,"read only");
            
            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index");
            var fullFilePath = _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            if (!System.IO.File.Exists(fullFilePath))
                return NotFound("source image missing " +fullFilePath);

//            var getExiftool = _exiftool.Info(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
//            return Json(getExiftool);

            // Get Info from C# code
            var databaseItem = ExifRead.ReadExifFromFile(fullFilePath);
            databaseItem = new XmpReadHelper(_appSettings).XmpGetSidecarFile(databaseItem, fullFilePath);

            var infoModel = new ExifToolModel
            {
                CaptionAbstract = databaseItem.Description,
                ColorClass = databaseItem.ColorClass,
                Tags = databaseItem.Tags,
                Orientation = databaseItem.Orientation
            };
            return Json(infoModel);
        }

        [HttpDelete]
        public IActionResult Delete(string f = "dbStyleFilepath")
        {
            if (_isReadOnly(f)) return NotFound("afbeelding is in lees-only mode en kan niet worden verwijderd");

            var singleItem = _query.SingleItem(f,null,false,false);
            if (singleItem == null) return NotFound("not in index");
            if (!System.IO.File.Exists(_appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath)))
                return NotFound("source image missing " +
                                _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath));
            var item = _query.SingleItem(singleItem.FileIndexItem.FilePath,null,false,false).FileIndexItem;

            //  Remove Files if exist xmp file
            var fullFilePath = _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            var toDeletePaths =
                new List<string>
                {
                    fullFilePath,
                    fullFilePath.Replace(Path.GetExtension(fullFilePath), ".xmp"),
                    fullFilePath.Replace(Path.GetExtension(fullFilePath), ".XMP")
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

        [ResponseCache(Duration = 90000, VaryByQueryKeys = new [] { "f", "json", "retryThumbnail"} )]
        [HttpGet("/api/thumbnail/{f}")]
        [HttpHead("/api/thumbnail/{f}")]
        [IgnoreAntiforgeryToken]
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
            
            var thumbPath = _appSettings.ThumbnailTempFolder + f + ".jpg";

            if (Files.IsFolderOrFile(thumbPath) == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                // When a file is corrupt show error + Delete
                var imageFormat = Files.GetImageFormat(thumbPath);
                if (imageFormat == Files.ImageFormat.unknown)
                {
                    if (!retryThumbnail)
                    {
                        Console.WriteLine("image is corrupt");
                        SetExpiresResponseHeadersToZero();
                        return NoContent();
                    }
                    System.IO.File.Delete(thumbPath);
                }
                
                // When using the api to check using javascript
                // use the cached version of imageFormat, otherwise you have to check if it deleted
                if (imageFormat != Files.ImageFormat.unknown)
                {
                    if (json) return Json("OK");

                    // thumbs are always in jpeg
                    FileStream fs = System.IO.File.OpenRead(thumbPath);
                    return File(fs, "image/jpeg");
                }
            }
            
            
            var sourcePath = _query.GetItemByHash(f);
            if (sourcePath == null) return NotFound("not in index");
            
            var sourceFullPath = _appSettings.DatabasePathToFilePath(sourcePath);

            if (!System.IO.File.Exists(thumbPath) &&
                System.IO.File.Exists(sourceFullPath))
            {
                if (!isSingleitem)
                {
                    // "Photo exist in database but " + "isSingleItem flag is Missing"
                    SetExpiresResponseHeadersToZero();
                    Response.StatusCode = 409; // A conflict, that the thumb is not generated yet
                    return Json("Thumbnail is not ready yet");
                }
                
                var fileExtensionWithoutDot = Path.GetExtension(sourceFullPath).Remove(0, 1).ToLower();
                    
                if (Files.ExtensionThumbSupportedList.Contains(fileExtensionWithoutDot.ToLower()))
                {
                    FileStream fs1 = System.IO.File.OpenRead(sourceFullPath);
                    return File(fs1, MimeHelper.GetMimeType(fileExtensionWithoutDot));
                }
                Response.StatusCode = 409; // A conflict, that the thumb is not generated yet
                return Json("Thumbnail is not supported; for example you try to view a raw file");
            }

            return NotFound("There is no thumbnail image " + thumbPath + " and no source image "+ sourcePath );
            // When you have duplicate files and one of them is removed and there is no thumbnail generated yet you might get an false error
        }

        
        public void SetExpiresResponseHeadersToZero()
        {
            Request.HttpContext.Response.Headers.Remove("Cache-Control");
            Request.HttpContext.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            Request.HttpContext.Response.Headers.Remove("Pragma");
            Request.HttpContext.Response.Headers.Add("Pragma", "no-cache");

            Request.HttpContext.Response.Headers.Remove("Expires");
            Request.HttpContext.Response.Headers.Add("Expires", "0");
        }

        [HttpGet]
        [HttpHead]
        public IActionResult DownloadPhoto(string f, bool isThumbnail = true)
        {
            // f = subpath/filepath
            if (f.Contains("?isthumbnail")) return NotFound("please use &isthumbnail= instead of ?isthumbnail=");

            var singleItem = _query.SingleItem(f);
            if (singleItem == null) return NotFound("not in index " + f);

            var sourceFullPath = _appSettings.DatabasePathToFilePath(singleItem.FileIndexItem.FilePath);
            if (!System.IO.File.Exists(sourceFullPath))
                return NotFound("source image missing " + sourceFullPath );

            // Return full image
            if (!isThumbnail)
            {
                FileStream fs = System.IO.File.OpenRead(sourceFullPath);
                // Return the right mime type
                return File(fs, MimeHelper.GetMimeTypeByFileName(sourceFullPath));
            }

            // Return Thumbnail
            
            var thumbPath = _appSettings.ThumbnailTempFolder + singleItem.FileIndexItem.FileHash + ".jpg";

            // If File is corrupt delete it
            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.unknown)
            {
                System.IO.File.Delete(thumbPath);
            }

            if (Files.GetImageFormat(thumbPath) == Files.ImageFormat.notfound)
            {
                if (Files.IsFolderOrFile(_appSettings.ThumbnailTempFolder) ==
                    FolderOrFileModel.FolderOrFileTypeList.Deleted)
                {
                    return NotFound("Thumb base folder " + _appSettings.ThumbnailTempFolder + " not found");
                }
                
                var searchItem = new FileIndexItem
                {
                    FileName = _appSettings.FullPathToDatabaseStyle(sourceFullPath).Split("/").LastOrDefault(),
                    ParentDirectory = Breadcrumbs.BreadcrumbHelper(_appSettings.
                        FullPathToDatabaseStyle(sourceFullPath)).LastOrDefault(),
                    FileHash = FileHash.GetHashCode(sourceFullPath)
                };
                    
                var isSuccesCreateAThumb = new Thumbnail(_appSettings).CreateThumb(searchItem);
                if (!isSuccesCreateAThumb)
                {
                    Response.StatusCode = 500;
                    return Json("Thumbnail generation failed");
                }

                FileStream fs2 = System.IO.File.OpenRead(thumbPath);
                return File(fs2, "image/jpeg");
            }

            var getExiftool = _exiftool.Info(sourceFullPath);
            _exiftool.Update(getExiftool, sourceFullPath);

            FileStream fs1 = System.IO.File.OpenRead(thumbPath);
            return File(fs1, "image/jpeg");
        }

        [HttpGet]
        [HttpPost]
        public IActionResult RemoveCache(string f = "/", bool json = false)
        {
            //For folder paths only
            if (_appSettings.AddMemoryCache == false)
            {
                Response.StatusCode = 412;
                if(!json) return RedirectToAction("Index", "Home", new { f = f });
                return Json("cache disabled in config");
            }

            var singleItem = _query.SingleItem(f);
            if (singleItem != null && singleItem.IsDirectory)
            {
                var displayFileFolders = _query.DisplayFileFolders(f);
                _query.RemoveCacheParentItem(displayFileFolders,f);
                if(!json) return RedirectToAction("Index", "Home", new { f = f });
                return Json("cache succesfull cleared");
            }

            Response.StatusCode = 400;
            if(!json) return RedirectToAction("Index", "Home", new { f = f });
            return Json("ignored, please check if the 'f' path exist or use a folder string to clear the cache");
        }
        
        
    }
}
