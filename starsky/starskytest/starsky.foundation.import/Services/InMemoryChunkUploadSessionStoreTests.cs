using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Services;

namespace starskytest.starsky.foundation.import.Services;

[TestClass]
public sealed class InMemoryChunkUploadSessionStoreTests
{
	[TestMethod]
	public void Create_And_GetStatus_DefaultFlow()
	{
		var store = new InMemoryChunkUploadSessionStore();
		var result = store.Create("test.jpg", "/", 2, 6);

		var status = store.GetStatus(result.UploadId);
		Assert.IsNotNull(status);
		Assert.AreEqual("test.jpg", status.FileName);
		Assert.AreEqual(2, status.TotalChunks);
		Assert.AreEqual(0, status.ReceivedChunks);
		Assert.AreEqual(6L, status.TotalSize);
		Assert.IsFalse(status.IsComplete);
	}

	[TestMethod]
	public void AddChunk_InvalidIndex_ReturnsFalse()
	{
		var store = new InMemoryChunkUploadSessionStore();
		var result = store.Create("test.jpg", "/", 1, 3);

		var success = store.AddChunk(result.UploadId, 1, [1, 2, 3], out var errorMessage);

		Assert.IsFalse(success);
		Assert.AreEqual("invalid chunk index", errorMessage);
	}

	[TestMethod]
	public void AddChunk_DuplicateChunk_ReturnsFalse()
	{
		var store = new InMemoryChunkUploadSessionStore();
		var result = store.Create("test.jpg", "/", 1, 3);

		Assert.IsTrue(store.AddChunk(result.UploadId, 0, [1, 2, 3], out _));
		var success = store.AddChunk(result.UploadId, 0, [1, 2, 3], out var errorMessage);

		Assert.IsFalse(success);
		Assert.AreEqual("chunk already uploaded", errorMessage);
	}

	[TestMethod]
	public void TryAssemble_MissingChunk_ReturnsFalse()
	{
		var store = new InMemoryChunkUploadSessionStore();
		var result = store.Create("test.jpg", "/", 2, 6);
		store.AddChunk(result.UploadId, 0, [1, 2, 3], out _);

		var success = store.TryAssemble(result.UploadId, out var payload, out var errorMessage);

		Assert.IsFalse(success);
		Assert.AreEqual("missing chunks", errorMessage);
		CollectionAssert.AreEqual(Array.Empty<byte>(), payload);
	}

	[TestMethod]
	public void TryAssemble_SizeMismatch_ReturnsFalse()
	{
		var store = new InMemoryChunkUploadSessionStore();
		var result = store.Create("test.jpg", "/", 2, 10);
		store.AddChunk(result.UploadId, 0, [1, 2, 3], out _);
		store.AddChunk(result.UploadId, 1, [4, 5, 6], out _);

		var success = store.TryAssemble(result.UploadId, out var payload, out var errorMessage);

		Assert.IsFalse(success);
		Assert.AreEqual("total size mismatch", errorMessage);
		CollectionAssert.AreEqual(Array.Empty<byte>(), payload);
	}

	[TestMethod]
	public void TryAssemble_HappyFlow_ReturnsOrderedPayload()
	{
		var store = new InMemoryChunkUploadSessionStore();
		var result = store.Create("test.jpg", "/", 2, 6);
		store.AddChunk(result.UploadId, 1, [4, 5, 6], out _);
		store.AddChunk(result.UploadId, 0, [1, 2, 3], out _);

		var success = store.TryAssemble(result.UploadId, out var payload, out var errorMessage);

		Assert.IsTrue(success);
		Assert.AreEqual(string.Empty, errorMessage);
		CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, payload);
	}

	[TestMethod]
	public void Delete_RemovesSession()
	{
		var store = new InMemoryChunkUploadSessionStore();
		var result = store.Create("test.jpg", "/", 1, 3);

		Assert.IsTrue(store.Delete(result.UploadId));
		Assert.IsNull(store.GetStatus(result.UploadId));
	}
}




