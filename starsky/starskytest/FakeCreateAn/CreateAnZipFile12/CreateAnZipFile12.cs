using System.Collections.Generic;
using System.Collections.Immutable;
using starsky.foundation.platform.Helpers;

namespace starskytest.FakeCreateAn.CreateAnZipFile12
{
	public static class CreateAnZipFile12
	{
		/// <summary>
		/// @see: https://superuser.com/a/1467266 and 80 chars
		/// </summary>
		private const string Base64CreateAnZipFile12String = "UEsDBAoAAAAAAJGKi1YxvvG8EAAAABAAAAAJABwAZmlsZTEudHh0VVQJAAPhljVk1ZY1ZHV4CwABBOgD" 
			+ "AAAE6AMAAFRoaXMgaXMgZmlsZSAxLgpQSwMECgAAAAAAloqLVmgAt74QAAAAEAAAAAkAHABmaWxlMi50" 
			+ "eHRVVAkAA+yWNWTsljVkdXgLAAEE6AMAAAToAwAAVGhpcyBpcyBmaWxlIDIuClBLAQIeAwoAAAAAAJGK" 
			+ "i1YxvvG8EAAAABAAAAAJABgAAAAAAAEAAACkgQAAAABmaWxlMS50eHRVVAUAA+GWNWR1eAsAAQToAwAA" 
			+ "BOgDAABQSwECHgMKAAAAAACWiotWaAC3vhAAAAAQAAAACQAYAAAAAAABAAAApIFTAAAAZmlsZTIudHh0" 
			+ "VVQFAAPsljVkdXgLAAEE6AMAAAToAwAAUEsFBgAAAAACAAIAngAAAKYAAAAAAA==";

		public static readonly ImmutableArray<byte> Bytes = [..Base64Helper.TryParse(Base64CreateAnZipFile12String)];
		
		public static readonly ImmutableDictionary<string, string> Content =
			new Dictionary<string, string>
		{
			{ "file1.txt", "This is file 1.\n" },
			{ "file2.txt", "This is file 2.\n" }
		}.ToImmutableDictionary();
	}
}

