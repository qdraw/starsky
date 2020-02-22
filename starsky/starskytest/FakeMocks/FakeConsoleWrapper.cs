using System.Collections.Generic;
using starskycore.Interfaces;

namespace starskytest.FakeMocks
{
	/// <summary>
	/// @see: https://stackoverflow.com/a/3161371
	/// </summary>
	public class FakeConsoleWrapper  : IConsole
	{
		public List<string> LinesToRead;

		public FakeConsoleWrapper(List<string> linesToRead)
		{
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
