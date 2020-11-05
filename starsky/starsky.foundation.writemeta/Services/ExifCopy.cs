using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.writemeta.Interfaces;
using ExifToolCmdHelper = starsky.foundation.writemeta.Helpers.ExifToolCmdHelper;

namespace starsky.foundation.writemeta.Services
{
	public class ExifCopy
	{
		private readonly IStorage _iStorage;
		private readonly IReadMeta _readMeta;
		private readonly IExifTool _exifTool;
		private readonly IStorage _thumbnailStorage;

		public ExifCopy(IStorage iStorage, IStorage thumbnailStorage,  IExifTool exifTool, IReadMeta readMeta)
		{
			_iStorage = iStorage;
			_exifTool = exifTool;
			_readMeta = readMeta;
			_thumbnailStorage = thumbnailStorage;
		}

		private const string XmpStartContent =
			"<x:xmpmeta xmlns:x=\'adobe:ns:meta/\' x:xmptk=\'Starsky\'>" +
			"\n<rdf:RDF xmlns:rdf=\'http://www.w3.org/1999/02/22-rdf-syntax-ns#\'>\n" +
			"</rdf:RDF>\n</x:xmpmeta>";

		/// <summary>
		/// Create a simple xmp starter file
		/// </summary>
		/// <param name="xmpPath">location</param>
		public void XmpCreate(string xmpPath)
		{
			if ( _iStorage.ExistFile(xmpPath) ) return;
			
			var plainTextStream = new PlainTextFileHelper().StringToStream(XmpStartContent);
			_iStorage.WriteStream(plainTextStream, xmpPath);
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

			// only for files that not exist yet
			if ( _iStorage.IsFolderOrFile(withXmp) != 
			     FolderOrFileModel.FolderOrFileTypeList.Deleted) return withXmp;
			
			XmpCreate(withXmp);
				
			// Now copy content using exifTool
			CopyExifPublish(subPath, withXmp);

			return withXmp;
		}
	    
		/// <summary>
		/// Keep within the same storage provider
		/// Used for example by Import
		/// </summary>
		/// <param name="fromSubPath"></param>
		/// <param name="toSubPath"></param>
		/// <returns></returns>
		internal string CopyExifPublish(string fromSubPath, string toSubPath)
		{
			var updateModel = _readMeta.ReadExifAndXmpFromFile(fromSubPath);
			var comparedNames = FileIndexCompareHelper.Compare(new FileIndexItem(), updateModel);
			comparedNames.Add(nameof(FileIndexItem.Software));
			updateModel.SetFilePath(toSubPath);
			return new ExifToolCmdHelper(_exifTool,_iStorage, _thumbnailStorage ,_readMeta).Update(updateModel, comparedNames);
		}


	}
}
