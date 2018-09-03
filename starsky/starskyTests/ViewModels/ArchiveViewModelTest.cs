using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.ViewModels;

namespace starskytests
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