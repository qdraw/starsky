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
            var searchViewModel = new SearchViewModel{ElapsedSeconds = 0.0006};
            Assert.AreEqual(true, searchViewModel.ElapsedSeconds <= 0.001);
        }

	    [TestMethod]
	    public void SearchViewModel_Offset_Test()
	    {
		    var searchViewModel = new SearchViewModel();
		    Assert.AreEqual(0,searchViewModel.Offset );
	    }
    }
}
