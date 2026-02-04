namespace starsky.foundation.database.Models.Account;

public enum IterationCountType
{
	Default = 0,
	IterateLegacySha1 = 10_000,
	Iterate100KSha256 = 100_000,
}
