using System;

namespace starsky.foundation.connect.Crypto;

/// <summary>
/// RFC 4648 base32 encoding (A–Z, 2–7) without padding.
/// </summary>
public static class Base32
{
	internal const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

	public static string Encode(ReadOnlySpan<byte> data)
	{
		if ( data.IsEmpty )
		{
			return string.Empty;
		}

		// Each 5 bits maps to one base32 char; 8 bytes → 13 chars (rounded up to full groups)
		var outputLength = ( data.Length * 8 + 4 ) / 5;
		var result = new char[outputLength];
		var index = 0;
		var buffer = 0;
		var bitsLeft = 0;

		foreach ( var b in data )
		{
			buffer = ( buffer << 8 ) | b;
			bitsLeft += 8;
			while ( bitsLeft >= 5 )
			{
				bitsLeft -= 5;
				result[index++] = Alphabet[( buffer >> bitsLeft ) & 0x1F];
			}
		}

		if ( bitsLeft > 0 )
		{
			result[index] = Alphabet[( buffer << ( 5 - bitsLeft ) ) & 0x1F];
		}

		return new string(result, 0, outputLength);
	}

	public static byte[] Decode(string encoded)
	{
		if ( string.IsNullOrEmpty(encoded) )
		{
			return [];
		}

		var outputLength = encoded.Length * 5 / 8;
		var result = new byte[outputLength];
		var buffer = 0;
		var bitsLeft = 0;
		var index = 0;

		foreach ( var c in encoded )
		{
			var val = Alphabet.IndexOf(char.ToUpperInvariant(c));
			if ( val < 0 )
			{
				throw new FormatException($"Invalid base32 character: {c}");
			}

			buffer = ( buffer << 5 ) | val;
			bitsLeft += 5;
			if ( bitsLeft >= 8 )
			{
				bitsLeft -= 8;
				result[index++] = ( byte )( ( buffer >> bitsLeft ) & 0xFF );
			}
		}

		return result;
	}
}
