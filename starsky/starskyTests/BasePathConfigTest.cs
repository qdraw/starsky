using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;

namespace starskytests
{
    [TestClass]
    public class BasePathConfigTest
    {
        [TestMethod]
        public void BasePathConfigTestReadonlyTest()
        {
            // Remove backslash DirectorySeparatorChar
            var readonlyTest = new List<string>{"test" + Path.DirectorySeparatorChar};
            var basePathConfig = new BasePathConfig{Readonly = readonlyTest};
            CollectionAssert.AreEqual(new List<string>{"test"}, basePathConfig.Readonly);
        }

        [TestMethod]
        public void BasePathConfigStructureStringBasicTest()
        {
            // Remove and add backslashes
            var basePathConfig = new BasePathConfig {Structure = "test" + Path.DirectorySeparatorChar};
            Assert.AreEqual("/test",basePathConfig.Structure);
        }

        [TestMethod]
        public void BasePathConfigStructureNotNull()
        {
            Assert.AreNotEqual(null,new BasePathConfig().Structure );
        }

    }
}