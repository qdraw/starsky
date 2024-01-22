using System.IO;
using System.Text;

namespace starsky.foundation.storage.Helpers;

public static class StringToStreamHelper
{
	/// <summary>
	/// String (UTF8) to Stream
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static Stream StringToStream(string input)
	{
		var byteArray = Encoding.UTF8.GetBytes(input);
		var stream = new MemoryStream(byteArray);
		return stream;
	}
}
