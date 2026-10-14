using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Crypto;

namespace starskytest.starsky.foundation.connect.Crypto;

[TestClass]
public sealed class Base32Test
{
	[TestMethod]
	public void Encode_KnownVector_ReturnsCorrectBase32()
	{
		// RFC 4648 test vector: 0x00 0x00 0x00 0x00 0x00 → "AAAAAAAAAA"
		// but let's use a simpler single-byte round-trip
		var data = new byte[] { 0x00 };
		var encoded = Base32.Encode(data);
		// 0x00 = 0b00000000: first 5 bits → 0 → 'A', remaining 3 bits padded → 0 → 'A'
		Assert.AreEqual("AA", encoded);
	}

	[TestMethod]
	public void Encode_Decode_RoundTrip()
	{
		var original = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x23, 0x45, 0x67 };
		var encoded = Base32.Encode(original);
		var decoded = Base32.Decode(encoded);
		CollectionAssert.AreEqual(original, decoded);
	}

	[TestMethod]
	public void Encode_32Bytes_Returns52Chars()
	{
		// SHA-256 output is 32 bytes → 32*8/5 = 51.2, rounds up to 52 chars (padded)
		var hash = new byte[32];
		System.Security.Cryptography.RandomNumberGenerator.Fill(hash);
		var encoded = Base32.Encode(hash);
		Assert.AreEqual(52, encoded.Length);
	}

	[TestMethod]
	public void Encode_EmptyInput_ReturnsEmptyString()
	{
		Assert.AreEqual(string.Empty, Base32.Encode([]));
	}
}
