using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;
using starsky.ViewModels;
using starskycore.Models;

namespace starskycore.Helpers
{
    public class StatusCodesHelper
    {
        private readonly AppSettings _appSettings;

        public StatusCodesHelper(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        
        /// <summary>
        /// Check the status of a file based on DetailView object
        /// </summary>
        /// <param name="detailView">The element used on the web</param>
        /// <returns>ExifStatus enum</returns>
        public FileIndexItem.ExifStatus FileCollectionsCheck(DetailView detailView)
        {
            if(_appSettings == null) throw new DllNotFoundException("add app settings to ctor");
            
            if (detailView == null)
            {
                return FileIndexItem.ExifStatus.NotFoundNotInIndex;
            }

            if (detailView.IsDirectory && _appSettings.IsReadOnly(detailView.SubPath))
            {
                return FileIndexItem.ExifStatus.DirReadOnly;
            }

            if (detailView.IsDirectory)
            {
                return FileIndexItem.ExifStatus.NotFoundIsDir;
            }

            if (_appSettings.IsReadOnly(detailView.FileIndexItem.ParentDirectory)) return  FileIndexItem.ExifStatus.ReadOnly;

            foreach (var collectionPath in detailView.FileIndexItem.CollectionPaths.ToList())
            {
	            // toList() is add to avoid :> Collection was modified; enumeration operation
                var fullPathCollection = _appSettings.DatabasePathToFilePath(collectionPath);
                
                //For the situation that the file is not on disk but the only one in the list
                if (!System.IO.File.Exists(fullPathCollection) 
                    && detailView.FileIndexItem.CollectionPaths.Count == 1)
                {
                    return FileIndexItem.ExifStatus.NotFoundSourceMissing;  //
                }
                // When there are more items in the list
                if (!System.IO.File.Exists(fullPathCollection))
                {
                    detailView.FileIndexItem.CollectionPaths.Remove(collectionPath);
                }
            }

            if (detailView.FileIndexItem.CollectionPaths.Count == 0)
            {
                return FileIndexItem.ExifStatus.NotFoundSourceMissing;
            }

            return FileIndexItem.ExifStatus.Ok;
        }
        
        /// <summary>
        /// Does deside if the loop should be stopped, true = stop
        /// Uses FileCollectionsCheck
        /// Add for all types exept for OK!
        /// </summary>
        /// <param name="statusModel">the main object to return later</param>
        /// <param name="statusResults">the status by FileCollectionsCheck</param>
        /// <param name="fileIndexResultsList">list of object that will be returned</param>
        /// <returns>If true skip the next code</returns>
        public bool ReturnExifStatusError(FileIndexItem statusModel, 
            FileIndexItem.ExifStatus statusResults, List<FileIndexItem> fileIndexResultsList)
        {
            switch (statusResults)
            {
                case FileIndexItem.ExifStatus.NotFoundIsDir:
                    statusModel.IsDirectory = true;
                    statusModel.Status = FileIndexItem.ExifStatus.NotFoundIsDir;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.DirReadOnly:
                    statusModel.IsDirectory = true;
                    statusModel.Status = FileIndexItem.ExifStatus.DirReadOnly;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.NotFoundNotInIndex:
                    statusModel.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.NotFoundSourceMissing:
                    statusModel.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
                    fileIndexResultsList.Add(statusModel);
                    return true;
                case FileIndexItem.ExifStatus.ReadOnly:
                    statusModel.Status = FileIndexItem.ExifStatus.ReadOnly;
                    fileIndexResultsList.Add(statusModel);
                    return true;
            }
            return false;
        }
    }
}