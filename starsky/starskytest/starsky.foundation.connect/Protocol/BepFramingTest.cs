using System.Buffers.Binary;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Protocol;
using starsky.foundation.connect.Protocol.Generated;
using BepFileInfo = starsky.foundation.connect.Protocol.Generated.FileInfo;
using BepIndex = starsky.foundation.connect.Protocol.Generated.Index;

namespace starskytest.starsky.foundation.connect.Protocol;

[TestClass]
public sealed class BepFramingTest
{
	[TestMethod]
	public async Task WriteHello_ReadHello_RoundTrip()
	{
		var hello = new Hello
		{
			DeviceName = "test-device",
			ClientName = "starsky-connect",
			ClientVersion = "v1.0.0",
		};

		using var stream = new MemoryStream();
		var writer = new BepWriter(stream);
		await writer.WriteHelloAsync(hello);

		stream.Position = 0;
		var reader = new BepReader(stream);
		var parsed = await reader.ReadHelloAsync();

		Assert.AreEqual("test-device", parsed.DeviceName);
		Assert.AreEqual("starsky-connect", parsed.ClientName);
		Assert.AreEqual("v1.0.0", parsed.ClientVersion);
	}

	[TestMethod]
	public async Task WriteHello_MagicBytesAreBigEndian()
	{
		var hello = new Hello { DeviceName = "x" };

		using var stream = new MemoryStream();
		var writer = new BepWriter(stream);
		await writer.WriteHelloAsync(hello);

		stream.Position = 0;
		var bytes = stream.ToArray();

		// First 4 bytes must be 0x2EA7D90B in big-endian
		var magic = BinaryPrimitives.ReadUInt32BigEndian(bytes);
		Assert.AreEqual(0x2EA7D90Bu, magic, "Hello magic must be 0x2EA7D90B in big-endian.");
	}

	[TestMethod]
	public async Task WriteHello_WrongMagic_ThrowsBepProtocolException()
	{
		using var stream = new MemoryStream();
		// Write wrong magic
		stream.Write([0x00, 0x00, 0x00, 0x00]);
		// Write 2-byte length = 0
		stream.Write([0x00, 0x00]);
		stream.Position = 0;

		var reader = new BepReader(stream);
		await Assert.ThrowsExactlyAsync<BepProtocolException>(
			() => reader.ReadHelloAsync());
	}

	[TestMethod]
	public async Task WriteMessage_ReadMessage_Ping_RoundTrip()
	{
		var ping = new Ping();
		using var stream = new MemoryStream();
		var writer = new BepWriter(stream);
		await writer.WriteMessageAsync(ping, MessageType.Ping);

		stream.Position = 0;
		var reader = new BepReader(stream);
		var msg = await reader.ReadMessageAsync();

		Assert.AreEqual(MessageType.Ping, msg.Header.Type);
		Assert.AreEqual(MessageCompression.None, msg.Header.Compression);
	}

	[TestMethod]
	public async Task WriteMessage_ClusterConfig_RoundTrip()
	{
		var config = new ClusterConfig();
		config.Folders.Add(new Folder { Id = "default", Label = "Default" });

		using var stream = new MemoryStream();
		var writer = new BepWriter(stream);
		await writer.WriteMessageAsync(config, MessageType.ClusterConfig);

		stream.Position = 0;
		var reader = new BepReader(stream);
		var msg = await reader.ReadMessageAsync();

		Assert.AreEqual(MessageType.ClusterConfig, msg.Header.Type);
		var parsed = msg.ParseBody(ClusterConfig.Parser);
		Assert.AreEqual(1, parsed.Folders.Count);
		Assert.AreEqual("default", parsed.Folders[0].Id);
	}

	[TestMethod]
	public async Task WriteMessage_Index_WithCompression_RoundTrip()
	{
		var index = new BepIndex { Folder = "default" };
		index.Files.Add(new BepFileInfo { Name = "photo.jpg", Size = 1024 });

		using var stream = new MemoryStream();
		var writer = new BepWriter(stream);
		await writer.WriteMessageAsync(index, MessageType.Index, compress: true);

		stream.Position = 0;
		var reader = new BepReader(stream);
		var msg = await reader.ReadMessageAsync();

		Assert.AreEqual(MessageType.Index, msg.Header.Type);
		Assert.AreEqual(MessageCompression.Lz4, msg.Header.Compression);
		var parsed = msg.ParseBody(BepIndex.Parser);
		Assert.AreEqual("default", parsed.Folder);
		Assert.AreEqual(1, parsed.Files.Count);
		Assert.AreEqual("photo.jpg", parsed.Files[0].Name);
	}

	[TestMethod]
	public async Task ReadMessage_BodyTooLarge_ThrowsBepProtocolException()
	{
		// Write a valid Ping frame, then tamper the body-length field to exceed 500 MB
		using var template = new MemoryStream();
		var writer = new BepWriter(template);
		await writer.WriteMessageAsync(new Ping(), MessageType.Ping);

		var frameBytes = template.ToArray();

		// The frame layout is: uint16 hdrLen | hdrBytes | uint32 bodyLen | bodyBytes
		// We need to overwrite the uint32 bodyLen field
		var hdrLen = BinaryPrimitives.ReadUInt16BigEndian(frameBytes);
		var bodyLenOffset = 2 + hdrLen;

		var tampered = new byte[frameBytes.Length];
		frameBytes.CopyTo(tampered, 0);
		BinaryPrimitives.WriteUInt32BigEndian(new System.Span<byte>(tampered, bodyLenOffset, 4), 500_000_001u);

		using var stream = new MemoryStream(tampered);
		var reader = new BepReader(stream);
		await Assert.ThrowsExactlyAsync<BepProtocolException>(
			() => reader.ReadMessageAsync());
	}

	[TestMethod]
	public async Task WriteMessage_HeaderLengthField_IsBigEndian()
	{
		var ping = new Ping();
		using var stream = new MemoryStream();
		var writer = new BepWriter(stream);
		await writer.WriteMessageAsync(ping, MessageType.Ping);

		stream.Position = 0;
		var bytes = stream.ToArray();

		// First 2 bytes of post-auth frame = uint16 big-endian header length
		var hdrLen = BinaryPrimitives.ReadUInt16BigEndian(bytes);
		// Should be a small positive number
		Assert.IsTrue(hdrLen is > 0 and < 1024,
			$"Header length {hdrLen} should be a small positive number.");
	}
}
