using System;
using System.IO;
using System.Text.RegularExpressions;

namespace starsky.Helpers
{
	public static class Base64Helper
	{
		/// <summary>
		/// input a base64-formated-string return base64-formated-byte array
		/// </summary>
		/// <param name="inputstring">base64 string</param>
		/// <returns>byte array</returns>
		public static byte[] TryParse(string inputstring)
		{
			if ( inputstring?.Length % 4 != 0 || !Base64Regex.IsMatch(inputstring) )
				return new byte[0];
			return Convert.FromBase64String(inputstring);
			// Source: https://stackoverflow.com/questions/7686585/something-like-tryparse-from-convert-frombase64string
		}

		/// <summary>
		/// Normal string to base64-formated-string
		/// </summary>
		/// <param name="plainText">Normal string</param>
		/// <returns>base64-string</returns>
		public static string EncodeToString(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		/// <summary>
		/// Normal string > base64-formated-string > base64-formated-bytes
		/// </summary>
		/// <param name="plainText">Normal string</param>
		/// <returns>base64 bytes</returns>
		public static byte[] EncodeToBytes(string plainText)
		{
			var fromBase64String = EncodeToString(plainText);
			byte[] bytes = TryParse(fromBase64String);
			return bytes;
		}

		/// <summary>
		/// Regex to match a base64 string
		/// </summary>
		private static readonly Regex Base64Regex = new Regex(
			@"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}[AEIMQUYcgkosw048]=|[A-Za-z0-9+/][AQgw]==)?$",
			RegexOptions.Compiled);

		/// <summary>
		/// Memorystring to base64 string
		/// </summary>
		/// <param name="outputStream">input stream</param>
		/// <returns>base64 string</returns>
		public static string ToBase64(MemoryStream outputStream)
		{
			var bytes = outputStream.ToArray();
			return ToBase64(bytes);
		}

		/// <summary>
		/// base64 bytes to base64 string
		/// </summary>
		/// <param name="bytes">The base64 bytes.</param>
		/// <returns></returns>
		public static string ToBase64(byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}

	}
}
