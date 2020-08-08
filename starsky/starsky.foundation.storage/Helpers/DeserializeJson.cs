using System.IO;
using System.Text.Json;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Helpers
{
	public class DeserializeJson
	{
		private readonly IStorage _iStorage;

		public DeserializeJson(IStorage iStorage)
		{
			_iStorage = iStorage;
		}	
		
		/// <summary>
		/// Read Json
		/// </summary>
		/// <param name="jsonSubPath">location on disk</param>
		/// <typeparam name="T">Typed</typeparam>
		/// <returns>Data</returns>
		/// <exception cref="FileNotFoundException">when file is not found</exception>
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
