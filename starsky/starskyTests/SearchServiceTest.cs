using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Data;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class SearchServiceTest
    {
        private SearchService _search;

        public SearchServiceTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("search");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _search = new SearchService(context);
        }

        [ExcludeFromCoverage]
        [TestMethod]
        public void Todo()
        {
            _search.Search("");
        }
        // Todo: add this unit test
    }
}