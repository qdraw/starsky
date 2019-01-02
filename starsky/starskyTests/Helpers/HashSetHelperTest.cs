using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;

namespace starskytests.Helpers
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
        public void HashSetHelperTest_SetToStringNull()
        {
            Assert.AreEqual(string.Empty,HashSetHelper.HashSetToString(null));
        }
    }
}