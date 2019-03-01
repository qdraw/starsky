using System.Collections.Generic;
using System.IO;
using starskycore.Models;

namespace starskycore.Helpers
{
    public class PlainTextFileHelper
    {
	    
	    /// <summary>
	    /// Return the content of the first file
	    /// </summary>
	    /// <param name="fullFilePaths"></param>
	    /// <returns></returns>
	    public virtual string ReadFirstFile(List<string> fullFilePaths)
	    {

		    foreach ( var singleFilePath in fullFilePaths )
		    {

			    if ( FilesHelper.IsFolderOrFile(singleFilePath) ==
			         FolderOrFileModel.FolderOrFileTypeList.File )
			    {
				    return new PlainTextFileHelper().ReadFile(singleFilePath);
			    }
		    }

		    return string.Empty;
	    }
	    
        /// <summary>
        /// Read a text based file (not binary) file
        /// </summary>
        /// <param name="fullFilePath">path on filesystem</param>
        /// <returns>content of the file as string</returns>
        public virtual string ReadFile(string fullFilePath)
        {
            if (!File.Exists(fullFilePath)) return string.Empty;
            
            return File.ReadAllText(fullFilePath);
        }
        
        /// <summary>
        /// Write and create a new plain text file to the filesystem
        /// </summary>
        /// <param name="fullFilePath">path on filesystem</param>
        /// <param name="writeString">content of the file</param>
        public virtual void WriteFile(string fullFilePath, string writeString)
        {
            if (File.Exists(fullFilePath)) return;
            
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(fullFilePath)) 
            {
                sw.WriteLine(writeString);
            }
        }

    }
}
