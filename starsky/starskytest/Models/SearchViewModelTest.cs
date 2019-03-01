using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.ViewModels;

namespace starskytest.Models
{
    [TestClass]
    public class SearchViewModelTest
    {
        [TestMethod]
        public void SearchViewModel_ElapsedSeconds_Test()
        {
            var q = new SearchViewModel{ElapsedSeconds = 0.0006};
            Assert.AreEqual(true, q.ElapsedSeconds <= 0.001);
        }
    }
}
