using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Interfaces
{
    public enum SignUpResultError
    {
        CredentialTypeNotFound,
	    NullString
    }
    
    public class SignUpResult
    {
        public User User { get; }
        public bool Success { get; private set; }
        public SignUpResultError? Error { get; }
        
        public SignUpResult(User user = null, bool success = false, SignUpResultError? error = null)
        {
            User = user;
            Success = success;
            Error = error;
        }
    }
    
    public enum ValidateResultError
    {
        CredentialTypeNotFound,
        CredentialNotFound,
        SecretNotValid
    }
    
    public class ValidateResult
    {
        public User User { get; set; }
        public bool Success { get; set; }
        public ValidateResultError? Error { get; set; }
        
        public ValidateResult(User user = null, bool success = false, ValidateResultError? error = null)
        {
            User = user;
            Success = success;
            Error = error;
        }
    }
    
    public enum ChangeSecretResultError
    {
        CredentialTypeNotFound,
        CredentialNotFound
    }

    public class ChangeSecretResult
    {
        public bool Success { get; set; }

        public ChangeSecretResultError? Error { get; set; }
        
        public ChangeSecretResult(bool success = false, ChangeSecretResultError? error = null)
        {
            Success = success;
            Error = error;
        }
    }

    public interface IUserManager
    {
	    List<User> AllUsers();
	    
        SignUpResult SignUp(string name, string credentialTypeCode, string identifier, string secret);
        
        void AddToRole(User user, string roleCode);
        void AddToRole(User user, Role role);
        void RemoveFromRole(User user, string roleCode);
        void RemoveFromRole(User user, Role role);
        ChangeSecretResult ChangeSecret(string credentialTypeCode, string identifier, string secret);
        ValidateResult Validate(string credentialTypeCode, string identifier, string secret);
        Task SignIn(HttpContext httpContext, User user, bool isPersistent = false);
        void SignOut(HttpContext httpContext);
        int GetCurrentUserId(HttpContext httpContext);
        User GetCurrentUser(HttpContext httpContext);
        User GetUser(string credentialTypeCode, string identifier);
        Credential GetCredentialsByUserId(int userId);
        ValidateResult RemoveUser(string credentialTypeCode, string identifier);
        User Exist(string identifier);
        Role GetRole(string credentialTypeCode, string identifier);
        bool PreflightValidate(string userName, string password, string confirmPassword);
    }
}
