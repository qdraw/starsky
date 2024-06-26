using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using starsky.foundation.platform.Helpers;

namespace starskytest.FakeCreateAn
{
	public static class CreateAnImageColorClass
	{
		/// <summary>
		/// @see: https://superuser.com/a/1467266 and 80 chars
		/// </summary>
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		private static readonly string Base64JpgString = "/9j/4AAQSkZJRgABAQABLAEsAAD/4QPORXhp" +
		                                                 "ZgAATU0AKgAAAAgACgEPAAIAAAAFAAAAhgEQAAIAAAAK" +
		                                                 "AAAAjAESAAMAAAABAAEAAAEaAAUAAAABAAAAlgEbAAUAAAABAAAAngEoAAMAAAABAAIAAAExAAIAAAAI" +
		                                                 "AAAApgEyAAIAAAAUAAAArodpAAQAAAABAAAAwoglAAQAAAABAAADQAAAAABTb255AABJTENFLTY2MDAA" +
		                                                 "AAABLAAAAAEAAAEsAAAAAVN0YXJza3kAMjAyMDowODoyMiAxMToxNDowOAAAJYKaAAUAAAABAAAChIKd" +
		                                                 "AAUAAAABAAACjIgiAAMAAAABAAMAAIgnAAMAAAABAZAAAIgwAAMAAAABAAIAAIgyAAQAAAABAAABkJAA" +
		                                                 "AAcAAAAEMDIzMZADAAIAAAAUAAAClJAEAAIAAAAUAAACqJIBAAoAAAABAAACvJICAAUAAAABAAACxJID" +
		                                                 "AAoAAAABAAACzJIEAAoAAAABAAAC1JIFAAUAAAABAAAC3JIHAAMAAAABAAUAAJIIAAMAAAABAAAAAJIJ" +
		                                                 "AAMAAAABABAAAJIKAAUAAAABAAAC5KABAAMAAAABAAEAAKACAAQAAAABAAAAAaADAAQAAAABAAAAAaIO" +
		                                                 "AAUAAAABAAAC7KIPAAUAAAABAAAC9KIQAAMAAAABAAMAAKMAAAcAAAABAwAAAKMBAAcAAAABAQAAAKQB" +
		                                                 "AAMAAAABAAAAAKQCAAMAAAABAAAAAKQDAAMAAAABAAAAAKQEAAUAAAABAAAC/KQFAAMAAAABAB4AAKQG" +
		                                                 "AAMAAAABAAAAAKQIAAMAAAABAAAAAKQJAAMAAAABAAAAAKQKAAMAAAABAAAAAKQyAAUAAAAEAAADBKQ0" +
		                                                 "AAIAAAAbAAADJAAAAAAAAAABAAAD5wAAAAkAAAABMjAyMDowODoyMiAxMToxNDowOAAyMDIwOjA4OjIy" +
		                                                 "IDExOjE0OjA4AAAAgtcAAA0hAADJagAAH8UAABglAAACgP////kAAAAKAAABzwAAAIAAAAAUAAAAAQBi" +
		                                                 "Dw8AAAnVAGIPDwAACdUAAAABAAAAAQAAABIAAAABAAAAyAAAAAEAAAAHAAAAAgAyZkcAB//7RSAxOC0y" +
		                                                 "MDBtbSBGMy41LTYuMyBPU1MgTEUAAAAGAAEAAgAAAAJOAAAAAAIABQAAAAMAAAOOAAMAAgAAAAJFAAAA" +
		                                                 "AAQABQAAAAMAAAOmAAUAAQAAAAEAAAAAAAYABQAAAAEAAAO+AAAAAAAAAC4AAAABAAAAAAAAAAEAAAKE" +
		                                                 "AAAAZAAAAAYAAAABAAAAKwAAAAEAABJhAAAAZAAACMcAAAAB/+EM7Wh0dHA6Ly9ucy5hZG9iZS5jb20v" +
		                                                 "eGFwLzEuMC8APD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQi" +
		                                                 "Pz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iWE1QIENvcmUgNS40" +
		                                                 "LjAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5" +
		                                                 "bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczpkYz0iaHR0cDovL3B1" +
		                                                 "cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNv" +
		                                                 "bS9waG90b3Nob3AvMS4wLyIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4" +
		                                                 "bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0" +
		                                                 "dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1sbnM6cGhvdG9t" +
		                                                 "ZWNoYW5pYz0iaHR0cDovL25zLmNhbWVyYWJpdHMuY29tL3Bob3RvbWVjaGFuaWMvMS4wLyIgcGhvdG9z" +
		                                                 "aG9wOkNpdHk9Ik1hZ2xhbmQiIHBob3Rvc2hvcDpTdGF0ZT0iQXV2ZXJnbmUtUmhvbmUtQWxwZXMiIHBo" +
		                                                 "b3Rvc2hvcDpDb3VudHJ5PSJGcmFuY2UiIHBob3Rvc2hvcDpEYXRlQ3JlYXRlZD0iMjAyMC0wOC0yMlQx" +
		                                                 "MToxNDowOCIgeG1wOkNyZWF0ZURhdGU9IjIwMjAtMDgtMjJUMTE6MTQ6MDgiIHhtcDpMYWJlbD0iV2lu" +
		                                                 "bmVyIiB4bXA6Q3JlYXRvclRvb2w9IlN0YXJza3kiIHhtcDpNb2RpZnlEYXRlPSIyMDIwLTA4LTIyVDEx" +
		                                                 "OjE0OjA4IiBwaG90b21lY2hhbmljOkNvbG9yQ2xhc3M9IjEiPiA8ZGM6c3ViamVjdD4gPHJkZjpCYWc+" +
		                                                 "IDxyZGY6bGk+dGV0ZSBkZSBiYWxhY2hhPC9yZGY6bGk+IDxyZGY6bGk+YmVyZ3RvcDwvcmRmOmxpPiA8" +
		                                                 "cmRmOmxpPm1pc3Q8L3JkZjpsaT4gPHJkZjpsaT5mbGFpbmUgPC9yZGY6bGk+IDwvcmRmOkJhZz4gPC9k" +
		                                                 "YzpzdWJqZWN0PiA8ZGM6dGl0bGU+IDxyZGY6QWx0PiA8cmRmOmxpIHhtbDpsYW5nPSJ4LWRlZmF1bHQi" +
		                                                 "PlRldGUgZGUgQmFsYWNoYSAmYW1wOyBGbGFpbmU8L3JkZjpsaT4gPC9yZGY6QWx0PiA8L2RjOnRpdGxl" +
		                                                 "PiA8eG1wTU06SGlzdG9yeT4gPHJkZjpTZXE+IDxyZGY6bGkgc3RFdnQ6c29mdHdhcmVBZ2VudD0iU3Rh" +
		                                                 "cnNreSIvPiA8L3JkZjpTZXE+IDwveG1wTU06SGlzdG9yeT4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRm" +
		                                                 "OlJERj4gPC94OnhtcG1ldGE+ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg" +
		                                                 "ICAgICAgICAgICAgICAgICAgICAgICAgICAgPD94cGFja2V0IGVuZD0idyI/PgD/7QD6UGhvdG9zaG9w" +
		                                                 "IDMuMAA4QklNBAQAAAAAAMEcAVoAAxslRxwCAAACAAIcAmUABkZyYW5jZRwCPgAIMjAyMDA4MjIcAj8A" +
		                                                 "BjExMTQwOBwCBQAYVGV0ZSBkZSBCYWxhY2hhICYgRmxhaW5lHAI3AAgyMDIwMDgyMhwCPAAGMTExNDA4" +
		                                                 "HAIZAA90ZXRlIGRlIGJhbGFjaGEcAhkAB2Jlcmd0b3AcAhkABG1pc3QcAhkAB2ZsYWluZSAcAl8AFEF1" +
		                                                 "dmVyZ25lLVJob25lLUFscGVzHAJaAAdNYWdsYW5kADhCSU0EJQAAAAAAECByBRi8iRdZCMF8Oqsv+IH/" +
		                                                 "2wCEAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5" +
		                                                 "PTgyPC4zNDIBCQkJDAsMGA0NGDIhHCEyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIy" +
		                                                 "MjIyMjIyMjIyMjIyMv/AABEIAAEAAQMBIgACEQEDEQH/xABLAAEBAAAAAAAAAAAAAAAAAAAABhABAAAA" +
		                                                 "AAAAAAAAAAAAAAAAAAEBAAAAAAAAAAAAAAAAAAAAAhEBAAAAAAAAAAAAAAAAAAAAAP/aAAwDAQACEQMR" +
		                                                 "AD8AswDF/9k=";

		public static readonly ImmutableArray<byte> Bytes =
			Base64Helper.TryParse(Base64JpgString).ToImmutableArray();
	}
}
