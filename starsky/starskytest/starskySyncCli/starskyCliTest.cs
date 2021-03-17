using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Attributes;
using starskysynccli;
using starskytest.FakeCreateAn;

namespace starskytest.starskySyncCli
{
    [TestClass]
    public class StarskyCliTest
    {
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
        public void StarskyCliTest_StarskyCliSubPathOneImage()
        {
            var newImage = new CreateAnImage();
            var args = new List<string> {
                "-v",
                "-c","test",
                "--connection", "StarskyCliSubPathOneImage",
                "-d", "InMemoryDatabase", 
                "-b", newImage.BasePath, 
                "--thumbnailtempfolder", newImage.BasePath, 
                "--exiftoolpath", newImage.FullFilePath 
            }.ToArray();

            Console.WriteLine("-->");
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
            
            Program.Main(args);
            Console.WriteLine("<--");
        }
        
    }
}
