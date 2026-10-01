using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.connect.Sync;
using starsky.foundation.connect.Crypto;
using starsky.foundation.connect.Transport;
using starsky.foundation.connect.Protocol.Generated;

namespace starskytest.starsky.feature.connect.Sync;

[TestClass]
public sealed class DownloaderTest
{
	private X509Certificate2 _cert = null!;
	private ConnectionManager _connectionManager = null!;
	private BlockStore _blockStore = null!;

	[TestInitialize]
	public void Setup()
	{
		_cert = DeviceIdentity.GenerateCertificate();
		_connectionManager = new ConnectionManager(_cert, NullLogger<ConnectionManager>.Instance, listenPort: 0);
		_blockStore = new BlockStore();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_blockStore.Dispose();
		_connectionManager.Dispose();
		_cert.Dispose();
	}

	[TestMethod]
	public void ReceiveResponse_UnknownId_DoesNotThrow()
	{
		using var downloader = new Downloader(_connectionManager, _blockStore, NullLogger<Downloader>.Instance);

		var response = new Response { Id = 9999, Code = ErrorCode.NoError };
		downloader.ReceiveResponse(response);
	}

	[TestMethod]
	public void ReceiveResponse_KnownId_ResolvesTask()
	{
		using var downloader = new Downloader(_connectionManager, _blockStore, NullLogger<Downloader>.Instance);

		// Use reflection to add a pending TCS with a known ID, then resolve via ReceiveResponse
		var field = typeof(Downloader)
			.GetField("_pending", BindingFlags.NonPublic | BindingFlags.Instance)!;
		var dict = ( System.Collections.Concurrent.ConcurrentDictionary<int, TaskCompletionSource<Response>> )field.GetValue(downloader)!;
		var tcs = new TaskCompletionSource<Response>(TaskCreationOptions.RunContinuationsAsynchronously);
		dict[42] = tcs;

		var response = new Response { Id = 42, Code = ErrorCode.NoError };
		downloader.ReceiveResponse(response);

		Assert.IsTrue(tcs.Task.IsCompletedSuccessfully);
		Assert.AreEqual(ErrorCode.NoError, tcs.Task.Result.Code);
	}

	[TestMethod]
	public async Task DownloadFileAsync_WithNoBlocks_ReturnsTrue()
	{
		using var downloader = new Downloader(_connectionManager, _blockStore, NullLogger<Downloader>.Instance);

		var result = await downloader.DownloadFileAsync(
			deviceId: "ANYDEVICE",
			folderId: "folder1",
			folderRoot: Path.GetTempPath(),
			name: "empty.txt",
			blocks: []);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task DownloadFileAsync_TimesOut_WhenPeerNotConnected()
	{
		using var downloader = new Downloader(_connectionManager, _blockStore, NullLogger<Downloader>.Instance);

		var block = new BlockInfo { Offset = 0, Size = 4, Hash = ByteString.CopyFrom(new byte[32]) };

		using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
		var result = await downloader.DownloadFileAsync(
			deviceId: "NOTCONNECTED",
			folderId: "f",
			folderRoot: Path.GetTempPath(),
			name: "file.txt",
			blocks: [block],
			ct: cts.Token);

		// Peer not connected → send is no-op → no Response comes → block times out → returns false
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Dispose_DoesNotThrow()
	{
		var downloader = new Downloader(_connectionManager, _blockStore, NullLogger<Downloader>.Instance);
		downloader.Dispose();
	}

	[TestMethod]
	public void Dispose_CancelsPendingTasks()
	{
		var downloader = new Downloader(_connectionManager, _blockStore, NullLogger<Downloader>.Instance);

		var field = typeof(Downloader)
			.GetField("_pending", BindingFlags.NonPublic | BindingFlags.Instance)!;
		var dict = ( System.Collections.Concurrent.ConcurrentDictionary<int, TaskCompletionSource<Response>> )field.GetValue(downloader)!;
		var tcs = new TaskCompletionSource<Response>(TaskCreationOptions.RunContinuationsAsynchronously);
		dict[1] = tcs;

		downloader.Dispose();

		Assert.IsTrue(tcs.Task.IsCanceled);
	}
}
