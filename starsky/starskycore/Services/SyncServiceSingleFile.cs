using System.Collections.Generic;
using starskycore.Helpers;
using starskycore.Models;

namespace starskycore.Services
{
    public partial class SyncService
    {
        // When input a direct file
        //        => if this file exist on the file system 
        //              => check if the hash in the db is up to date
        // Does not include parent folders

        // True is stop after
        // False is continue
        
        // Has support for subPaths in the index
        
        public enum SingleFileSuccess
        {
            Success,
            Fail,
            Ignore
        }
        
        public SingleFileSuccess SingleFile(string subPath = "")
        {
	        if ( !_iStorage.ExistFile(subPath) ) return SingleFileSuccess.Ignore;
	        
	        // File check if jpg #not corrupt
	        var imageFormat = ExtensionRolesHelper.GetImageFormat(_iStorage.ReadStream(subPath,160));
	        if(imageFormat == ExtensionRolesHelper.ImageFormat.unknown) return SingleFileSuccess.Fail;
                
	        // The same check as in GetFilesInDirectory
	        if (!ExtensionRolesHelper.IsExtensionSyncSupported(subPath)) return SingleFileSuccess.Fail;
                 
	        // single file -- update or adding
	        var dbListWithOneFile = new List<FileIndexItem>();
	        var dbItem = _query.GetObjectByFilePath(subPath);
	        if (dbItem != null)
	        {
		        // If file already exist in database
		        dbListWithOneFile.Add(dbItem);
	        }

	        var localListWithOneFileDbStyle = new List<string> {subPath};

	        CheckMd5Hash(localListWithOneFileDbStyle, dbListWithOneFile);
	        AddFileToDatabase(localListWithOneFileDbStyle, dbListWithOneFile);

	        // add subpath
	        AddSubPathFolder(subPath);
                
	        return SingleFileSuccess.Success;
        }
    }
}
