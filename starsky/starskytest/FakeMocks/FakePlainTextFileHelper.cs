using System;
using starskycore.Helpers;

namespace starskytest.FakeMocks
{
	public class FakePlainTextFileHelper : PlainTextFileHelper
	{
		private string _outputValue;

		public FakePlainTextFileHelper(string outputValue = null)
		{
			_outputValue = outputValue;
		}

		[Obsolete("Replaced")]
		public override void WriteFile(string fullFilePath, string writeString)
		{
		}

	}
}
