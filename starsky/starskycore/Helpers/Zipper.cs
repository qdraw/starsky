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
		/// Extract zip file to a folder
		/// </summary>
		/// <param name="zipInputFullPath">input e.g: /path/file.zip</param>
		/// <param name="storeZipFolderFullPath">output e.g. /folder/</param>
		/// <returns></returns>
		public void ExtractZip( string zipInputFullPath, string storeZipFolderFullPath)
		{
			// todo: todo: implement this comma separated list  >> string matchExtensions = "*" 
			// Ensures that the last character on the extraction path
			// is the directory separator char. 
			// Without this, a malicious zip file could try to traverse outside of the expected
			// extraction path.
			storeZipFolderFullPath = PathHelper.AddBackslash(storeZipFolderFullPath);
			using ( ZipArchive archive = ZipFile.OpenRead(zipInputFullPath) )
			{
				foreach ( ZipArchiveEntry entry in archive.Entries )
				{
					// Gets the full path to ensure that relative segments are removed.
					string destinationPath = Path.GetFullPath(Path.Combine(storeZipFolderFullPath, entry.FullName));

					// Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
					// are case-insensitive.
					if (destinationPath.StartsWith(storeZipFolderFullPath, StringComparison.Ordinal))
						entry.ExtractToFile(destinationPath);
				}
			}
			

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

			if(File.Exists(tempFileFullPath))
			{
				return tempFileFullPath;
			}
			
			ZipArchive zip = ZipFile.Open(tempFileFullPath, ZipArchiveMode.Create);

			for ( int i = 0; i < filePaths.Count; i++ )
			{
				if ( File.Exists(filePaths[i]) )
				{
					zip.CreateEntryFromFile(filePaths[i], fileNames[i]);
				}
			}
			zip.Dispose();
			return tempFileFullPath;
		}
	}
}
