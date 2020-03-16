﻿using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.query.Models;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.ViewModels;

namespace starskycore.Helpers
{
    public class StatusCodesHelper
    {
        private readonly AppSettings _appSettings;
	    private readonly IStorage _iStorage;

	    public StatusCodesHelper(AppSettings appSettings, IStorage iStorage)
        {
            _appSettings = appSettings;
	        _iStorage = iStorage;
        }

	    public StatusCodesHelper()
	    {
	    }

	    /// <summary>
        /// Check the status of a file based on DetailView object
        /// </summary>
        /// <param name="detailView">The element used on the web</param>
        /// <returns>ExifStatus enum</returns>
        public FileIndexItem.ExifStatus FileCollectionsCheck(DetailView detailView)
        {
            if(_appSettings == null) throw new DllNotFoundException("add app settings to ctor");
	        if(_iStorage == null) throw new DllNotFoundException("add _iStorage to ctor");

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

            foreach (var collectionSubPath in detailView.FileIndexItem.CollectionPaths.ToList())
            {
	            //For the situation that the file is not on disk but the only one in the list
                if (! _iStorage.ExistFile(collectionSubPath)
                    && detailView.FileIndexItem.CollectionPaths.Count == 1)
                {
                    return FileIndexItem.ExifStatus.NotFoundSourceMissing;
                }
                // When there are more items in the list
                if (!_iStorage.ExistFile(collectionSubPath) )
                {
                    detailView.FileIndexItem.CollectionPaths.Remove(collectionSubPath);
                }
            }
            
            if (detailView.FileIndexItem.Tags.Contains("!delete!"))
            {
	            return FileIndexItem.ExifStatus.Deleted;
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
        /// Add for all types exept for OK/Readonly!
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
            }
            return false;
        }

        public bool ReadonlyDenied(FileIndexItem statusModel,
	        FileIndexItem.ExifStatus statusResults, List<FileIndexItem> fileIndexResultsList)
        {
	        switch (statusResults)
	        {
		        case FileIndexItem.ExifStatus.ReadOnly:
			        statusModel.Status = FileIndexItem.ExifStatus.ReadOnly;
			        fileIndexResultsList.Add(statusModel);
			        return true;
	        }
	        return false;
        }

        public void ReadonlyAllowed(FileIndexItem statusModel,
	        FileIndexItem.ExifStatus statusResults, List<FileIndexItem> fileIndexResultsList)
        {
	        // Readonly is allowed
	        if ( statusResults != FileIndexItem.ExifStatus.ReadOnly ) return;
	        
	        statusModel.Status = FileIndexItem.ExifStatus.ReadOnly;
	        fileIndexResultsList.Add(statusModel);
        }

        
    }
}
