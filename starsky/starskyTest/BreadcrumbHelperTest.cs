using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Services;

namespace starskyTest
{
    [TestClass]
    public class BreadcrumbHelperTest
    {
        [TestMethod]
        public void SlashMethod()
        {
            var breadcrumbExample = Breadcrumbs.BreadcrumbHelper("/");

            var breadcrumblist = new List<string> {"/"};
            CollectionAssert.AreEqual(breadcrumbExample,breadcrumblist);
        }

        [TestMethod]
        public void FileNameMethod()
        {
            var breadcrumbExample = Breadcrumbs.BreadcrumbHelper("/2018/2.jpg");
            var breadcrumblist = new List<string> {"/","/2018"};
            CollectionAssert.AreEqual(breadcrumbExample,breadcrumblist);
        }
        
        [TestMethod]
        public void Null()
        {
            var breadcrumbExample = Breadcrumbs.BreadcrumbHelper(null);
            var breadcrumblist = new List<string>();
            CollectionAssert.AreEqual(breadcrumbExample,breadcrumblist);
        }
    }
}