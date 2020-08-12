using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskywebhtmlcli;

namespace starskytest.starskyWebHtmlCli
{
    [TestClass]
    public class starskyWebHtmlCli_IntegrationTest
    {
        [TestMethod]
        public void starskyWebHtmlCli_IntegrationTest_NotFoundTest()
        {
            var args = new List<string> {"-p", "not-found-folder" ,"-n", "testrun"
            }.ToArray();
            Program.Main(args);
            // see console log ==> Please add a valid folder: not-found-folder
        }
        
        [TestMethod]
        public void starskyWebHtmlCli_IntegrationTest_NoPath()
        {
            var args = new List<string> {}.ToArray();
            Program.Main(args);
            // There is a console log
        }
    }
}
