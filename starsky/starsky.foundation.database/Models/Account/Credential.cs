namespace starsky.foundation.database.Models.Account;

public class Credential
{
	/// <summary>
	///     Database Id
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///     The user id
	/// </summary>
	public int UserId { get; set; }

	public int CredentialTypeId { get; set; }

	/// <summary>
	///     Email address
	/// </summary>
	public string? Identifier { get; set; }

	/// <summary>
	///     Password
	/// </summary>
	public string? Secret { get; set; }

	/// <summary>
	///     Some hash
	/// </summary>
	public string? Extra { get; set; }

	public User? User { get; set; }
	public CredentialType? CredentialType { get; set; }
	public IterationCountType IterationCount { get; set; }
}
