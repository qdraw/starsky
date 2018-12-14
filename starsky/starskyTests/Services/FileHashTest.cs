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

        [TestMethod]
        public void FileHash_Md5TimeoutAsyncWrapper_Fail_Test()
        {
            // Give the hasher 0 seconds to calc a hash; so timeout is activated
            var q = FileHash.GetHashCode(new CreateAnImage().FullFilePath,0);
            Assert.AreEqual(true, q.Contains("_T"));
        }
        
        [TestMethod]
        public void FileHash_CreateAnImage_Test()
        {
            // Give the hasher 0 seconds to calc a hash; so timeout is activated
            var q = FileHash.GetHashCode(new CreateAnImage().FullFilePath);
        }
        
        
        
    }
}