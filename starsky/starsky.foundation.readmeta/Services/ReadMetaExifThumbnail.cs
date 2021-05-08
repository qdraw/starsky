using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using Directory = MetadataExtractor.Directory;

namespace starsky.foundation.readmeta.Services
{
	public class ReadMetaThumbnail
	{
		private readonly IStorage _iStorage;

		public ReadMetaThumbnail(ISelectorStorage selectorStorage)
		{
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		}
		
		public bool ReadExifFromFile(string subPath)
		{
			List<Directory> allExifItems;
			using ( var stream = _iStorage.ReadStream(subPath) )
			{
				if ( stream == Stream.Null ) return false;
				try
				{
					allExifItems = ImageMetadataReader.ReadMetadata(stream).ToList();
					GetOffset(allExifItems);
					
					stream.Seek(0, SeekOrigin.Begin);
				}
				catch (Exception)
				{
					// ImageProcessing or System.Exception: Handler moved stream beyond end of atom
					stream.Dispose();
					return false;
				}
			}

			return true;
		}
		
		private void GetOffset(List<Directory> allExifItems)
		{

			foreach ( var exifItem in allExifItems )
			{
				//exifItem.Tags.Where()
			}

			Console.WriteLine();
			//var offset = allExifItems.Where(p => p.Tags.Where(p => p.Name == ""))
		}

		private void WriteImageThumbnail(Stream stream)
		{
			
		}
		
	}
}
