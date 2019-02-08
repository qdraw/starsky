using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace starskycore.Helpers
{
	public class Zipper
	{
		public Zipper()
		{
			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="zipInputFullPath">input e.g: /path/file.zip</param>
		/// <param name="storeZipFolderFullPath">output e.g. /folder/</param>
		/// <returns></returns>
		public string ExtractZip( string zipInputFullPath, string storeZipFolderFullPath)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// To Create the zip file in the storeZipFolderFullPath folder
		/// </summary>
		/// <param name="storeZipFolderFullPath">folder to create zip in</param>
		/// <param name="filePaths">list of full file paths</param>
		/// <param name="fileNames">list of filenames</param>
		/// <param name="zipOutputFilename">to name of the zip file (zipHash)</param>
		/// <returns>a zip in the temp folder</returns>
		public string CreateZip(string storeZipFolderFullPath, List<string> filePaths, 
			List<string> fileNames, string zipOutputFilename)
		{

			var tempFileFullPath = Path.Combine(storeZipFolderFullPath,zipOutputFilename) + ".zip";

			if(System.IO.File.Exists(tempFileFullPath))
			{
				return tempFileFullPath;
			}
			
			ZipArchive zip = ZipFile.Open(tempFileFullPath, ZipArchiveMode.Create);

			for ( int i = 0; i < filePaths.Count; i++ )
			{
				if ( System.IO.File.Exists(filePaths[i]) )
				{
					zip.CreateEntryFromFile(filePaths[i], fileNames[i]);
				}
			}
			zip.Dispose();
			return tempFileFullPath;
		}
	}
}
