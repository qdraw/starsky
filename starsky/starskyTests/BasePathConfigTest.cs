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
        public void BasePathConfig_StructureNotNull()
        {
            Assert.AreNotEqual(null,new BasePathConfig().Structure );
        }

    }
}