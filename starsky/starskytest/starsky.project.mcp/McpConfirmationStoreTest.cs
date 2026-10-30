using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.mcp.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class McpConfirmationStoreTest
{
	private static readonly string[] Paths = ["/a.jpg", "/b.jpg"];

	[TestMethod]
	public void Consume_SameCallAndArguments_Works()
	{
		var store = new McpConfirmationStore(new FakeTimeProvider());
		var token = store.Issue("alice", "tool", Paths, "x=1");

		Assert.IsTrue(store.Consume(token, "alice", "tool", ["/b.jpg", "/a.jpg"], "x=1"));
	}

	[TestMethod]
	public void Consume_SecondTime_IsRejected()
	{
		var store = new McpConfirmationStore(new FakeTimeProvider());
		var token = store.Issue("alice", "tool", Paths, "x=1");

		Assert.IsTrue(store.Consume(token, "alice", "tool", Paths, "x=1"));
		Assert.IsFalse(store.Consume(token, "alice", "tool", Paths, "x=1"));
	}

	[TestMethod]
	public void Consume_Expired_IsRejected()
	{
		var time = new FakeTimeProvider();
		var store = new McpConfirmationStore(time);
		var token = store.Issue("alice", "tool", Paths, "x=1");

		time.Now = time.Now.Add(McpConfirmationStore.Lifetime).AddSeconds(1);

		Assert.IsFalse(store.Consume(token, "alice", "tool", Paths, "x=1"));
	}

	[TestMethod]
	[DataRow("bob", "tool", "/a.jpg", "x=1")]
	[DataRow("alice", "other", "/a.jpg", "x=1")]
	[DataRow("alice", "tool", "/c.jpg", "x=1")]
	[DataRow("alice", "tool", "/a.jpg", "x=2")]
	public void Consume_DifferentCallerToolPathsOrArgs_IsRejected(string caller, string tool,
		string path, string args)
	{
		var store = new McpConfirmationStore(new FakeTimeProvider());
		var token = store.Issue("alice", "tool", ["/a.jpg"], "x=1");

		Assert.IsFalse(store.Consume(token, caller, tool, [path], args));
		// a failed check also burns the token
		Assert.IsFalse(store.Consume(token, "alice", "tool", ["/a.jpg"], "x=1"));
	}

	[TestMethod]
	public void Consume_UnknownToken_IsRejected()
	{
		var store = new McpConfirmationStore(new FakeTimeProvider());

		Assert.IsFalse(store.Consume("unknown", "alice", "tool", Paths, ""));
	}

	[TestMethod]
	public void Issue_RemovesExpiredTokens()
	{
		var time = new FakeTimeProvider();
		var store = new McpConfirmationStore(time);
		var old = store.Issue("alice", "tool", Paths, "");
		time.Now = time.Now.Add(TimeSpan.FromHours(1));

		store.Issue("alice", "tool", Paths, "");

		Assert.IsFalse(store.Consume(old, "alice", "tool", Paths, ""));
	}

	[TestMethod]
	public void DefaultConstructor_IssuesWorkingToken()
	{
		var store = new McpConfirmationStore();
		var token = store.Issue("alice", "tool", Paths, "");

		Assert.IsTrue(store.Consume(token, "alice", "tool", Paths, ""));
	}
}
