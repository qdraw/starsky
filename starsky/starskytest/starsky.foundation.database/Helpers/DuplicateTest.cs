using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.sync.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public class DuplicateTest
	{
		[TestMethod]
		public async Task DuplicateItems()
		{
			var content = new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg"),
				new FileIndexItem("/test.jpg"),
				new FileIndexItem("/test.jpg")
			};
			
			var query = new FakeIQuery(content);

			await new Duplicate(query).RemoveDuplicateAsync(content);

			var queryResult= await query.GetAllFilesAsync("/");
			
			Assert.AreEqual(1,queryResult.Count);
		}

		[TestMethod]
		public async Task NonDuplicateItems()
		{
			var content = new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg"),
			};
			
			var query = new FakeIQuery(content);

			await new Duplicate(query).RemoveDuplicateAsync(content);
			var queryResult= await query.GetAllFilesAsync("/");
			
			Assert.AreEqual(1,queryResult.Count);
		}
	}
}
