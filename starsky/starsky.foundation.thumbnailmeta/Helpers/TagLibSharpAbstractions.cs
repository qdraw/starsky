using System.IO;

namespace starsky.foundation.thumbnailmeta.Helpers;

public class TagLibSharpAbstractions
{
	public class MemoryFileAbstraction : TagLib.File.IFileAbstraction
	{
		readonly Stream stream;

		public MemoryFileAbstraction (Stream data)
		{
			stream = data;
		}

		public string Name => "MEMORY";

		public Stream ReadStream => stream;

		public Stream WriteStream => stream;

		public void CloseStream (Stream stream)
		{
			// This causes a stackoverflow
			//stream?.Close();
		}
	}
}
