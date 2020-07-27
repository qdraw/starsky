using System.IO;
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
		/// Get the jsonSubPath `parentDir/.starsky.filename.ext.json`
		/// </summary>
		/// <param name="parentDirectory">parent Directory</param>
		/// <param name="fileName">and filename</param>
		/// <returns>parentDir/.starsky.filename.`ext.json</returns>
		public string JsonLocation(string parentDirectory, string fileName)
		{
			return PathHelper.AddSlash(parentDirectory) + ".starsky." + fileName
			        + ".json";
		}
		
		public async Task Write(FileIndexItem fileIndexItem)
		{
			var jsonOutput = JsonSerializer.Serialize(fileIndexItem, new JsonSerializerOptions
			{
				WriteIndented = true, 
			});
			var jsonSubPath = JsonLocation(fileIndexItem.ParentDirectory, fileIndexItem.FileName);
			await _iStorage.WriteStreamAsync(
				new PlainTextFileHelper().StringToStream(jsonOutput), jsonSubPath);
		}

		public FileIndexItem Read(FileIndexItem fileIndexItem)
		{
			var jsonSubPath = JsonLocation(fileIndexItem.ParentDirectory, fileIndexItem.FileName);
			// when sidecar file does not exist
			if ( !_iStorage.ExistFile(jsonSubPath) ) return fileIndexItem;
			
			var returnFileIndexItem = Read<FileIndexItem>(jsonSubPath);
			returnFileIndexItem.Status = FileIndexItem.ExifStatus.ExifWriteNotSupported;
			return returnFileIndexItem;
		}
		
		public T Read<T>(string jsonSubPath)
		{
			if ( !_iStorage.ExistFile(jsonSubPath) ) throw new FileNotFoundException(jsonSubPath);
			var stream = _iStorage.ReadStream(jsonSubPath);
			var jsonAsString = new PlainTextFileHelper().StreamToString(stream);
			var returnFileIndexItem = JsonSerializer.Deserialize<T>(jsonAsString);
			return returnFileIndexItem;
		}
	}

}
