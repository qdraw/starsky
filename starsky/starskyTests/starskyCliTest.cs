using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Helpers;
using starskyCli;

namespace starskytests
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
                "-b", newImage.BasePath, "--thumbnailtempfolder", newImage.BasePath, "-e", newImage.FullFilePath }.ToArray();
            Program.Main(args);
            
        }
    }
}