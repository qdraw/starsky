using System;
using System.IO;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;

namespace starskytest.FakeMocks;

public class FakeDownloadResponse : IDownloadResponse<FileMetadata>
{
	private readonly byte[] _content;
	private bool _disposed;

	public FakeDownloadResponse(byte[] content)
	{
		_content = content;
		Response = new FileMetadata(
			id: "id",
			name: "file.txt",
			clientModified: DateTime.UtcNow,
			serverModified: DateTime.UtcNow,
			rev: "123456789",
			size: ( ulong ) content.Length,
			pathLower: "/file.txt",
			pathDisplay: "/file.txt",
			sharingInfo: null,
			isDownloadable: true,
			contentHash: new string('a', 64)
		);
	}

	public Task<string> GetContentAsStringAsync()
	{
		throw new NotImplementedException();
	}

	public FileMetadata Response { get; }

	public Task<Stream> GetContentAsStreamAsync()
	{
		throw new NotImplementedException();
	}

	public Task<byte[]> GetContentAsByteArrayAsync()
	{
		return Task.FromResult(_content);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(true);
	}

	protected virtual void Dispose(bool _)
	{
		_disposed = true;
	}

	public Stream GetContentAsStream()
	{
		return new MemoryStream(_content);
	}
}
