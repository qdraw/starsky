using System.IO;

namespace starsky.Helpers
{
    public class PlainTextFileHelper
    {
        /// <summary>
        /// Read a text based file (not binary) file
        /// </summary>
        /// <param name="fullFilePath">path on filesystem</param>
        /// <returns>content of the file as string</returns>
        public string ReadFile(string fullFilePath)
        {
            if (!File.Exists(fullFilePath)) return string.Empty;
            
            return File.ReadAllText(fullFilePath);
        }
        
        /// <summary>
        /// Write and create a new plain text file to the filesystem
        /// </summary>
        /// <param name="fullFilePath">path on filesystem</param>
        /// <param name="writeString">content of the file</param>
        public void WriteFile(string fullFilePath, string writeString)
        {
            if (File.Exists(fullFilePath)) return;
            
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(fullFilePath)) 
            {
                sw.WriteLine(writeString);
            }
        }

    }
}