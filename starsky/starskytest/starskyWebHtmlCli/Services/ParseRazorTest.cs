using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starskytest.FakeMocks;

namespace starskytest.starskyWebHtmlCli.Services
{
    [TestClass]
    public class ParseRazorTest
    {
        [TestMethod]
        public async Task ParseRazorTestNotFound()
        {
            var result = await new ParseRazor(new FakeIStorage()).EmbeddedViews(null, null);
            Assert.AreEqual(string.Empty,result);
        }
    }
}
