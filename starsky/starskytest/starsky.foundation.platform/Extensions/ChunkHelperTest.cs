using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;

namespace starskytest.starsky.foundation.platform.Extensions
{
	[TestClass]
	public sealed class ChunkHelperTest
	{
		[TestMethod]
		public void ChunkTestNull()
		{
			IEnumerable<string>? chucky = null;
			var result = chucky!.ChunkyEnumerable(1);
			Assert.AreEqual(0,result.Count());
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public void ChunkTest_WithOne()
		{
			var exampleList = new List<string>{"test1","test2","test3", "test4","test5"};
			var result = exampleList.ChunkyEnumerable(1);
			Assert.IsNotNull(result);
			Assert.AreEqual(5,result.Count());
			Assert.AreEqual("test1",result.ToList()[0].ToList()[0]);
			Assert.AreEqual("test2",result.ToList()[1].ToList()[0]);
			Assert.AreEqual("test3",result.ToList()[2].ToList()[0]);
			Assert.AreEqual("test4",result.ToList()[3].ToList()[0]);
			Assert.AreEqual("test5",result.ToList()[4].ToList()[0]);
		}
		
		[TestMethod]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public void ChunkTest_WithTwo()
		{
			var exampleList = new List<string>{"test1","test2","test3", "test4","test5"};
			var result = exampleList.ChunkyEnumerable(2);
			Assert.AreEqual(3,result.Count());
			Assert.AreEqual("test1",result.ToList()[0].ToList()[0]);
			Assert.AreEqual("test2",result.ToList()[0].ToList()[1]);
			Assert.AreEqual("test3",result.ToList()[1].ToList()[0]);
			Assert.AreEqual("test4",result.ToList()[1].ToList()[1]);
			Assert.AreEqual("test5",result.ToList()[2].ToList()[0]);
		}
		
				
		[TestMethod]
		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public void ChunkTest_With50()
		{
			var exampleList = new List<string>{"test1","test2","test3", "test4","test5"};
			var result = exampleList.ChunkyEnumerable(50);
			Assert.AreEqual(1,result.Count());
			Assert.AreEqual("test1",result.ToList()[0].ToList()[0]);
			Assert.AreEqual("test2",result.ToList()[0].ToList()[1]);
			Assert.AreEqual("test3",result.ToList()[0].ToList()[2]);
			Assert.AreEqual("test4",result.ToList()[0].ToList()[3]);
			Assert.AreEqual("test5",result.ToList()[0].ToList()[4]);
		}
	}
}
