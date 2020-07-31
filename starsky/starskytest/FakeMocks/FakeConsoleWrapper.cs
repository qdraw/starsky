using System.Collections.Generic;
using starsky.foundation.platform.Interfaces;
using starskycore.Interfaces;

namespace starskytest.FakeMocks
{
	/// <summary>
	/// @see: https://stackoverflow.com/a/3161371
	/// </summary>
	public class FakeConsoleWrapper  : IConsole
	{
		public List<string> LinesToRead;

		public FakeConsoleWrapper()
		{
			LinesToRead = new List<string>();
		}

		public FakeConsoleWrapper(List<string> linesToRead)
		{
			if ( linesToRead == null )
			{
				linesToRead = new List<string>();
			}
			LinesToRead = linesToRead;
		}
		
		public List<string> WrittenLines = new List<string>();

		public void Write(string message)
		{
			WrittenLines.Add(message);
		}

		public void WriteLine(string message)
		{
			WrittenLines.Add(message);
		}

		public string ReadLine()
		{
			string result = LinesToRead[0];
			LinesToRead.RemoveAt(0);
			return result;
		}

	}
}
