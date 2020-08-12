using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.writemeta.JsonService
{
	public class FileIndexItemJsonParser
	{
		private readonly IStorage _iStorage;

		public FileIndexItemJsonParser(IStorage storage)
		{
			_iStorage = storage;
		}
		
		/// <summary>
		/// Write FileIndexItem to IStorage
		/// </summary>
		/// <param name="fileIndexItem">data object</param>
		/// <returns>void</returns>
		public async Task Write(FileIndexItem fileIndexItem)
		{
			var jsonOutput = JsonSerializer.Serialize(fileIndexItem, new JsonSerializerOptions
			{
				WriteIndented = true, 
			});
			var jsonSubPath = JsonSidecarLocation.JsonLocation(fileIndexItem.ParentDirectory, fileIndexItem.FileName);
			await _iStorage.WriteStreamAsync(
				new PlainTextFileHelper().StringToStream(jsonOutput), jsonSubPath);
		}

		/// <summary>
		/// Read sidecar item
		/// </summary>
		/// <param name="fileIndexItem">data object</param>
		/// <returns>data</returns>
		public FileIndexItem Read(FileIndexItem fileIndexItem)
		{
			var jsonSubPath = JsonSidecarLocation.JsonLocation(fileIndexItem.ParentDirectory, fileIndexItem.FileName);
			// when sidecar file does not exist
			if ( !_iStorage.ExistFile(jsonSubPath) ) return fileIndexItem;
			
			var returnFileIndexItem = new DeserializeJson(_iStorage).Read<FileIndexItem>(jsonSubPath);
			returnFileIndexItem.Status = FileIndexItem.ExifStatus.ExifWriteNotSupported;
			return returnFileIndexItem;
		}
	}

}
