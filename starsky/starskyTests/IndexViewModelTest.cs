using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.ViewModels;

namespace starskytests
{
    [TestClass]
    public class IndexViewModelTest
    {
        [TestMethod]
        public void IndexViewModelPageTypeTest()
        {
            var t = new IndexViewModel();
            Assert.AreEqual(t.PageType, PageViewType.PageType.Archive.ToString());            
        }
    }
}