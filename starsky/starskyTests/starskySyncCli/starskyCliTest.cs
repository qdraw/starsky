using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starskysynccli;
using starskytests.FakeCreateAn;

namespace starskytests.starskySyncCli
{
    [TestClass]
    public class StarskyCliTest
    {
        [ExcludeFromCoverage]
        [TestMethod]
        public void StarskyCliHelpVerbose()
        {
            var args = new List<string> {
                "-h","-v"
            }.ToArray();
            Program.Main(args);
        }
        
        [ExcludeFromCoverage]
        [TestMethod]
        public void StarskyCliHelpTest()
        {
            var newImage = new CreateAnImage();
            var args = new List<string> {"-h","-v","-c","test","-d", "InMemoryDatabase", 
                "-b", newImage.BasePath, "--thumbnailtempfolder", 
                newImage.BasePath, "-e", newImage.FullFilePath 
            }.ToArray();
            Program.Main(args);
        }
        
        [ExcludeFromCoverage]
        [TestMethod]
        public void StarskyCliSubPathOneImage()
        {
            var newImage = new CreateAnImage();
            var args = new List<string> {
                "-v",
                "-c","test",
                "-d", "InMemoryDatabase", 
                "-b", newImage.BasePath, 
                "--thumbnailtempfolder", newImage.BasePath, 
                "--exiftoolpath", newImage.FullFilePath 
            }.ToArray();
            
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
            
            Program.Main(args);
        }
        
    }
}
