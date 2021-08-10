using System;
using starsky.foundation.storage.Helpers;
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
	}
}
