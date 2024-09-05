using System.Collections.Generic;
using System.Collections.Immutable;
using starsky.foundation.platform.Helpers;

namespace starskytest.FakeCreateAn.CreateAnZipFileChildFolders;

public static class CreateAnZipFileChildFolders
{
	/// <summary>
	/// @see: https://superuser.com/a/1467266 and 80 chars
	/// </summary>
	private const string Base64CreateAnZipFileChildFoldersString = "UEsDBBQAAAAAAAR8JVkAAAAAAAAAAAAAAAAKAAAAdGVzdC90ZXN0L1BLAwQUAAAAAAAHfCVZt+/cgwEA" +
	                                                     "AAABAAAAEgAAAHRlc3QvdGVzdC90ZXN0LnR4dDFQSwECFAAUAAAAAAAEfCVZAAAAAAAAAAAAAAAACgAA" +
"AAAAAAAAABAAAAAAAAAAdGVzdC90ZXN0L1BLAQIUABQAAAAAAAd8JVm379yDAQAAAAEAAAASAAAAAAAA" +
"AAEAIAAAACgAAAB0ZXN0L3Rlc3QvdGVzdC50eHRQSwUGAAAAAAIAAgB4AAAAWQAAAAAA";

	public static readonly ImmutableArray<byte> Bytes = [..Base64Helper.TryParse(Base64CreateAnZipFileChildFoldersString)];
		
	public static readonly ImmutableDictionary<string, bool> Content =
		new Dictionary<string, bool>
		{
			{ "test", true},
			{ "test/test",true },
			{ "test/test/test.txt", false }
		}.ToImmutableDictionary();
}
