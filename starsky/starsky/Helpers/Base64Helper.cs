using System;
using System.Text.RegularExpressions;

namespace starsky.Helpers
{
    public static class Base64Helper
    {
        public static byte[] TryParse(string s)
        {
            if (s?.Length % 4 != 0 || !_rx.IsMatch(s)) return null;
            try
            {
                return Convert.FromBase64String(s);
            }
            catch (FormatException)
            {
                // ignore this FormatException
                Console.WriteLine("ignore this: FormatException");
            }
            return null;
        }
    
        private static readonly Regex _rx = new Regex(
            @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}[AEIMQUYcgkosw048]=|[A-Za-z0-9+/][AQgw]==)?$",
            RegexOptions.Compiled);
    }
}