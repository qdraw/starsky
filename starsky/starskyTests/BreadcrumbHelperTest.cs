using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class BreadcrumbHelperTest
    {
        [TestMethod]
        [ExcludeFromCoverage]
        public void BreadcrumbSlashMethodTest()
        {
            var breadcrumbExample = Breadcrumbs.BreadcrumbHelper("/");

            var breadcrumblist = new List<string> {"/"};
            CollectionAssert.AreEqual(breadcrumbExample,breadcrumblist);
        }

        [ExcludeFromCoverage]
        [TestMethod]
        public void BreadcrumbFileNameMethodTest()
        {
            var breadcrumbExample = Breadcrumbs.BreadcrumbHelper("/2018/2.jpg");
            var breadcrumblist = new List<string> {"/","/2018"};
            CollectionAssert.AreEqual(breadcrumbExample,breadcrumblist);
        }

        [ExcludeFromCoverage]
        [TestMethod]
        public void BreadcrumbNullTest()
        {
            var breadcrumbExample = Breadcrumbs.BreadcrumbHelper(null);
            var breadcrumblist = new List<string>();
            CollectionAssert.AreEqual(breadcrumbExample,breadcrumblist);
        }
    }
}
