using System;
using System.IO;
using System.Text.RegularExpressions;

namespace starsky.Helpers
{
    public static class Base64Helper
    {
        public static byte[] TryParse(string s)
        {
            if (s?.Length % 4 != 0 || !_rx.IsMatch(s)) return null;
            return Convert.FromBase64String(s);
            // Source: https://stackoverflow.com/questions/7686585/something-like-tryparse-from-convert-frombase64string
        }
    
        private static readonly Regex _rx = new Regex(
            @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}[AEIMQUYcgkosw048]=|[A-Za-z0-9+/][AQgw]==)?$",
            RegexOptions.Compiled);

        public static string ToBase64(MemoryStream outputStream)
        {
            var bytes = outputStream.ToArray();
            return Convert.ToBase64String(bytes);
        }
    }
}