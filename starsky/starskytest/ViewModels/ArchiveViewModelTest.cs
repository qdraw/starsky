using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.ViewModels;

namespace starskytest.ViewModels
{
    [TestClass]
    public class ArchiveViewModelTest
    {
        [TestMethod]
        public void ArchiveViewModelPageTypeTest()
        {
            var t = new ArchiveViewModel {IsDirectory = true}; // IsDirectory= not used
            
            Assert.AreEqual(t.PageType, PageViewType.PageType.Archive.ToString());            
        }
    }
}