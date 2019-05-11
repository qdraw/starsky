using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;

namespace starskytest.Helpers
{
    [TestClass]
    public class HashSetHelperTest
    {
        [TestMethod]
        public void HashSetHelperTest_HashSetDuplicateContent()
        {
            // hashsets does not support duplicates, but test the comma implementation
            var result = HashSetHelper.StringToHashSet("test, test");
            var expetedresult = HashSetHelper.HashSetToString(result);
            Assert.AreEqual("test",expetedresult);
        }
	    
	    [TestMethod]
	    public void HashSetHelperTest_HashSetDoubleComma()
	    {
		    // testing with double commas those are not supported by the c# exif read tool
		    var result = HashSetHelper.StringToHashSet(",,,,,test,, test1");
		    var expetedresult = HashSetHelper.HashSetToString(result);
		    Assert.AreEqual("test, test1",expetedresult);
	    }
	    
	    [TestMethod]
	    public void HashSetHelperTest_HashSetSingleComma()
	    {
		    // testing with double commas those are not supported by the c# exif read tool
		    var result = HashSetHelper.StringToHashSet("test,test1");
		    var expetedresult = HashSetHelper.HashSetToString(result);
		    Assert.AreEqual("test, test1",expetedresult);
	    }

	    [TestMethod]
	    public void HashSetHelperTest_DoubleSpaces()
	    {
		    var hashSetResult = HashSetHelper.StringToHashSet("test0,   test1 , test2,  test3");
			Assert.AreEqual(4,hashSetResult.Count);

			Assert.AreEqual("test0",hashSetResult.ToList()[0]);
			Assert.AreEqual("test1",hashSetResult.ToList()[1]);
			Assert.AreEqual("test2",hashSetResult.ToList()[2]);
			Assert.AreEqual("test3",hashSetResult.ToList()[3]);
	    }

	    [TestMethod]
        public void HashSetHelperTest_SetToStringNull()
        {
            Assert.AreEqual(string.Empty,HashSetHelper.HashSetToString(null));
        }


	    [TestMethod]
	    public void HashSetHelperTest_ListToStringNullTest()
	    {
		    var item = HashSetHelper.ListToString(null);
		    Assert.AreEqual(0,item.Length);
	    }
    }
}