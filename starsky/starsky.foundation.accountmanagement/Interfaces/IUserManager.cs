using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Interfaces;

public enum SignUpResultError
{
	CredentialTypeNotFound,
	NullString
}

public class SignUpResult
{
	public SignUpResult(User? user = null, bool success = false,
		SignUpResultError? error = null)
	{
		User = user;
		Success = success;
		Error = error;
	}

	public User? User { get; }
	public bool Success { get; private set; }
	public SignUpResultError? Error { get; }
}

public enum ValidateResultError
{
	CredentialTypeNotFound,
	CredentialNotFound,
	SecretNotValid,
	Lockout,
	UserNotFound
}

public enum ChangeSecretResultError
{
	CredentialTypeNotFound,
	CredentialNotFound
}

public class ChangeSecretResult
{
	public ChangeSecretResult(bool success = false, ChangeSecretResultError? error = null)
	{
		Success = success;
		Error = error;
	}

	public bool Success { get; set; }

	public ChangeSecretResultError? Error { get; set; }
}

public interface IUserManager
{
	Task<UserOverviewModel> AllUsersAsync();

	/// <summary>
	///     Add a new user, including Roles and UserRoles
	/// </summary>
	/// <param name="name">Nice Name, default string.Empty</param>
	/// <param name="credentialTypeCode">default is: Email</param>
	/// <param name="identifier">an email address, e.g. dont@mail.us</param>
	/// <param name="secret">Password</param>
	/// <returns>result object</returns>
	Task<SignUpResult> SignUpAsync(string name, string credentialTypeCode,
		string? identifier, string? secret);

	void AddToRole(User user, string roleCode);
	void AddToRole(User user, Role role);
	void RemoveFromRole(User user, string roleCode);
	void RemoveFromRole(User user, Role role);

	ChangeSecretResult ChangeSecret(string credentialTypeCode, string? identifier,
		string secret);

	Task<ValidateResult> ValidateAsync(string credentialTypeCode,
		string? identifier, string secret);

	Task<bool> SignIn(HttpContext httpContext, User? user,
		bool isPersistent = false);

	void SignOut(HttpContext httpContext);
	int GetCurrentUserId(HttpContext httpContext);
	User? GetCurrentUser(HttpContext httpContext);
	User? GetUser(string credentialTypeCode, string identifier);
	Credential? GetCredentialsByUserId(int userId);

	Task<ValidateResult> RemoveUser(string credentialTypeCode,
		string identifier);

	User? Exist(string identifier);

	Task<User?> ExistAsync(int userTableId);
	Role? GetRole(string credentialTypeCode, string identifier);

	Task<Role?> GetRoleAsync(int userId);
	bool PreflightValidate(string userName, string password, string confirmPassword);
	CredentialType? GetCachedCredentialType(string credentialTypeCode);
}
