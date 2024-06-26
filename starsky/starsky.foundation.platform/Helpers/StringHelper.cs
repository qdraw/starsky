namespace starsky.foundation.platform.Helpers
{
	public static class StringHelper
	{
		public static string AsciiNullReplacer(string newStringValue)
		{
			return ( newStringValue == "\\0" || newStringValue == "\\\\0" )
				? string.Empty
				: newStringValue;
		}

		public const string AsciiNullChar = @"\\0";
	}
}
