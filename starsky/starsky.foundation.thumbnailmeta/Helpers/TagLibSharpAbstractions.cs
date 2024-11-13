using System.IO;
using File = TagLib.File;

namespace starsky.foundation.thumbnailmeta.Helpers;

public class TagLibSharpAbstractions
{
	public class FileBytesAbstraction : File.IFileAbstraction
	{
		public FileBytesAbstraction(string name, Stream stream)
		{
			Name = name;

			ReadStream = stream;
			WriteStream = stream;
		}

		public void CloseStream(Stream stream)
		{
			stream.Dispose();
		}

		public string Name { get; }

		public Stream ReadStream { get; }

		public Stream WriteStream { get; }
	}
}
