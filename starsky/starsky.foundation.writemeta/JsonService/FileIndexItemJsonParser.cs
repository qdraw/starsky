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
		
		public async Task Write(FileIndexItem fileIndexItem)
		{
			var jsonOutput = JsonSerializer.Serialize(fileIndexItem, new JsonSerializerOptions
			{
				WriteIndented = true, 
			});

			var subPath = PathHelper.AddSlash(fileIndexItem.ParentDirectory) + ".starsky." +
			              fileIndexItem.FileName + ".json";
			
			await _iStorage.WriteStreamAsync(
				new PlainTextFileHelper().StringToStream(jsonOutput), subPath);
		}
		
		public FileIndexItem Read(FileIndexItem fileIndexItem)
		{
			var subPath = PathHelper.AddSlash(fileIndexItem.ParentDirectory) + ".starsky." +
			              fileIndexItem.FileName + ".json";

			// when sidecar file does not exist
			if ( !_iStorage.ExistFile(subPath) ) return fileIndexItem;
			
			var stream = _iStorage.ReadStream( subPath);
			var jsonAsString = new PlainTextFileHelper().StreamToString(stream);
			var returnFileIndexItem = JsonSerializer.Deserialize<FileIndexItem>(jsonAsString);
			returnFileIndexItem.Status = FileIndexItem.ExifStatus.ExifWriteNotSupported;
			return returnFileIndexItem;
		}
	}
}
