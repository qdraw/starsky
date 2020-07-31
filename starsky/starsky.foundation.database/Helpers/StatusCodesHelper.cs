using System;
using System.Collections.Generic;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;

namespace starsky.foundation.database.Helpers
{
    public class StatusCodesHelper
    {
        private readonly AppSettings _appSettings;

	    public StatusCodesHelper(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

	    public StatusCodesHelper()
	    {
	    }

	    public FileIndexItem.ExifStatus IsReadOnlyStatus(DetailView detailView)
	    {
		    if(_appSettings == null) throw new DllNotFoundException("add app settings to ctor");

		    if ( detailView == null )
		    {
			    return FileIndexItem.ExifStatus.Default;
		    }
		    
		    if (detailView.IsDirectory && _appSettings.IsReadOnly(detailView.SubPath))
		    {
			    return FileIndexItem.ExifStatus.DirReadOnly;
		    }

		    if ( _appSettings.IsReadOnly(detailView.FileIndexItem.ParentDirectory) )
		    {
			    return  FileIndexItem.ExifStatus.ReadOnly;
		    }

		    return FileIndexItem.ExifStatus.Default;
	    }

	    public FileIndexItem.ExifStatus IsDeletedStatus(DetailView detailView)
	    {
		    if (detailView.FileIndexItem.Tags.Contains("!delete!"))
		    {
			    return FileIndexItem.ExifStatus.Deleted;
		    }

		    return FileIndexItem.ExifStatus.Default;
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
                // case FileIndexItem.ExifStatus.NotFoundIsDir:
                //     statusModel.IsDirectory = true;
                //     statusModel.Status = FileIndexItem.ExifStatus.NotFoundIsDir;
                //     fileIndexResultsList.Add(statusModel);
                //     return true;
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
                case FileIndexItem.ExifStatus.OperationNotSupported:
	                statusModel.Status = FileIndexItem.ExifStatus.OperationNotSupported;
	                fileIndexResultsList.Add(statusModel);
	                return true;
                case FileIndexItem.ExifStatus.ExifWriteNotSupported:
	                statusModel.Status = FileIndexItem.ExifStatus.ExifWriteNotSupported;
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
