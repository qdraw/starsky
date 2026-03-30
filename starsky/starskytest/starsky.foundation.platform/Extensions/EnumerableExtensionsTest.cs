using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;

namespace starskytest.starsky.foundation.platform.Extensions;

[TestClass]
public class EnumerableExtensionsTest
{
	[TestMethod]
	public async Task ForEachAsync_ReturnsAllResults()
	{
		var items = new[] { 1, 2, 3 };

		var results = await items.ForEachAsync(async i =>
		{
			await Task.Yield();
			return i * 2;
		}, 3);

		var list = results?.ToList();
		Assert.IsNotNull(list);
		CollectionAssert.AreEquivalent(new List<int> { 2, 4, 6 }, list);
		Assert.HasCount(3, list);
	}

	[TestMethod]
	public async Task ForEachAsync_EmptySequence_ReturnsNull()
	{
		var items = Array.Empty<int>();
		var results = await items.ForEachAsync(async i =>
		{
			await Task.Yield();
			return i;
		});

		// Implementation uses BufferBlock Try receive All which returns false and sets out var to null when empty
		Assert.IsNull(results);
	}

	[TestMethod]
	public async Task ForEachAsync_RespectsParallelism()
	{
		var items = new[] { 1, 2, 3 };

		var sw = Stopwatch.StartNew();
		var res1 = await items.ForEachAsync(Work, 1);
		sw.Stop();
		var singleMs = sw.ElapsedMilliseconds;

		sw.Restart();
		var res3 = await items.ForEachAsync(Work, 3);
		sw.Stop();
		var parallelMs = sw.ElapsedMilliseconds;

		// With sequential execution 3 * 300ms ~= 900ms
		Assert.IsGreaterThanOrEqualTo(800, singleMs,
			"Expected sequential run to take around 900ms (was " + singleMs + "ms)");

		// With full parallelism it should be significantly faster than sequential
		Assert.IsLessThan(singleMs, parallelMs,
			"Expected parallel run to be faster than sequential");
		Assert.IsLessThan(700, parallelMs,
			"Expected parallel run to be under 700ms (was " + parallelMs + "ms)");

		// validate results count
		Assert.AreEqual(3, res1?.Count() ?? 0);
		Assert.AreEqual(3, res3?.Count() ?? 0);
		return;

		async Task<int> Work(int i)
		{
			// 300ms per item
			await Task.Delay(300, TestContext.CancellationToken);
			return i;
		}
	}

	public TestContext TestContext { get; set; }

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	public void FirstOrDefaultIfValid_NullOrEmpty_ReturnsFirst(string? token)
	{
		var source = new List<string> { "one", "two" };
		var result = source.FirstOrDefaultWithFallback(token);
		// Implementation: null or empty returns the first element
		Assert.AreEqual("one", result);
	}

	[TestMethod]
	public void FirstOrDefaultIfValid_SingleItemCollection_ReturnsItem()
	{
		var source = new List<string> { "only" };
		var result = source.FirstOrDefaultWithFallback("only");
		// Implementation: A single item collection returns the only item
		Assert.AreEqual("only", result);
	}

	[TestMethod]
	[DataRow("b", true)]
	[DataRow("z", false)]
	public void FirstOrDefaultIfValid_MultipleItems_ReturnsMatchOrNull(string value, bool exists)
	{
		var source = new List<string> { "a", "b", "c" };
		var result = source.FirstOrDefaultWithFallback(value);
		if ( exists )
		{
			Assert.AreEqual(value, result);
		}
		else
		{
			Assert.IsNull(result);
		}
	}

	private sealed class Person(string name)
	{
		public readonly string Name = name;
	}

	[TestMethod]
	public void FirstOrDefaultIfValid_ReferenceType_ReturnsSameInstance()
	{
		var p1 = new Person("p1");
		var p2 = new Person("p2");
		var source = new List<Person> { p1, p2 };
		var result = source.FirstOrDefaultWithFallback(p2);
		Assert.IsNotNull(result);
		Assert.AreSame(p2, result);
		// Use Name to avoid unused auto-property analyzer warning
		Assert.AreEqual("p2", result.Name);
	}

	private static readonly string[] SourceArray = ["x", "y", "z"];

	[TestMethod]
	public void FirstOrDefaultIfValid_NonICollectionSource_Works()
	{
		// Create a LINQ iterator (Where) which is not an ICollection<T>
		var iter = SourceArray.Where(_ => true);
		var result = iter.FirstOrDefaultWithFallback("y");
		Assert.IsNotNull(result);
		Assert.AreEqual("y", result);
	}
}
