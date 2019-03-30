using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using starskycore.Models;

namespace starskycore.Helpers
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
	    /// String (UTF8) to Stream
	    /// </summary>
	    /// <param name="input"></param>
	    /// <returns></returns>
	    public Stream StringToStream(string input)
	    {
		    byte[] byteArray = Encoding.UTF8.GetBytes(input);
		    MemoryStream stream = new MemoryStream(byteArray);
		    return stream;
	    }

	    /// <summary>
        /// [Obsolete] Read a text based file (not binary) file
        /// </summary>
        /// <param name="fullFilePath">path on filesystem</param>
        /// <returns>content of the file as string</returns>
        [Obsolete]
        public string ReadFile(string fullFilePath)
        {
            if (!File.Exists(fullFilePath)) return string.Empty;
            
            return File.ReadAllText(fullFilePath);
        }
        
        /// <summary>
        /// [Obsolete] Write and create a new plain text file to the filesystem
        /// </summary>
        /// <param name="fullFilePath">path on filesystem</param>
        /// <param name="writeString">content of the file</param>
        [Obsolete]
        public virtual void WriteFile(string fullFilePath, string writeString)
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
