using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class FileHashTest
    {
        [TestMethod]
        [ExcludeFromCoverage]
        public void FileHashGenerateRandomBytesTest()
        {
            var input1 = FileHash.GenerateRandomBytes(10);
            var input2 = FileHash.GenerateRandomBytes(10);
            var test2 = FileHash.GenerateRandomBytes(0);
            Assert.AreEqual(test2.Length,1);
            Assert.AreNotEqual(input1,input2);
        }
    }
}