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
        public void BasePathConfigStructureAsterixPositionsTest()
        {
            var structureAsterixPositions = new BasePathConfig{Structure = "/t*/*"}.StructureAsterixPositions();
            CollectionAssert.AreEqual(new List<int> {2, 4},structureAsterixPositions);
        }
        [TestMethod]
        public void BasePathConfigStructureAsterixPositionsNoAstrixTest()
        {
            var structureAsterixPositions = new BasePathConfig{Structure = "/test"}.StructureAsterixPositions();
            CollectionAssert.AreEqual(new List<int>(),structureAsterixPositions);
        }


    }
}