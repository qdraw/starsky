using System;
using System.Collections.Generic;
using System.Linq;
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
        public void ArgsHelper_NeedVerboseTest()
        {
            var args = new List<string> {"-v"}.ToArray();
            Assert.AreEqual(ArgsHelper.NeedVerbose(args), true);
            
            // Bool parse check
            args = new List<string> {"-v","true"}.ToArray();
            Assert.AreEqual(ArgsHelper.NeedVerbose(args), true);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetIndexModeTest()
        {
            // Default on so testing off
            var args = new List<string> {"-i","false"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetIndexMode(args), false);
        }
        
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_NeedHelpTest()
        {
            var args = new List<string> {"-h"}.ToArray();
            Assert.AreEqual(ArgsHelper.NeedHelp(args), true);
            
            // Bool parse check
            args = new List<string> {"-h","true"}.ToArray();
            Assert.AreEqual(ArgsHelper.NeedHelp(args), true);
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetPathFormArgsTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-p", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetPathFormArgs(args), "/");
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetSubpathFormArgsTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-s", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetSubpathFormArgs(args), "/");
        }    
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_IfSubpathTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-s", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.IfSubpathOrPath(args), true);
            
            // Default
            args = new List<string>{string.Empty}.ToArray();
            Assert.AreEqual(ArgsHelper.IfSubpathOrPath(args), true);
            
            args = new List<string> {"-p", "/"}.ToArray();
            Assert.AreEqual(ArgsHelper.IfSubpathOrPath(args), false);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetThumbnailTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-t", "true"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetThumbnail(args), true);
        }   
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetOrphanFolderCheckTest()
        {
            AppSettingsProvider.BasePath = new CreateAnImage().BasePath;
            var args = new List<string> {"-o", "true"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetOrphanFolderCheck(args), true);
        }   
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetMoveTest()
        {
            var args = new List<string> {"-m"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetMove(args), true);
            
            // Bool parse check
            args = new List<string> {"-m","true"}.ToArray();
            Assert.AreEqual(ArgsHelper.GetMove(args), true);
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_GetAllTest()
        {
            var args = new List<string> {"-a"}.ToArray();
            Assert.AreEqual(false, ArgsHelper.GetAll(args));
            
            // Bool parse check
            args = new List<string> {"-a","false"}.ToArray();
            Assert.AreEqual(false, ArgsHelper.GetAll(args));
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_SetEnvironmentByArgsShortTestListTest()
        {
            var shortNameList = ArgsHelper.ShortNameList.ToArray();
            var envNameList = ArgsHelper.EnvNameList.ToArray();

            var shortTestList = new List<string>();
            for (int i = 0; i < shortNameList.Length; i++)
            {
                shortTestList.Add(shortNameList[i]);
                shortTestList.Add(i.ToString());
            }
            
            ArgsHelper.SetEnvironmentByArgs(shortTestList);
            
            for (int i = 0; i < envNameList.Length; i++)
            {
                Assert.AreEqual(Environment.GetEnvironmentVariable(envNameList[i]),i.ToString());
            }
        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void ArgsHelper_SetEnvironmentByArgsLongTestListTest()
        {
            var longNameList = ArgsHelper.LongNameList.ToArray();
            var envNameList = ArgsHelper.EnvNameList.ToArray();
            
            var longTestList = new List<string>();
            for (int i = 0; i < longNameList.Length; i++)
            {
                longTestList.Add(longNameList[i]);
                longTestList.Add(i.ToString());
            }
            
            ArgsHelper.SetEnvironmentByArgs(longTestList);

            for (int i = 0; i < envNameList.Length; i++)
            {
                Assert.AreEqual(Environment.GetEnvironmentVariable(envNameList[i]),i.ToString());
            }
        }



    }
}