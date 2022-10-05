using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace starsky.foundation.storage.Helpers
{
    public class PlainTextFileHelper
    {
	    /// <summary>
	    /// Stream to string (UTF8)
	    /// </summary>
	    /// <param name="stream">stream</param>
	    /// <returns>content of the file as string</returns>
	    public string StreamToString(Stream stream)
	    {
		    var reader = new StreamReader(stream, Encoding.UTF8);
		    var result = reader.ReadToEnd();
		    stream.Dispose();
		    return result;
	    }

	    /// <summary>
	    /// Stream to string (UTF8) But Async
	    /// </summary>
	    /// <param name="stream">stream</param>
	    /// <param name="dispose">dispose afterwards = default true</param>
	    /// <returns>content of the file as string</returns>
	    public static async Task<string> StreamToStringAsync(Stream stream, bool dispose = true)
	    {
		    var reader = new StreamReader(stream, Encoding.UTF8);
		    var result = await reader.ReadToEndAsync();
		    if ( dispose ) await stream.DisposeAsync();
		    return result;  
	    }

	    /// <summary>
	    /// String (UTF8) to Stream
	    /// </summary>
	    /// <param name="input"></param>
	    /// <returns></returns>
	    public static Stream StringToStream(string input)
	    {
		    byte[] byteArray = Encoding.UTF8.GetBytes(input);
		    MemoryStream stream = new MemoryStream(byteArray);
		    return stream;
	    }
    }
}
