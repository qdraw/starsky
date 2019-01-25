﻿// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

namespace starskycore.Services
{
public class UserManager : IUserManager
    {
        private readonly ApplicationDbContext _storage;
	    private readonly IMemoryCache _cache;

	    public UserManager(ApplicationDbContext storage,
	        IMemoryCache memoryCache = null )
        {
            _storage = storage;
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
			    Role role = _storage.Roles.FirstOrDefault(p => p.Code.ToLowerInvariant() == roleName.ToLowerInvariant());

			    if ( role == null )
			    {
				    role = new Role
				    {
					    Code = roleName,
					    Name = roleName,
				    };
				    _storage.Roles.Add(role);
			    }
			    _storage.SaveChanges();

			    // Get the Int Ids from the database
			    role = _storage.Roles.FirstOrDefault(p => p.Code.ToLowerInvariant() == roleName.ToLowerInvariant());
			    
			    roles.Add(role);
		    }
			
			return roles;
	    }

	    /// <summary>
	    /// The default username is an email-adres, this is added as default value to an empty database (and checks this list)
	    /// </summary>
	    /// <param name="credentialTypeCode">the type, for example email</param>
	    /// <returns></returns>
	    public CredentialType AddDefaultCredentialType(string credentialTypeCode)
	    {
		    CredentialType credentialType = _storage.CredentialTypes.FirstOrDefault(
			    ct => string.Equals(ct.Code, credentialTypeCode, StringComparison.OrdinalIgnoreCase));

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
			    _storage.CredentialTypes.Add(credentialType);
		    }

		    return credentialType;
	    }

        /// <summary>
        /// Add a new user, including Roles and UserRoles
        /// </summary>
        /// <param name="name">Nice Name, default string.Emthy</param>
        /// <param name="credentialTypeCode">usaly email</param>
        /// <param name="identifier">Email</param>
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
	        User user = null;
	        var credential = _storage.Credentials.FirstOrDefault(p => p.Identifier == identifier);
	        if ( credential != null )
	        {
		        user = _storage.Users.FirstOrDefault(p => p.Id == credential.UserId);		        
	        }
	        if ( user == null )
	        {
		        // Check if user not already exist
		        user = new User
		        {
			        Name = name,
			        Created = DateTime.Now
		        };
		        _storage.Users.Add(user);
		        _storage.SaveChanges();
		        
		        // to get the Id
		        user = _storage.Users.FirstOrDefault(p => p.Name == name);
	        }

			// Add a user role based on a user id
			AddToRole(user, roles.FirstOrDefault());

            if (credentialType == null)
            {
                return new SignUpResult(success: false, error: SignUpResultError.CredentialTypeNotFound);
            }


	        if ( credential == null && !string.IsNullOrEmpty(secret))
	        {
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
		        _storage.Credentials.Add(credential);
		        _storage.SaveChanges();
	        }

            return new SignUpResult(user: user, success: true);
        }
       
        
        public void AddToRole(User user, string roleCode)
        {
            Role role = _storage.Roles.FirstOrDefault(r => 
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
			UserRole userRole = _storage.UserRoles.Find(user.Id, role.Id);
			
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
			_storage.UserRoles.Add(userRole);
			_storage.SaveChanges();
        }
        
	    //  // Features are temp off // keep in code 
//        public void RemoveFromRole(User user, string roleCode)
//        {
//            Role role = _storage.Roles.FirstOrDefault(
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
//            UserRole userRole = _storage.UserRoles.Find(user.Id, role.Id);
//            
//            if (userRole == null)
//            {
//                return;
//            }
//            
//            _storage.UserRoles.Remove(userRole);
//            _storage.SaveChanges();
//        }
        
        public ChangeSecretResult ChangeSecret(string credentialTypeCode, string identifier, string secret)
        {
            CredentialType credentialType = _storage.CredentialTypes.FirstOrDefault(
                ct => string.Equals(ct.Code, credentialTypeCode, StringComparison.OrdinalIgnoreCase));
            
            if (credentialType == null)
            {
                return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialTypeNotFound);
            }
            
            Credential credential = _storage.Credentials.FirstOrDefault(
                c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
            
            if (credential == null)
            {
                return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialNotFound);
            }
            
            byte[] salt = Pbkdf2Hasher.GenerateRandomSalt();
            string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
            
            credential.Secret = hash;
            credential.Extra = Convert.ToBase64String(salt);
            _storage.Credentials.Update(credential);
            _storage.SaveChanges();
            return new ChangeSecretResult(success: true);
        }
        
       
        public ValidateResult Validate(string credentialTypeCode, string identifier, string secret)
        {
	        // Add caching for credentialType
	        CredentialType credentialType;
	        if (IsCacheEnabled() && _cache.TryGetValue("credentialTypeCode_" + credentialTypeCode, out var objectCredentialTypeCode))
	        {
		        credentialType = ( CredentialType ) objectCredentialTypeCode;
	        }
	        else
	        {
		        credentialType = _storage.CredentialTypes.FirstOrDefault(
			        ct => string.Equals(ct.Code, credentialTypeCode, StringComparison.OrdinalIgnoreCase));
		        if(IsCacheEnabled()) _cache.Set("credentialTypeCode_" + credentialTypeCode, credentialType, new TimeSpan(99,0,0));
	        }

	        
            
            
            if (credentialType == null)
            {
                return new ValidateResult(success: false, error: ValidateResultError.CredentialTypeNotFound);
            }
            
            Credential credential = _storage.Credentials.FirstOrDefault(
                c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);

            if (credential == null)
            {
                return new ValidateResult(success: false, error: ValidateResultError.CredentialNotFound);
            }
            
            if (!string.IsNullOrEmpty(secret))
            {
                byte[] salt = Convert.FromBase64String(credential.Extra);
                string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
                
                if (credential.Secret != hash)
                    return new ValidateResult(success: false, error: ValidateResultError.SecretNotValid);
            }

	        // Cache ValidateResult always query on passwords and return result
	        ValidateResult validateResult; 
	        if (IsCacheEnabled() && _cache.TryGetValue("ValidateResult_" + credential.UserId, out var objectValidateResult))
	        {
		        validateResult = ( ValidateResult ) objectValidateResult;
	        }
	        else
	        {
		        validateResult = new ValidateResult(user: this._storage.Users.Find(credential.UserId), success: true);
		        if(IsCacheEnabled()) _cache.Set("ValidateResult_" + credential.UserId, validateResult, new TimeSpan(99,0,0));
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
            
            int currentUserId;
            
            if (!int.TryParse(claim.Value, out currentUserId))
            {
                return -1;
            }
            
            return currentUserId;
        }
        
        public User GetCurrentUser(HttpContext httpContext)
        {
            int currentUserId = this.GetCurrentUserId(httpContext);
            
            if (currentUserId == -1)
            {
                return null;
            }
            
            return this._storage.Users.Find(currentUserId);
        }
            
        private IEnumerable<Claim> GetUserClaims(User user)
        {
            List<Claim> claims = new List<Claim>();
            
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            claims.AddRange(GetUserRoleClaims(user));
            return claims;
        }
        
        private IEnumerable<Claim> GetUserRoleClaims(User user)
        {
            List<Claim> claims = new List<Claim>();
            IEnumerable<int> roleIds = this._storage.UserRoles.Where(
                ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
            
            if (roleIds != null)
            {
                foreach (int roleId in roleIds)
                {
                    Role role = this._storage.Roles.Find(roleId);
                    
                    claims.Add(new Claim(ClaimTypes.Role, role.Code));
                    claims.AddRange(this.GetUserPermissionClaims(role));
                }
            }
            return claims;
        }
        
        private IEnumerable<Claim> GetUserPermissionClaims(Role role)
        {
            List<Claim> claims = new List<Claim>();
            IEnumerable<int> permissionIds = this._storage.RolePermissions.Where(
                rp => rp.RoleId == role.Id).Select(rp => rp.PermissionId).ToList();
            
            if (permissionIds != null)
            {
                foreach (int permissionId in permissionIds)
                {
                    Permission permission = _storage.Permissions.Find(permissionId);
                    
                    claims.Add(new Claim("Permission", permission.Code));
                }
            }
        
            return claims;
        }
    }
}