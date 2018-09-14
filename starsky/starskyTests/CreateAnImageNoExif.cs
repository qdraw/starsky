using System;
using System.IO;
using System.Reflection;

namespace starskytests
{
    public class CreateAnImageNoExif
    {

        public readonly string FullFilePathWithDate = 
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + FileNameWithDate;
        private const string FileNameWithDate = "123300_20120101.jpg";
        // HHmmss_yyyyMMdd > not very logical but used to test features

        public CreateAnImageNoExif()
        {
            
             var base64JpgString1 =
                 "/9j/4AAQSkZJRgABAQAAAQABAAD/2wDFAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEAAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB/8EAEQgAAgADAwARAAERAAIRAP/EACcAAQEAAAAAAAAAAAAAAAAAAAAKEAEAAAAAAAAAAAAAAAAAAAAA/9oADAMAAAEAAgAAPwC/gH//2Q==";

            if (!File.Exists(FullFilePathWithDate))
            {
                File.WriteAllBytes(FullFilePathWithDate, Convert.FromBase64String(base64JpgString1));
            }
         }
    }
}