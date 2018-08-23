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
        
        public void WriteFile(string path, string writeString)
        {
            if (File.Exists(path)) return;
            
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path)) 
            {
                sw.WriteLine(writeString);
            }
        }

    }
}