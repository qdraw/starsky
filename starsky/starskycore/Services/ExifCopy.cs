using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		private const string XmpStartContent =
			"<x:xmpmeta xmlns:x=\'adobe:ns:meta/\' x:xmptk=\'Starsky\'>" +
			"\n<rdf:RDF xmlns:rdf=\'http://www.w3.org/1999/02/22-rdf-syntax-ns#\'>\n" +
			"</rdf:RDF>\n</x:xmpmeta>";

		
		public void XmpCreate(string xmpPath)
		{
			if ( _iStorage.ExistFile(xmpPath) ) return;
			
			var plainTextStream = new PlainTextFileHelper().StringToStream(XmpStartContent);
			_iStorage.WriteStream(plainTextStream, xmpPath);
		}

		
		
//		public void XmpSync(List<string> inputSubPaths)
//		{
//			foreach ( var subPath in inputSubPaths )
//			{
//				XmpSync(subPath);
//			}
//		}
//
//		/// <summary>
//		/// Add a .xmp sidecar file
//		/// </summary>
//		/// <param name="subPath"></param>
//		/// <returns></returns>
//		public string XmpSync(string subPath)
//		{
//			// only for raw files
//			if ( !ExtensionRolesHelper.IsExtensionForceXmp(subPath) ) return subPath;
//
//			var withXmp = ExtensionRolesHelper.ReplaceExtensionWithXmp(subPath);
//               
//			if (_iStorage.IsFolderOrFile(withXmp) == FolderOrFileModel.FolderOrFileTypeList.Deleted)
//			{
//				XmpCreate(withXmp);
//			}
//
//			// Now copy content using exifTool
//			CopyExifPublish(subPath, withXmp);
//
//			return withXmp;
//		}
	    
		
		
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

		/// <summary>
		/// Copy for list all items (ignore if file already exist)
		/// </summary>
		/// <param name="fileIndexList">List with all content to be synced</param>
		public void CopyExifToThumbnail(IEnumerable<FileIndexItem> fileIndexList)
		{
			foreach ( var updateModel in fileIndexList )
			{
				if ( ! _iStorage.ThumbnailExist(updateModel.FileHash) ) continue;
				var comparedNames = CompareAll(updateModel);
				comparedNames.Add(nameof(FileIndexItem.Software));
				new ExifToolCmdHelper(_exifTool,_iStorage,_readMeta).UpdateThumbnail(updateModel, comparedNames);
			}
		}
	    
//		public void CopyExifToThumbnail(string subPath, string thumbPath)
//		{
//			var updateModel = _readMeta.ReadExifAndXmpFromFile(subPath);
//			var comparedNames = CompareAll(updateModel);
//
//			new ExifToolCmdHelper(_exifTool,_iStorage,_readMeta).Update(updateModel, comparedNames);
//		}
	}
}
