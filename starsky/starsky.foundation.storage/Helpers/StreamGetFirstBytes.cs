using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.storage.Helpers;

public static class StreamGetFirstBytes
{
	public static async Task<MemoryStream> GetFirstBytesAsync(Stream originalStream, int count,
		CancellationToken cancellationToken = default)
	{
		// Create a new MemoryStream to store the first 'count' bytes
		var resultStream = new MemoryStream();

		// Save the current position of the originalStream
		var originalPosition = originalStream.Position;

		// Set the position of the originalStream to the beginning
		originalStream.Seek(0, SeekOrigin.Begin);

		// Copy 'count' bytes from the originalStream to the resultStream asynchronously
		var buffer = new byte[count];
		var memory = new Memory<byte>(buffer);
		var bytesRead = await originalStream.ReadAsync(memory, cancellationToken);
		var readOnlyMemory = new ReadOnlyMemory<byte>(buffer, 0, bytesRead);
		await resultStream.WriteAsync(readOnlyMemory, CancellationToken.None);

		// Reset the position of the originalStream to its original position
		originalStream.Seek(originalPosition, SeekOrigin.Begin);

		// Set the position of the resultStream to the beginning
		resultStream.Seek(0, SeekOrigin.Begin);

		return resultStream;
	}
}
