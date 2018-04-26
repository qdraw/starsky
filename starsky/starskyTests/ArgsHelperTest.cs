using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Helpers;
using starsky.Models;

namespace starskytests
{
    [TestClass]
    public class ArgsHelperTest
    {
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelperNeedVerboseTest()
        {
            var args = new List<string> {"-v"}.ToArray();
            Assert.AreEqual(ArgsHelper.NeedVerbose(args), true);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsGetIndexModeTest()
        {
            // Default on so testing off
            var args = new List<string> {"-i","false"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetIndexMode(args), false);
        }
        
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelperNeedHelpTest()
        {
            var args = new List<string> {"-h"}.ToArray();
            Assert.AreEqual(ArgsHelper.NeedHelp(args), true);
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelperGetPathFormArgsTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-p", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetPathFormArgs(args), "/");
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelperGetSubpathFormArgsTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-s", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetSubpathFormArgs(args), "/");
        }    
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelperIfSubpathTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-s", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.IfSubpath(args), true);
            
            args = new List<string> {"-p", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.IfSubpath(args), false);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelperGetThumbnailTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-t", "true"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetThumbnail(args), true);
        }   
        
        
    }
}