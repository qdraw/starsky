using System;
using System.IO;
using System.Text.RegularExpressions;

namespace starsky.Helpers
{
    public static class Base64Helper
    {
        /// <summary>
        /// input a base64 string return byte array
        /// </summary>
        /// <param name="inputstring">base64 string</param>
        /// <returns>byte array</returns>
        public static byte[] TryParse(string inputstring)
        {
			if (inputstring?.Length % 4 != 0 || !_rx.IsMatch(inputstring)) return new byte[0];
            return Convert.FromBase64String(inputstring);
            // Source: https://stackoverflow.com/questions/7686585/something-like-tryparse-from-convert-frombase64string
        }
    
        private static readonly Regex _rx = new Regex(
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
            return Convert.ToBase64String(bytes);
        }
    }
}