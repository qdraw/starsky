using System.Collections.Generic;
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
		public override string ReadFirstFile(List<string> fullFilePaths)
		{
			return _outputValue;
		}


		public override void WriteFile(string fullFilePath, string writeString)
		{
		}

	}
}
