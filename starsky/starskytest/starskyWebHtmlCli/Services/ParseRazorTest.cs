using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskywebhtmlcli.Services;

namespace starskytests.starskyWebHtmlCli.Services
{
    [TestClass]
    public class ParseRazorTest
    {
        [TestMethod]
        public async Task ParseRazorTestNotFound()
        {
            var result = await new ParseRazor().EmbeddedViews(null, null);
            Assert.AreEqual(string.Empty,result);
        }
    }
}