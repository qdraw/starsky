using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class ExifToolTest
    {
        [TestMethod]
        [ExcludeFromCoverage]
        public void ExifToolTestFixingJsonKeywordStringTest()
        {
            var input = "{\"keywords\": [\"test\"] }";
            var output = ExifTool.FixingJsonKeywordString(input);
            Assert.AreEqual(input,output);
            
            var input2 = "{\"keywords\": \"test\" }";
            var output2 = ExifTool.FixingJsonKeywordString(input);
            Assert.AreEqual(input,output2);   
            
        }

    }
}