using System;
using System.IO;

namespace starsky.Helpers
{
    public class PlainTextFileHelper
    {
        public string ReadFile(string fullFilePath)
        {
            if (!File.Exists(fullFilePath)) return string.Empty;
            
            return File.ReadAllText(fullFilePath);
        }
    }
}