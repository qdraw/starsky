using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace starsky.foundation.platform.Helpers;

public static class Sha256
{
	public static string ComputeSha256(string input)
	{
		if ( string.IsNullOrEmpty(input) ) return string.Empty;
		var hash = string.Empty;
		// Initialize a SHA256 hash object
		using (SHA256 sha256 = SHA256.Create())
		{
			// Compute the hash of the given string
			var hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
			// Convert the byte array to string format
			hash = hashValue.Aggregate(hash, (current, b) => current + $"{b:X2}");
		}
		return hash;
	}
}
