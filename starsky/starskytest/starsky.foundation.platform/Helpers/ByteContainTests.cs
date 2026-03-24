using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public class ByteContainTests
{
	[TestMethod]
	[DataRow("hello world", null, false)]
	[DataRow("hello world", "", false)]
	[DataRow("abc", "abcdef", false)]
	[DataRow("--start-MIDDLE-end--", "--start", true)]
	[DataRow("--start-MIDDLE-end--", "MIDDLE", true)]
	[DataRow("--start-MIDDLE-end--", "end--", true)]
	[DataRow("exact", "exact", true)]
	[DataRow("abcdef", "é", false)]
	[DataRow("aaaaa", "aaa", true)]
	public void Contain_DataDriven_Works(string? source, string pattern, bool expected)
	{
		var bytes = source is null ? [] : Encoding.UTF8.GetBytes(source);
		var res = bytes.Contain(pattern);
		Assert.AreEqual(expected, res);
	}
}
