using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Helpers
{
	public sealed class DeserializeJson
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
		public async Task<T?> ReadAsync<T>(string jsonSubPath)
		{
			if ( !_iStorage.ExistFile(jsonSubPath) ) throw new FileNotFoundException(jsonSubPath);
			var stream = _iStorage.ReadStream(jsonSubPath);
			var jsonAsString = await StreamToStringHelper.StreamToStringAsync(stream);
			var returnFileIndexItem = JsonSerializer.Deserialize<T>(jsonAsString, DefaultJsonSerializer.CamelCase);
			return returnFileIndexItem;
		}
	}
}
