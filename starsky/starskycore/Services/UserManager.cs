// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models.Account;

[assembly: InternalsVisibleTo("starskytest")]
namespace starskycore.Services
{
public class UserManager : IUserManager
    {
        private readonly ApplicationDbContext _dbContext;
	    private readonly IMemoryCache _cache;

	    public UserManager(ApplicationDbContext dbContext,
	        IMemoryCache memoryCache = null )
        {
            _dbContext = dbContext;
	        _cache = memoryCache;
        }
	    
	    private bool IsCacheEnabled()
	    {
		    // || _appSettings?.AddMemoryCache == false > disabled
		    if( _cache == null ) return false;
		    return true;
	    }
	    
	    
	    /// <summary>
	    /// Add the roles 'User' and 'Administrator' to an empty database (and checks this list)
	    /// </summary>
	    /// <returns>List of roles in existingRoleNames</returns>
		public List<Role> AddDefaultRoles()
		{
			// User.HasClaim(ClaimTypes.Role, "Administrator") -- > p.Code

			var existingRoleNames = new List<string>
			{
				"User",
				"Administrator",
			};
		    var roles = new List<Role>();
		    foreach ( var roleName in existingRoleNames )
		    {
			    Role role = _dbContext.Roles.FirstOrDefault(p => p.Code.ToLower().Equals(roleName.ToLower()));

			    if ( role == null )
			    {
				    role = new Role
				    {
					    Code = roleName,
					    Name = roleName,
				    };
				    _dbContext.Roles.Add(role);
			    }
			    _dbContext.SaveChanges();

			    // Get the Int Ids from the database
			    role = _dbContext.Roles.FirstOrDefault(p => p.Code.ToLower().Equals(roleName.ToLower()));
			    
			    roles.Add(role);
		    }
			
			return roles;
	    }

	    /// <summary>
	    /// The default username is an email-address, this is added as default value to an empty database (and checks this list)
	    /// </summary>
	    /// <param name="credentialTypeCode">the type, for example email</param>
	    /// <returns></returns>
	    public CredentialType AddDefaultCredentialType(string credentialTypeCode)
	    {
		     CredentialType credentialType = _dbContext
			     .CredentialTypes.FirstOrDefault(p => p.Code.ToLower()
				     .Equals(credentialTypeCode.ToLower()));

		    // When not exist add it
		    if (credentialType == null && credentialTypeCode.ToLower() == "email" )
		    {
			    credentialType = new CredentialType
			    {
				    Code = "email",
				    Name = "email",
				    Position = 1,
				    Id = 1
			    };
			    _dbContext.CredentialTypes.Add(credentialType);
		    }

		    return credentialType;
	    }

	    /// <summary>
	    /// Return the number of users in the database
	    /// </summary>
	    /// <returns></returns>
	    public List<User> AllUsers()
	    {
		    List<User> allUsers; 
		    
		    if (IsCacheEnabled() && _cache.TryGetValue("UserManager_AllUsers", out var objectAllUsersResult))
		    {
			    allUsers = ( List<User> ) objectAllUsersResult;
		    }
		    else
		    {
			    allUsers = _dbContext.Users.ToList();
			    if(IsCacheEnabled())
				    _cache.Set("UserManager_AllUsers", allUsers, new TimeSpan(99,0,0));
		    }

		    return allUsers;
	    }

	    /// <summary>
	    /// Add one user to cached value
	    /// </summary>
	    internal void AddUserToCache(User user)
	    {
		    if ( !IsCacheEnabled() ) return;
		    var allUsers = AllUsers();
		    if(allUsers.Contains(user)) return;
		    allUsers.Add(user);
		    _cache.Set("UserManager_AllUsers", allUsers, new TimeSpan(99,0,0));
	    }
	    
	    /// <summary>
	    /// Remove one user from cache
	    /// </summary>
	    internal void RemoveUserFromCache(User user)
	    {
		    if ( !IsCacheEnabled() ) return;
		    var allUsers = AllUsers();
		    allUsers.Remove(user);
		    _cache.Set("UserManager_AllUsers", allUsers, new TimeSpan(99,0,0));
	    }

	    /// <summary>
	    /// Check if the user exist by username
	    /// </summary>
	    /// <param name="identifier">email</param>
	    /// <returns>null or user</returns>
	    public User Exist(string identifier)
	    {
		    var credential = _dbContext.Credentials.FirstOrDefault(p => p.Identifier == identifier);
		    if ( credential == null ) return null;

		    var user = _dbContext.Users.FirstOrDefault(p => p.Id == credential.UserId);
		    return user;
	    }

	    /// <summary>
        /// Add a new user, including Roles and UserRoles
        /// </summary>
        /// <param name="name">Nice Name, default string.Emthy</param>
        /// <param name="credentialTypeCode">default is: Email</param>
        /// <param name="identifier">an email address, e.g. dont@mail.us</param>
        /// <param name="secret">Password</param>
        /// <returns></returns>
        public SignUpResult SignUp(string name, string credentialTypeCode, string identifier, string secret)
        {
	        var credentialType = AddDefaultCredentialType(credentialTypeCode);
	        var roles = AddDefaultRoles();
	        
	        if ( string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(secret))
	        {
				return new SignUpResult(success: false, error: SignUpResultError.NullString);
	        }
	        
	        // The email is stored in the Credentials database
	        var user = Exist(identifier);
	        if ( user == null )
	        {
		        // Check if user not already exist
		        var createdDate = DateTime.Now;
		        user = new User
		        {
			        Name = name,
			        Created = createdDate
		        };
		        
		        _dbContext.Users.Add(user);
		        _dbContext.SaveChanges();
		        AddUserToCache(user);

		        // to get the Id
		        user = _dbContext.Users.FirstOrDefault(p => p.Created == createdDate);
	        }

			// Add a user role based on a user id
			AddToRole(user, roles.FirstOrDefault());

            if (credentialType == null)
            {
                return new SignUpResult(success: false, error: SignUpResultError.CredentialTypeNotFound);
            }

            var credential = _dbContext.Credentials.FirstOrDefault(p => p.Identifier == identifier);
            if ( credential != null ) return new SignUpResult(user: user, success: true);
            
            // Check if credential not already exist
            credential = new Credential
            {
	            UserId = user.Id,
	            CredentialTypeId = credentialType.Id,
	            Identifier = identifier
            };
            byte[] salt = Pbkdf2Hasher.GenerateRandomSalt();
            string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
                
            credential.Secret = hash;
            credential.Extra = Convert.ToBase64String(salt);
            _dbContext.Credentials.Add(credential);
            _dbContext.SaveChanges();

            return new SignUpResult(user: user, success: true);
        }
       
        
        public void AddToRole(User user, string roleCode)
        {
            Role role = _dbContext.Roles.FirstOrDefault(r => 
                string.Equals(r.Code, roleCode, StringComparison.OrdinalIgnoreCase));

            if (role == null)
            {
                return;                
            }
            
            AddToRole(user, role);
        }
   
	    /// <summary>
	    ///  Add a link between the user and the role (for example Admin)
	    /// </summary>
	    /// <param name="user">AccountUser object</param>
	    /// <param name="role">Role object</param>
        public void AddToRole(User user, Role role)
        {
			UserRole userRole = _dbContext.UserRoles.Find(user.Id, role.Id);
			
			if (userRole != null)
			{
				return;                
			}

	        // Add a user role based on a user id
			userRole = new UserRole
			{
				UserId = user.Id,
				RoleId = role.Id
			};
			_dbContext.UserRoles.Add(userRole);
			_dbContext.SaveChanges();
        }
        
	    //  // Features are temp off // keep in code 
//        public void RemoveFromRole(User user, string roleCode)
//        {
//            Role role = _dbContext.Roles.FirstOrDefault(
//                r => string.Equals(r.Code, roleCode, StringComparison.OrdinalIgnoreCase));
//            
//            if (role == null)
//            {
//                return;                
//            }
//            
//            RemoveFromRole(user, role);
//        }
//        
//        public void RemoveFromRole(User user, Role role)
//        {
//            UserRole userRole = _dbContext.UserRoles.Find(user.Id, role.Id);
//            
//            if (userRole == null)
//            {
//                return;
//            }
//            
//            _dbContext.UserRoles.Remove(userRole);
//            _dbContext.SaveChanges();
//        }
        
        public ChangeSecretResult ChangeSecret(string credentialTypeCode, string identifier, string secret)
        {
	        var credentialType = _dbContext.CredentialTypes.FirstOrDefault(
		        ct => ct.Code.ToLower().Equals(credentialTypeCode.ToLower()));
	        
            if (credentialType == null)
            {
                return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialTypeNotFound);
            }
            
            Credential credential = _dbContext.Credentials.FirstOrDefault(
                c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
            
            if (credential == null)
            {
                return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialNotFound);
            }
            
            byte[] salt = Pbkdf2Hasher.GenerateRandomSalt();
            string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
            
            credential.Secret = hash;
            credential.Extra = Convert.ToBase64String(salt);
            _dbContext.Credentials.Update(credential);
            _dbContext.SaveChanges();
            return new ChangeSecretResult(success: true);
        }

        /// <summary>
        /// Get the CredentialType by the credentialTypeCode
        /// </summary>
        /// <param name="credentialTypeCode">code to get the CredentialType</param>
        /// <returns>CredentialType</returns>
        private CredentialType CachedCredentialType(string credentialTypeCode)
        {
	        // Add caching for credentialType
	        CredentialType credentialType;
	        if (IsCacheEnabled() && _cache.TryGetValue("credentialTypeCode_" + credentialTypeCode, 
		            out var objectCredentialTypeCode))
	        {
		        credentialType = ( CredentialType ) objectCredentialTypeCode;
	        }
	        else
	        {
		        credentialType = _dbContext.CredentialTypes.FirstOrDefault(
			        ct => ct.Code.ToLower().Equals(credentialTypeCode.ToLower()));
		        if(IsCacheEnabled() && credentialType != null ) 
			        _cache.Set("credentialTypeCode_" + credentialTypeCode, credentialType, 
				        new TimeSpan(99,0,0));
	        }

	        return credentialType;
        }
        
       
        /// <summary>
        /// Is the username and password combination correct
        /// </summary>
        /// <param name="credentialTypeCode">default: email</param>
        /// <param name="identifier">email</param>
        /// <param name="secret">password</param>
        /// <returns>status</returns>
        public ValidateResult Validate(string credentialTypeCode, string identifier, string secret)
        {
	        var credentialType = CachedCredentialType(credentialTypeCode);

	        if (credentialType == null)
            {
                return new ValidateResult(success: false, error: ValidateResultError.CredentialTypeNotFound);
            }
            
            Credential credential = _dbContext.Credentials.FirstOrDefault(
                c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);

            if (credential == null)
            {
                return new ValidateResult(success: false, error: ValidateResultError.CredentialNotFound);
            }
            
            // No Password
            if(string.IsNullOrWhiteSpace(secret))
	            return new ValidateResult(success: false, error: ValidateResultError.SecretNotValid); 
            
            
            // To compare the secret
            byte[] salt = Convert.FromBase64String(credential.Extra);
            string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
            
            if (credential.Secret != hash)
                return new ValidateResult(success: false, error: ValidateResultError.SecretNotValid);


	        // Cache ValidateResult always query on passwords and return result
	        ValidateResult validateResult; 
	        if (IsCacheEnabled() && _cache.TryGetValue("ValidateResult_" + credential.Secret + credential.UserId, out var objectValidateResult))
	        {
		        validateResult = ( ValidateResult ) objectValidateResult;
	        }
	        else
	        {
		        validateResult = new ValidateResult(user: this._dbContext.Users.Find(credential.UserId), success: true);
		        if(IsCacheEnabled())
			        _cache.Set("ValidateResult_" + credential.Secret + credential.UserId, 
				        validateResult, new TimeSpan(99,0,0));
	        }
            return validateResult;
        }
        
        public async Task SignIn(HttpContext httpContext, User user, bool isPersistent = false)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                principal, 
                new AuthenticationProperties() { IsPersistent = isPersistent }
            );
            // Required in the direct context;  when using a REST like call
            httpContext.User = principal;
        }

        /// <summary>
        /// Remove user from database
        /// </summary>
        /// <param name="credentialTypeCode">default: email</param>
        /// <param name="identifier">email address</param>
        /// <returns>status</returns>
        public ValidateResult RemoveUser(string credentialTypeCode, string identifier)
        {
	        var credentialType = CachedCredentialType(credentialTypeCode);
	        Credential credential = _dbContext.Credentials.FirstOrDefault(
		        c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
	        var user = _dbContext.Users.FirstOrDefault(p => p.Id == credential.UserId);

	        var userRole = _dbContext.UserRoles.FirstOrDefault(p => p.UserId == credential.UserId);
	        
	        if(userRole == null || user == null || credential == null) return new ValidateResult{Success = false, Error = ValidateResultError.CredentialNotFound};

	        _dbContext.Credentials.Remove(credential);
	        _dbContext.Users.Remove(user);
	        _dbContext.UserRoles.Remove(userRole);
	        _dbContext.SaveChanges();
	        
	        RemoveUserFromCache(user);
	        
	        return new ValidateResult{Success = true};
        }
        
        public async void SignOut(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        
        public int GetCurrentUserId(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return -1;
            }
            
            Claim claim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            
            if (claim == null)
            {
                return -1;
            }

            if (!int.TryParse(claim.Value, out var currentUserId))
            {
                return -1;
            }
            
            return currentUserId;
        }
        
        public User GetCurrentUser(HttpContext httpContext)
        {
	        int currentUserId = GetCurrentUserId(httpContext);
            
	        if (currentUserId == -1)
	        {
		        return null;
	        }
	        return _dbContext.Users.Find(currentUserId);
        }

        public Credential GetCredentialsByUserId(int userId)
        {
	        return _dbContext.Credentials.FirstOrDefault(p => p.UserId == userId);
		}

        private IEnumerable<Claim> GetUserClaims(User user)
        {
	        var claims = new List<Claim>
	        {
		        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
		        new Claim(ClaimTypes.Name, user.Name)
	        };

	        claims.AddRange(GetUserRoleClaims(user));
            return claims;
        }
        
        private IEnumerable<Claim> GetUserRoleClaims(User user)
        {
            var claims = new List<Claim>();
            IEnumerable<int> roleIds = this._dbContext.UserRoles.Where(
                ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();

            foreach (var roleId in roleIds)
            {
	            Role role = _dbContext.Roles.Find(roleId);
                    
	            claims.Add(new Claim(ClaimTypes.Role, role.Code));
	            claims.AddRange(GetUserPermissionClaims(role));
            }
            return claims;
        }
        
        private IEnumerable<Claim> GetUserPermissionClaims(Role role)
        {
            List<Claim> claims = new List<Claim>();
            IEnumerable<int> permissionIds = this._dbContext.RolePermissions.Where(
                rp => rp.RoleId == role.Id).Select(rp => rp.PermissionId).ToList();
            
            if (permissionIds != null)
            {
                foreach (int permissionId in permissionIds)
                {
                    Permission permission = _dbContext.Permissions.Find(permissionId);
                    
                    claims.Add(new Claim("Permission", permission.Code));
                }
            }
        
            return claims;
        }
    }
}
