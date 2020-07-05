using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.writemeta.JsonService
{
	public class FileIndexItemJsonWriter
	{
		private IStorage _iStorage;

		public FileIndexItemJsonWriter(IStorage storage)
		{
			_iStorage = storage;
		}
		
		public async Task Write(FileIndexItem fileIndexItem)
		{
			var jsonOutput = JsonSerializer.Serialize(fileIndexItem, new JsonSerializerOptions
			{
				WriteIndented = true, 
			});

			var subPath = PathHelper.AddSlash(fileIndexItem.ParentDirectory) + "." +
			              fileIndexItem.FileName + ".json";
			
			await _iStorage.WriteStreamAsync(
				new PlainTextFileHelper().StringToStream(jsonOutput), subPath);
		}
	}
}
