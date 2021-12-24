// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.accountmanagement.Models.Account;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.accountmanagement.Services
{
	[Service(typeof(IUserManager), InjectionLifetime = InjectionLifetime.Scoped)]
	public class UserManager : IUserManager
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IMemoryCache _cache;
		private readonly AppSettings _appSettings;

		public UserManager(ApplicationDbContext dbContext, AppSettings appSettings,
			IMemoryCache memoryCache = null )
		{
			_dbContext = dbContext;
			_cache = memoryCache;
			_appSettings = appSettings;
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
		private List<Role> AddDefaultRoles()
		{
			// User.HasClaim(ClaimTypes.Role, "Administrator") -- > p.Code

			var existingRoleNames = new List<string>
			{
				AccountRoles.AppAccountRoles.User.ToString(),
				AccountRoles.AppAccountRoles.Administrator.ToString(),
			};
			
			var roles = new List<Role>();
			foreach ( var roleName in existingRoleNames )
			{
				Role role = _dbContext.Roles
					.TagWith("AddDefaultRoles")
					.FirstOrDefault(p => p.Code.ToLower().Equals(roleName.ToLower()));

				if ( role == null )
				{
					role = new Role
					{
						Code = roleName,
						Name = roleName,
					};
					_dbContext.Roles.Add(role);
					_dbContext.SaveChanges();
				}

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
		private CredentialType AddDefaultCredentialType(string credentialTypeCode)
		{
			CredentialType credentialType = _dbContext
				.CredentialTypes.TagWith("AddDefaultCredentialType")
				.FirstOrDefault(p => p.Code.ToLower()
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


		private const string AllUsersCacheKey = "UserManager_AllUsers";
		
		/// <summary>
		/// Return the number of users in the database
		/// </summary>
		/// <returns></returns>
		public async Task<List<User>> AllUsersAsync()
		{
			List<User> allUsers;
			if (IsCacheEnabled() && _cache.TryGetValue(AllUsersCacheKey, out var objectAllUsersResult))
			{
				allUsers = ( List<User> ) objectAllUsersResult;
			}
			else
			{
				allUsers = await _dbContext.Users.TagWith("AllUsersAsync").ToListAsync();
				if(IsCacheEnabled())
					_cache.Set(AllUsersCacheKey, allUsers, 
						new TimeSpan(99,0,0));
			}

			return allUsers;
		}

		/// <summary>
		/// Add one user to cached value
		/// </summary>
		internal async Task AddUserToCache(User user)
		{
			if ( !IsCacheEnabled() ) return;
			var allUsers = await AllUsersAsync();
			if ( allUsers.Any(p => p.Id == user.Id) )
			{
				var indexOf = allUsers.IndexOf(allUsers.Find(p => p.Id == user.Id));
				allUsers[indexOf] = user;
			}
			else
			{
				allUsers.Add(user);
			}
			_cache.Set(AllUsersCacheKey, allUsers, 
				new TimeSpan(99,0,0));
		}
	    
		/// <summary>
		/// Remove one user from cache
		/// </summary>
		private async Task RemoveUserFromCacheAsync(User user)
		{
			if ( !IsCacheEnabled() ) return;
			var allUsers = await AllUsersAsync();
			allUsers.Remove(user);
			_cache.Set("UserManager_AllUsers", allUsers, 
				new TimeSpan(99,0,0));
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

		public async Task<User> Exist(int userTableId)
		{
			if ( !IsCacheEnabled() )
			{
				return await _dbContext.Users.FirstOrDefaultAsync(p => p.Id == userTableId);
			}
			
			var users = await AllUsersAsync();
			return users.FirstOrDefault(p => p.Id == userTableId);
		}

		/// <summary>
		/// Add a new user, including Roles and UserRoles
		/// </summary>
		/// <param name="name">Nice Name, default string.Emthy</param>
		/// <param name="credentialTypeCode">default is: Email</param>
		/// <param name="identifier">an email address, e.g. dont@mail.us</param>
		/// <param name="secret">Password</param>
		/// <returns>result object</returns>
		public async Task<SignUpResult> SignUpAsync(string name,
			string credentialTypeCode, string identifier, string secret)
		{
			var credentialType = AddDefaultCredentialType(credentialTypeCode);
			var roles = AddDefaultRoles();
			AddDefaultPermissions();
			AddDefaultRolePermissions();
	        
			if ( string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(secret))
			{
				return new SignUpResult(success: false, error: SignUpResultError.NullString);
			}
	        
			// The email is stored in the Credentials database
			var user = Exist(identifier);
			if ( user == null )
			{
				// Check if user not already exist
				var createdDate = DateTime.UtcNow;
				user = new User
				{
					Name = name,
					Created = createdDate
				};
		        
				await _dbContext.Users.AddAsync(user);
				await _dbContext.SaveChangesAsync();
				await AddUserToCache(user);

				// to get the Id
				user = await _dbContext.Users.FirstOrDefaultAsync(p => p.Created == createdDate);
		        
				if ( user == null ) throw new AggregateException("user should not be null");
			}

			// Add a user role based on a user id
			AddToRole(user, roles.FirstOrDefault( p=> p.Code == _appSettings.AccountRegisterDefaultRole.ToString()));

			if (credentialType == null)
			{
				return new SignUpResult(success: false, error: SignUpResultError.CredentialTypeNotFound);
			}

			var credential = await _dbContext.Credentials.FirstOrDefaultAsync(p => p.Identifier == identifier);
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
			await _dbContext.Credentials.AddAsync(credential);
			await _dbContext.SaveChangesAsync();

			return new SignUpResult(user: user, success: true);
		}
       
		/// <summary>
		///  Add a link between the user and the role (for example Admin)
		/// </summary>
		/// <param name="user">AccountUser object</param>
		/// <param name="roleCode">RoleCode</param>
		public void AddToRole(User user, string roleCode)
		{
			Role role = _dbContext.Roles.TagWith("AddToRole").FirstOrDefault(r => r.Code == roleCode);

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
			UserRole userRole = _dbContext.UserRoles.FirstOrDefault(p => p.User.Id == user.Id);
			
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
		public void RemoveFromRole(User user, string roleCode)
		{
			Role role = _dbContext.Roles.TagWith("RemoveFromRole").FirstOrDefault(
				r => string.Equals(r.Code, roleCode, StringComparison.OrdinalIgnoreCase));
            
			if (role == null)
			{
				return;                
			}
            
			RemoveFromRole(user, role);
		}
        
		public void RemoveFromRole(User user, Role role)
		{
			UserRole userRole = _dbContext.UserRoles.Find(user.Id, role.Id);
            
			if (userRole == null)
			{
				return;
			}
            
			_dbContext.UserRoles.Remove(userRole);
			_dbContext.SaveChanges();
		}
        
		public ChangeSecretResult ChangeSecret(string credentialTypeCode, string identifier, string secret)
		{
			var credentialType = _dbContext.CredentialTypes.FirstOrDefault(
				ct => ct.Code.ToLower().Equals(credentialTypeCode.ToLower()));
	        
			if (credentialType == null)
			{
				return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialTypeNotFound);
			}
            
			Credential credential = _dbContext.Credentials.TagWith("ChangeSecret").FirstOrDefault(
				c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
            
			if (credential == null)
			{
				return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialNotFound);
			}
            
			var salt = Pbkdf2Hasher.GenerateRandomSalt();
			var hash = Pbkdf2Hasher.ComputeHash(secret, salt);
            
			credential.Secret = hash;
			credential.Extra = Convert.ToBase64String(salt);
			_dbContext.Credentials.Update(credential);
			_dbContext.SaveChanges();
            
			if ( IsCacheEnabled() )
			{
				_cache.Set(CredentialCacheKey(credentialType, identifier), 
					credential,new TimeSpan(99,0,0));
			}
            
			return new ChangeSecretResult(success: true);
		}

		internal string CredentialCacheKey(CredentialType credentialType, string identifier)
		{
			return "credential_" + credentialType.Id + "_" + identifier;
		}

		/// <summary>
		/// Get the credential by cache data
		/// </summary>
		/// <param name="credentialType">email</param>
		/// <param name="identifier">the id</param>
		/// <returns>Credential data object</returns>
		internal Credential CachedCredential(CredentialType credentialType, string identifier)
		{
			var key = CredentialCacheKey(credentialType, identifier);
	        
			// Add caching for credentialType
			if (IsCacheEnabled() && _cache.TryGetValue(key, 
				out var objectCredentialTypeCode))
			{
				return ( Credential ) objectCredentialTypeCode;
			}

			var credential = _dbContext.Credentials.TagWith("Credential").FirstOrDefault(
				c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);

			if ( IsCacheEnabled() && credential != null )
			{
				_cache.Set(key, credential,new TimeSpan(99,0,0));
			}

			return credential;
		}
        

		/// <summary>
		/// Get the CredentialType by the credentialTypeCode
		/// </summary>
		/// <param name="credentialTypeCode">code to get the CredentialType</param>
		/// <returns>CredentialType</returns>
		private CredentialType CachedCredentialType(string credentialTypeCode)
		{
			var cacheKey = "credentialTypeCode_" + credentialTypeCode;
			// Add caching for credentialType
			if (IsCacheEnabled() && _cache.TryGetValue(cacheKey, 
				out var objectCredentialTypeCode))
			{
				return ( CredentialType ) objectCredentialTypeCode;
			}

			var credentialType = _dbContext.CredentialTypes.TagWith("CredentialType").FirstOrDefault(
				ct => ct.Code.ToLower().Equals(credentialTypeCode.ToLower()));
			if ( IsCacheEnabled() && credentialType != null )
			{
				_cache.Set(cacheKey, credentialType, 
					new TimeSpan(99,0,0));
			}
			return credentialType;
		}

		public bool PreflightValidate(string userName, string password, string confirmPassword)
		{
			var model = new RegisterViewModel
			{
				Email = userName, 
				Password = password, 
				ConfirmPassword = confirmPassword
			};
	        
			var context = new ValidationContext(model, null, null);
			var results = new List<ValidationResult>();
			return Validator.TryValidateObject(
				model, context, results, 
				true
			);
		}

		/// <summary>
		/// Is the username and password combination correct
		/// </summary>
		/// <param name="credentialTypeCode">default: email</param>
		/// <param name="identifier">email</param>
		/// <param name="secret">password</param>
		/// <returns>status</returns>
		public async Task<ValidateResult> ValidateAsync(string credentialTypeCode,
			string identifier, string secret)
		{
			var credentialType = CachedCredentialType(credentialTypeCode);

			if (credentialType == null)
			{
				return new ValidateResult(success: false, error: ValidateResultError.CredentialTypeNotFound);
			}

			var credential = CachedCredential(credentialType, identifier);
            
			if (credential == null)
			{
				return new ValidateResult(success: false, error: ValidateResultError.CredentialNotFound);
			}
            
			// No Password
			if ( string.IsNullOrWhiteSpace(secret) )
			{
				return new ValidateResult(success: false, error: ValidateResultError.SecretNotValid);
			}
			
			var userData = (await AllUsersAsync()).FirstOrDefault(p => p.Id == credential.UserId);
			if ( userData == null )
			{
				return new ValidateResult(success: false, error: ValidateResultError.UserNotFound);
			}

			if ( userData.LockoutEnabled && userData.LockoutEnd >= DateTime.UtcNow )
			{
				return new ValidateResult(success: false, error: ValidateResultError.Lockout);
			}
            
			// To compare the secret
			byte[] salt = Convert.FromBase64String(credential.Extra);
			string hashedPassword = Pbkdf2Hasher.ComputeHash(secret, salt);

			if ( credential.Secret == hashedPassword )
			{
				return await ResetAndSuccess(userData.AccessFailedCount, credential.UserId, userData);
			}

			return await SetLockIfFailedCountIsToHigh(credential.UserId);
		}

		private async Task<ValidateResult> ResetAndSuccess(int accessFailedCount, int userId, User userData )
		{
			if ( accessFailedCount <= 0 )
				return new ValidateResult(userData, true);
			
			userData = await _dbContext.Users.FindAsync(userId);
			userData.LockoutEnabled = false;
			userData.AccessFailedCount = 0;
			userData.LockoutEnd = DateTime.MinValue;
			await _dbContext.SaveChangesAsync();
			await AddUserToCache(userData);
			
			return new ValidateResult(userData, true);
		}

		private async Task<ValidateResult> SetLockIfFailedCountIsToHigh(int userId)
		{
			var errorReason = ValidateResultError.SecretNotValid;
			// ReSharper disable once SuggestVarOrType_SimpleTypes
			User userData = await _dbContext.Users.FindAsync(userId);
			userData.AccessFailedCount++;
			if ( userData.AccessFailedCount >= 3 )
			{
				userData.LockoutEnabled = true;
				userData.AccessFailedCount = 0;
				errorReason = ValidateResultError.Lockout;
				userData.LockoutEnd = DateTime.UtcNow.AddHours(1);
			}
			await _dbContext.SaveChangesAsync();
			await AddUserToCache(userData);
			return new ValidateResult(success: false, error: errorReason);
		}
        
		public async Task SignIn(HttpContext httpContext, User user, bool isPersistent = false)
		{
			ClaimsIdentity identity = new ClaimsIdentity(
				GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
			ClaimsPrincipal principal = new ClaimsPrincipal(identity);
			
			await httpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme, 
				principal, 
				new AuthenticationProperties() { IsPersistent = isPersistent}
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
		public async Task<ValidateResult> RemoveUser(string credentialTypeCode,
			string identifier)
		{
			var credentialType = CachedCredentialType(credentialTypeCode);
			Credential credential = _dbContext.Credentials.FirstOrDefault(
				c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
			var user = await _dbContext.Users.FirstOrDefaultAsync(p => p.Id == credential.UserId);

			var userRole = await _dbContext.UserRoles.FirstOrDefaultAsync(p => p.UserId == credential.UserId);
	        
			if(userRole == null || user == null || credential == null) return new 
				ValidateResult{Success = false, Error = ValidateResultError.CredentialNotFound};

			_dbContext.Credentials.Remove(credential);
			_dbContext.Users.Remove(user);
			_dbContext.UserRoles.Remove(userRole);
			await _dbContext.SaveChangesAsync();
	        
			await RemoveUserFromCacheAsync(user);
	        
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

		public User GetUser(string credentialTypeCode, string identifier)
		{
			var credentialType = CachedCredentialType(credentialTypeCode);
			Credential credential = _dbContext.Credentials.FirstOrDefault(
				c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
			if ( credential == null ) return null;
			return _dbContext.Users.TagWith("GetUser").FirstOrDefault(p => p.Id == credential.UserId);
		}

		public Role GetRole(string credentialTypeCode, string identifier)
		{
			var user = GetUser(credentialTypeCode, identifier);
			var role = _dbContext.UserRoles.FirstOrDefault(p => p.User.Id == user.Id);
			if ( role == null ) return new Role();
			var roleId = role.RoleId;
			return _dbContext.Roles.TagWith("GetRole").FirstOrDefault(p=> p.Id == roleId);
		}

		public Credential GetCredentialsByUserId(int userId)
		{
			return _dbContext.Credentials
				.TagWith("GetCredentialsByUserId")
				.FirstOrDefault(p => p.UserId == userId);
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
			IEnumerable<int> roleIds = _dbContext.UserRoles.TagWith("GetUserRoleClaims").Where(
				ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();

			foreach (var roleId in roleIds)
			{
				Role role = _dbContext.Roles.Find(roleId);
                    
				claims.Add(new Claim(ClaimTypes.Role, role.Code));
				claims.AddRange(GetUserPermissionClaims(role));
			}
			return claims;
		}
        
		internal IEnumerable<Claim> GetUserPermissionClaims(Role role)
		{
			List<Claim> claims = new List<Claim>();
			var rolePermissions = _dbContext.RolePermissions.Where(
				rp => rp.RoleId == role.Id);
			IEnumerable<int> permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

			foreach (var permissionId in permissionIds)
			{
				Permission permission = _dbContext.Permissions.Find(permissionId);
                    
				claims.Add(new Claim("Permission", permission.Code));
			}

			return claims;
		}

		public enum AppPermissions
		{
			AppSettingsWrite = 10,
		}
        
		private static readonly List<AppPermissions> AllPermissions = new List<AppPermissions>
		{
			AppPermissions.AppSettingsWrite,
		};
        
		private void AddDefaultPermissions()
		{
			foreach ( var permissionEnum in AllPermissions )
			{
				var permission = _dbContext.Permissions.FirstOrDefault(p => p.Code == permissionEnum.ToString());

				if ( permission != null ) continue;
		        
				permission = new Permission()
				{
					Name = permissionEnum.ToString(),
					Code = permissionEnum.ToString(),
					Position = (int) permissionEnum,
				};
				_dbContext.Permissions.Add(permission);
				_dbContext.SaveChanges();
			}
		}

		private void AddDefaultRolePermissions()
		{
			var existingRolePermissions = new List<KeyValuePair<string,AppPermissions>>
			{
				new KeyValuePair<string, AppPermissions>(
					AccountRoles.AppAccountRoles.Administrator.ToString(), AppPermissions.AppSettingsWrite),
			};
	        
			foreach ( var rolePermissionsDictionary in existingRolePermissions )
			{
				var role = _dbContext.Roles.TagWith("AddDefaultRolePermissions").FirstOrDefault(p => p.Code == rolePermissionsDictionary.Key );
				var permission = _dbContext.Permissions.FirstOrDefault(p => 
					p.Code == rolePermissionsDictionary.Value.ToString());

				if ( permission == null || role == null ) continue;
				var rolePermission = _dbContext.RolePermissions.FirstOrDefault(p =>
					p.RoleId == role.Id && p.PermissionId == permission.Id);

				if ( rolePermission != null )
				{
					continue;
				}

				rolePermission = new RolePermission
				{
					RoleId = role.Id,
					PermissionId = permission.Id
				};
		        
				_dbContext.RolePermissions.Add(rolePermission);
				_dbContext.SaveChanges();
			}
		}
	}
}
