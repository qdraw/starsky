using System;
using System.Collections.Generic;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	public class ExifCopy
	{
		private IStorage _iStorage;
		private IReadMeta _readMeta;
		private IExifTool _exifTool;

		public ExifCopy(IStorage iStorage, IExifTool exifTool, IReadMeta readMeta)
		{
			_iStorage = iStorage;
			_exifTool = exifTool;
			_readMeta = readMeta;
		}
		
		/// <summary>
		/// Add a .xmp sidecar file
		/// </summary>
		/// <param name="subPath"></param>
		/// <returns></returns>
		public string XmpSync(string subPath)
		{
			// only for raw files
			if ( !ExtensionRolesHelper.IsExtensionForceXmp(subPath) ) return subPath;

			var withXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);
                
		    
			if (_iStorage.IsFolderOrFile(withXmp) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
			{
				throw new NotImplementedException();
//			    _exifTool.WriteTagsAsync(withXmp, "-TagsFromFile \""  + fullFilePath + "\"",  "\""+ xmpFullPath +  "\"");
			}
			return withXmp;
		}
	    
		public string CopyExifPublish(string fromSubPath, string toSubPath)
		{
			var updateModel = _readMeta.ReadExifAndXmpFromFile(fromSubPath);
			var comparedNames = CompareAll(updateModel);
			comparedNames.Add(nameof(FileIndexItem.Software));
			updateModel.SetFilePath(toSubPath);
			return new ExifToolCmdHelper(_exifTool,_iStorage,_readMeta).Update(updateModel, comparedNames);
		}


		private List<string> CompareAll(FileIndexItem fileIndexItem)
		{
			return FileIndexCompareHelper.Compare(new FileIndexItem(), fileIndexItem);
		} 
	    
		public void CopyExifToThumbnail(string subPath, string thumbPath)
		{
			var updateModel = _readMeta.ReadExifAndXmpFromFile(subPath);
			var comparedNames = CompareAll(updateModel);

			new ExifToolCmdHelper(_exifTool,_iStorage,_readMeta).Update(updateModel, comparedNames);
		}
	}
}
