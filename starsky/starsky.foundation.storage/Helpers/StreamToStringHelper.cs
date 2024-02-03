using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace starsky.foundation.storage.Helpers;

/// <summary>
/// Before known as PlainTextFileHelper
/// </summary>
public static class StreamToStringHelper
{

	/// <summary>
	/// Stream to string (UTF8) But Async - check dispose flag
	/// </summary>
	/// <param name="stream">stream</param>
	/// <param name="dispose">dispose afterwards = default true</param>
	/// <returns>content of the file as string</returns>
	public static async Task<string> StreamToStringAsync(Stream stream, bool dispose = true)
	{
		var reader = new StreamReader(stream, Encoding.UTF8);
		var result = await reader.ReadToEndAsync();
		if ( dispose )
		{
			await stream.DisposeAsync();
		}
		return result;  
	}

}
