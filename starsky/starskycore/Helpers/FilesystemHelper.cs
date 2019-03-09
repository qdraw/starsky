using System.Collections.Generic;
using System.IO;

namespace starskycore.Helpers
{
	public class FilesystemHelper
	{
		public IEnumerable<string> GetAllFilesInDirectory(string fullFilePath)
		{
			string[] allFiles = Directory.GetFiles(fullFilePath);

			var imageFilesList = new List<string>();
			foreach (var file in allFiles)
			{
				// Path.GetExtension uses (.ext)
				// the same check in SingleFile
				// Recruisive >= same check
				// ignore Files with ._ names, this is Mac OS specific
				var isAppleDouble = Path.GetFileName(file).StartsWith("._");
				if (!isAppleDouble)
				{
					imageFilesList.Add(file);
				}
				// to filter use:
				// ..etAllFilesInDirectory(subPath)
				//	.Where(ExtensionRolesHelper.IsExtensionExifToolSupported)
			}

			return imageFilesList;
		}

		public IEnumerable<string> GetDirectoryRecursive(string fullFilePath)
		{
			return Directory.GetDirectories(fullFilePath, "*", SearchOption.AllDirectories);
		}

	}
}
