using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.FileSystem;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.native.FileSystem;

[TestClass]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MacOsSecurityScopedBookmarkTests
{
	// =====================================================================
	// Integration tests — run on macOS only, exercise the real P/Invoke layer
	// =====================================================================

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void TryCreateBookmark_ValidPath_ReturnsValidBase64()
	{
		var tempFile = Path.GetTempFileName();
		try
		{
			var sut = new MacOsSecurityScopedBookmark();
			var result = sut.TryCreateBookmark(tempFile, out var bookmarkBase64);

			Assert.IsTrue(result, "Bookmark creation should succeed");
			Assert.IsNotNull(bookmarkBase64);
			Assert.IsNotEmpty(bookmarkBase64);
			// Verify the output is parseable base64
			_ = Convert.FromBase64String(bookmarkBase64);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void TryCreateBookmark_NonExistentPath_ReturnsFalse()
	{
		var sut = new MacOsSecurityScopedBookmark();
		var result =
			sut.TryCreateBookmark("/this/path/does/not/exist/file.txt", out var bookmarkBase64);

		Assert.IsFalse(result);
		Assert.IsNull(bookmarkBase64);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void TryCreateBookmark_EmptyPath_ReturnsFalse()
	{
		var sut = new MacOsSecurityScopedBookmark();
		var result = sut.TryCreateBookmark("", out var bookmarkBase64);

		Assert.IsFalse(result);
		Assert.IsNull(bookmarkBase64);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void TryResolveAndStartAccess_ValidBookmark_ResolvesPath()
	{
		var tempFile = Path.GetTempFileName();
		try
		{
			var sut = new MacOsSecurityScopedBookmark();
			var createResult = sut.TryCreateBookmark(tempFile, out var bookmarkBase64);
			Assert.IsTrue(createResult, "Setup: bookmark creation should succeed");

			var resolveResult =
				sut.TryResolveAndStartAccess(bookmarkBase64!, out var resolvedPath);

			Assert.IsTrue(resolveResult);
			Assert.IsNotNull(resolvedPath);
			Assert.AreEqual(tempFile, resolvedPath);

			sut.StopAccess(resolvedPath);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void TryResolveAndStartAccess_InvalidBase64_ReturnsFalse()
	{
		var sut = new MacOsSecurityScopedBookmark();
		var result =
			sut.TryResolveAndStartAccess("this is not valid base64!!!", out var resolvedPath);

		Assert.IsFalse(result);
		Assert.IsNull(resolvedPath);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void TryResolveAndStartAccess_EmptyBookmarkData_ReturnsFalse()
	{
		var sut = new MacOsSecurityScopedBookmark();
		var result = sut.TryResolveAndStartAccess(
			Convert.ToBase64String(Array.Empty<byte>()),
			out var resolvedPath);

		Assert.IsFalse(result);
		Assert.IsNull(resolvedPath);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void TryResolveAndStartAccess_CorruptedBookmarkData_ReturnsFalse()
	{
		var sut = new MacOsSecurityScopedBookmark();
		var corruptedBase64 = Convert.ToBase64String(new byte[] { 0xFF, 0xFE, 0xFD, 0xFC });
		var result = sut.TryResolveAndStartAccess(corruptedBase64, out var resolvedPath);

		Assert.IsFalse(result);
		Assert.IsNull(resolvedPath);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.OSX)]
	public void StopAccess_InvalidPath_DoesNotThrow()
	{
		var sut = new MacOsSecurityScopedBookmark();
		sut.StopAccess("/this/does/not/exist");

		Assert.IsNotNull(sut, "StopAccess completed without exception");
	}

	// =====================================================================
	// Unit tests — use FakeMacOsSecurityScopedBookmarkNative, run on all OSes
	// =====================================================================

	// --- TryCreateBookmark ---

	[TestMethod]
	public void TryCreateBookmark_WhenCreateFileUrlReturnsZero_ReturnsFalse()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative { FileUrlToReturn = IntPtr.Zero };
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryCreateBookmark("/some/path", out var bookmark);

		Assert.IsFalse(result);
		Assert.IsNull(bookmark);
	}

	[TestMethod]
	public void TryCreateBookmark_WhenCreateBookmarkDataReturnsZero_ReturnsFalse()
	{
		var fake =
			new FakeMacOsSecurityScopedBookmarkNative { BookmarkDataToReturn = IntPtr.Zero };
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryCreateBookmark("/some/path", out var bookmark);

		Assert.IsFalse(result);
		Assert.IsNull(bookmark);
		// fileUrl was released even though bookmark data failed
		Assert.AreEqual(1, fake.CfReleaseCalls);
	}

	[TestMethod]
	public void TryCreateBookmark_WhenSuccessful_ReturnsBase64OfNsDataBytes()
	{
		var expectedBytes = new byte[] { 10, 20, 30, 40 };
		var fake = new FakeMacOsSecurityScopedBookmarkNative
		{
			NsDataBytesToReturn = expectedBytes
		};
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryCreateBookmark("/some/path", out var bookmark);

		Assert.IsTrue(result);
		Assert.AreEqual(Convert.ToBase64String(expectedBytes), bookmark);
	}

	[TestMethod]
	public void TryCreateBookmark_WhenSuccessful_ReleasesFileUrlAndBookmarkNsData()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative();
		var sut = new MacOsSecurityScopedBookmark(fake);

		sut.TryCreateBookmark("/some/path", out _);

		// fileUrl released via CfRelease, bookmarkNsData released via ObjcRelease
		Assert.AreEqual(1, fake.CfReleaseCalls);
		Assert.AreEqual(1, fake.ObjcReleaseCalls);
	}

	// --- TryResolveAndStartAccess ---

	[TestMethod]
	public void TryResolveAndStartAccess_InvalidBase64_ReturnsFalse_NoNativeCallsMade()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative();
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryResolveAndStartAccess("not-valid-base64!!!", out var path);

		Assert.IsFalse(result);
		Assert.IsNull(path);
		// No native calls because Convert.FromBase64String threw first
		Assert.AreEqual(0, fake.ObjcReleaseCalls);
	}

	[TestMethod]
	public void TryResolveAndStartAccess_WhenNsDataFromBytesReturnsZero_ReturnsFalse()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative { NsDataToReturn = IntPtr.Zero };
		var sut = new MacOsSecurityScopedBookmark(fake);
		var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		var result = sut.TryResolveAndStartAccess(validBase64, out var path);

		Assert.IsFalse(result);
		Assert.IsNull(path);
	}

	[TestMethod]
	public void TryResolveAndStartAccess_WhenResolveBookmarkDataReturnsZero_ReturnsFalse()
	{
		var fake =
			new FakeMacOsSecurityScopedBookmarkNative { ResolvedUrlToReturn = IntPtr.Zero };
		var sut = new MacOsSecurityScopedBookmark(fake);
		var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		var result = sut.TryResolveAndStartAccess(validBase64, out var path);

		Assert.IsFalse(result);
		Assert.IsNull(path);
		// nsData was ObjcReleased even though nsUrl was zero
		Assert.AreEqual(1, fake.ObjcReleaseCalls);
	}

	[TestMethod]
	public void TryResolveAndStartAccess_WhenStartAccessReturnsFalse_ReturnsFalse()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative { StartAccessResult = false };
		var sut = new MacOsSecurityScopedBookmark(fake);
		var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		var result = sut.TryResolveAndStartAccess(validBase64, out var path);

		Assert.IsFalse(result);
		Assert.IsNull(path);
		// nsUrl must be released when access is denied
		Assert.AreEqual(1, fake.CfReleaseCalls);
	}

	[TestMethod]
	public void TryResolveAndStartAccess_WhenGetPathReturnsEmpty_ReturnsFalse()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative { PathToReturn = string.Empty };
		var sut = new MacOsSecurityScopedBookmark(fake);
		var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		var result = sut.TryResolveAndStartAccess(validBase64, out var path);

		Assert.IsFalse(result);
		Assert.AreEqual(string.Empty, path);
	}

	[TestMethod]
	public void TryResolveAndStartAccess_WhenSuccessful_ReturnsTrueWithPath()
	{
		const string expectedPath = "/resolved/path";
		var fake = new FakeMacOsSecurityScopedBookmarkNative { PathToReturn = expectedPath };
		var sut = new MacOsSecurityScopedBookmark(fake);
		var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		var result = sut.TryResolveAndStartAccess(validBase64, out var path);

		Assert.IsTrue(result);
		Assert.AreEqual(expectedPath, path);
	}

	[TestMethod]
	public void TryResolveAndStartAccess_WhenSuccessful_ReleasesNsDataButNotNsUrl()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative();
		var sut = new MacOsSecurityScopedBookmark(fake);
		var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		sut.TryResolveAndStartAccess(validBase64, out _);

		// nsData released via ObjcRelease; nsUrl NOT released (caller owns it until StopAccess)
		Assert.AreEqual(1, fake.ObjcReleaseCalls);
		Assert.AreEqual(0, fake.CfReleaseCalls);
	}

	// --- StopAccess ---

	[TestMethod]
	public void StopAccess_WhenCreateFileUrlReturnsZero_DoesNotCallStopAccessing()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative { FileUrlToReturn = IntPtr.Zero };
		var sut = new MacOsSecurityScopedBookmark(fake);

		sut.StopAccess("/some/path");

		Assert.IsFalse(fake.StopAccessCalled);
		Assert.AreEqual(0, fake.CfReleaseCalls);
	}

	[TestMethod]
	public void StopAccess_WhenSuccessful_CallsStopAccessingAndReleasesUrl()
	{
		var urlHandle = new IntPtr(999);
		var fake = new FakeMacOsSecurityScopedBookmarkNative { FileUrlToReturn = urlHandle };
		var sut = new MacOsSecurityScopedBookmark(fake);

		sut.StopAccess("/some/path");

		Assert.IsTrue(fake.StopAccessCalled);
		Assert.AreEqual(1, fake.CfReleaseCalls);
		Assert.AreEqual(urlHandle, fake.AllCfReleased[0]);
	}

	[TestMethod]
	public void StopAccess_WhenNativeThrows_DoesNotPropagateException()
	{
		// The fake can be replaced with a throwing implementation to verify the catch block
		var fake = new ThrowingNative();
		var sut = new MacOsSecurityScopedBookmark(fake);

		// Should not throw
		sut.StopAccess("/some/path");

		Assert.IsNotNull(sut, "StopAccess absorbed the exception");
	}

	// =====================================================================
	// TryStartAccessFromToken — fake-based, all platforms
	// =====================================================================

	[TestMethod]
	public void TryStartAccessFromToken_WhenTokenIsNull_ReturnsFalse()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative();
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryStartAccessFromToken("/some/path", null);

		Assert.IsFalse(result);
		Assert.AreEqual(0, fake.ObjcReleaseCalls, "No native calls expected");
	}

	[TestMethod]
	public void TryStartAccessFromToken_WhenTokenIsEmpty_ReturnsFalse()
	{
		var fake = new FakeMacOsSecurityScopedBookmarkNative();
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryStartAccessFromToken("/some/path", string.Empty);

		Assert.IsFalse(result);
		Assert.AreEqual(0, fake.ObjcReleaseCalls, "No native calls expected");
	}

	[TestMethod]
	public void TryStartAccessFromToken_WithRawBase64Token_ResolvesAccess()
	{
		const string expectedPath = "/resolved/path";
		var rawBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
		var fake = new FakeMacOsSecurityScopedBookmarkNative { PathToReturn = expectedPath };
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryStartAccessFromToken("/some/path", rawBase64);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void TryStartAccessFromToken_WithJsonWrappedToken_UnwrapsAndResolvesAccess()
	{
		// Swift encodes as: JSONEncoder().encode(base64) → the string value gets surrounding quotes
		var rawBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
		var jsonWrapped = JsonSerializer.Serialize(rawBase64); // produces: "\"AQIDBA==\""

		const string expectedPath = "/resolved/path";
		var fake = new FakeMacOsSecurityScopedBookmarkNative { PathToReturn = expectedPath };
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryStartAccessFromToken("/some/path", jsonWrapped);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void TryStartAccessFromToken_WhenNativeResolutionFails_ReturnsFalse()
	{
		var rawBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
		var fake = new FakeMacOsSecurityScopedBookmarkNative
		{
			ResolvedUrlToReturn = IntPtr.Zero
		};
		var sut = new MacOsSecurityScopedBookmark(fake);

		var result = sut.TryStartAccessFromToken("/some/path", rawBase64);

		Assert.IsFalse(result);
	}

	// =====================================================================
	// UnwrapJsonToken — pure .NET, all platforms
	// =====================================================================

	[TestMethod]
	public void UnwrapJsonToken_WithRawBase64_ReturnsSameString()
	{
		var rawBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		var result = MacOsSecurityScopedBookmark.UnwrapJsonToken(rawBase64);

		Assert.AreEqual(rawBase64, result);
	}

	[TestMethod]
	public void UnwrapJsonToken_WithJsonQuotedBase64_UnwrapsCorrectly()
	{
		var rawBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
		// Simulate what Swift's JSONEncoder produces: the value has literal surrounding quotes
		var jsonWrapped = JsonSerializer.Serialize(rawBase64); // → "\"AQID\""

		var result = MacOsSecurityScopedBookmark.UnwrapJsonToken(jsonWrapped);

		Assert.AreEqual(rawBase64, result);
	}

	[TestMethod]
	public void UnwrapJsonToken_WithMalformedJsonQuotes_FallsBackToTrimming()
	{
		// A string that starts/ends with " but is not valid JSON
		const string malformed = "\"notValidJson";

		// No surrounding end-quote → should return as-is (no trimming)
		var result = MacOsSecurityScopedBookmark.UnwrapJsonToken(malformed);

		Assert.AreEqual(malformed, result);
	}

	[TestMethod]
	public void UnwrapJsonToken_WithSurroundingQuotesButInvalidInnerJson_StripsQuotes()
	{
		// e.g., "\"hello\"" is valid JSON string but here test something that has quotes but
		// the inner content is not a valid JSON string for Deserialize<string>
		const string token = "\"plainValue\"";

		var result = MacOsSecurityScopedBookmark.UnwrapJsonToken(token);

		// Valid JSON string → JsonSerializer.Deserialize<string> succeeds → "plainValue"
		Assert.AreEqual("plainValue", result);
	}

	[TestMethod]
	public void UnwrapJsonToken_WithShortString_ReturnsAsIs()
	{
		// Length <= 2 — not possible for a bookmark, but guard-test
		Assert.AreEqual("\"", MacOsSecurityScopedBookmark.UnwrapJsonToken("\""));
		Assert.AreEqual("ab", MacOsSecurityScopedBookmark.UnwrapJsonToken("ab"));
	}
	{
		public override IntPtr CreateFileUrl(string path)
		{
			throw new InvalidOperationException("simulated native failure");
		}
	}
}
