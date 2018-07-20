using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starskyCli;

namespace starskytests.StarskyCli
{
    [TestClass]
    public class StarskyCliTest
    {
        [ExcludeFromCoverage]
        [TestMethod]
        public void StarskyCliHelpTest()
        {
            var newImage = new CreateAnImage();
            var args = new List<string> {"-h","-v","-c","test","-d", "inmemorydatabase",
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
            var args = new List<string> {"-s",newImage.DbPath, "-v","-c","test","-d", "inmemorydatabase",
                "-b", newImage.BasePath, "--thumbnailtempfolder", newImage.BasePath, "-e", newImage.FullFilePath 
            }.ToArray();
            Program.Main(args);
        }
        
    }
}