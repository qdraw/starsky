using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;

namespace starskytest.starsky.foundation.platform.Extensions
{
	[TestClass]
	public class ChunkHelperTest
	{
		[TestMethod]
		public void ChunkTestNull()
		{
			IEnumerable<string> chucky = null;
			var result = chucky.Chunk(1);
			Assert.AreEqual(0,result.Count());
		}

		[TestMethod]
		public void ChunkTest_WithOne()
		{
			var exampleList = new List<string>{"test1","test2","test3", "test4","test5"};
			var result = exampleList.Chunk(1);
			Assert.AreEqual(5,result.Count());
			Assert.AreEqual("test1",result.ToList()[0].ToList()[0]);
			Assert.AreEqual("test2",result.ToList()[1].ToList()[0]);
			Assert.AreEqual("test3",result.ToList()[2].ToList()[0]);
			Assert.AreEqual("test4",result.ToList()[3].ToList()[0]);
			Assert.AreEqual("test5",result.ToList()[4].ToList()[0]);
		}
		
		[TestMethod]
		public void ChunkTest_WithTwo()
		{
			var exampleList = new List<string>{"test1","test2","test3", "test4","test5"};
			var result = exampleList.Chunk(2);
			Assert.AreEqual(3,result.Count());
			Assert.AreEqual("test1",result.ToList()[0].ToList()[0]);
			Assert.AreEqual("test2",result.ToList()[0].ToList()[1]);
			Assert.AreEqual("test3",result.ToList()[1].ToList()[0]);
			Assert.AreEqual("test4",result.ToList()[1].ToList()[1]);
			Assert.AreEqual("test5",result.ToList()[2].ToList()[0]);
		}
		
				
		[TestMethod]
		public void ChunkTest_With50()
		{
			var exampleList = new List<string>{"test1","test2","test3", "test4","test5"};
			var result = exampleList.Chunk(50);
			Assert.AreEqual(1,result.Count());
			Assert.AreEqual("test1",result.ToList()[0].ToList()[0]);
			Assert.AreEqual("test2",result.ToList()[0].ToList()[1]);
			Assert.AreEqual("test3",result.ToList()[0].ToList()[2]);
			Assert.AreEqual("test4",result.ToList()[0].ToList()[3]);
			Assert.AreEqual("test5",result.ToList()[0].ToList()[4]);
		}
	}
}
