using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public sealed class DuplicateTest
{
	[TestMethod]
	public async Task DuplicateItems()
	{
		var content = new List<FileIndexItem>
		{
			new("/test.jpg"), new("/test.jpg"), new("/test.jpg")
		};

		var query = new FakeIQuery(content);

		await new Duplicate(query).RemoveDuplicateAsync(content);

		var queryResult = await query.GetAllFilesAsync("/");

		Assert.HasCount(1, queryResult);
	}

	[TestMethod]
	public async Task NonDuplicateItems()
	{
		var content = new List<FileIndexItem> { new("/test.jpg") };

		var query = new FakeIQuery(content);

		await new Duplicate(query).RemoveDuplicateAsync(content);
		var queryResult = await query.GetAllFilesAsync("/");

		Assert.HasCount(1, queryResult);
	}
}
