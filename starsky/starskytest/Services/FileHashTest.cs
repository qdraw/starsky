using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Services
{
    [TestClass]
    public class FileHashTest
    {
        [TestMethod]
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
            var iStorageFake = new FakeIStorage(
		        new List<string>{"/"},
		        new List<string>{"/test.jpg"},
		        new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes}
	        );
	        var fileHashCode = new FileHash(iStorageFake).GetHashCode("/test.jpg",0);
	        
	        Assert.IsFalse(fileHashCode.Value);
            Assert.AreEqual(true, fileHashCode.Key.Contains("_T"));
        }
        
        [TestMethod]
        public void FileHash_CreateAnImage_Test()
        {
	        var createAnImage = new CreateAnImage();
	        var iStorage = new StorageSubPathFilesystem(new AppSettings{StorageFolder = createAnImage.BasePath});
	        var fileHashCode = new FileHash(iStorage).GetHashCode(createAnImage.DbPath);
	        Assert.IsTrue(fileHashCode.Value);
	        Assert.AreEqual(26,fileHashCode.Key.Length);
        }
        
        
        
    }
}
