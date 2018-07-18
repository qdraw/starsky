// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using starsky.Data;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models.Account;

namespace starsky.Services
{
public class UserManager : IUserManager
    {
        private ApplicationDbContext storage;
        
        public UserManager(ApplicationDbContext storage)
        {
            this.storage = storage;
        }
        
        public SignUpResult SignUp(string name, string credentialTypeCode, string identifier)
        {
            return this.SignUp(name, credentialTypeCode, identifier, null);
        }
        public SignUpResult SignUp(string name, string credentialTypeCode, string identifier, string secret)
        {
            User user = new User();
            
            user.Name = name;
            user.Created = DateTime.Now;
            this.storage.Users.Add(user);
            this.storage.SaveChanges();
            
            CredentialType credentialType = this.storage.CredentialTypes.FirstOrDefault(ct => string.Equals(ct.Code, credentialTypeCode, StringComparison.OrdinalIgnoreCase));

            // If not exist add it;
            if (credentialType == null && credentialTypeCode.ToLower() == "email" )
            {
                credentialType = new CredentialType
                {
                    Code = "email",
                    Name = "email",
                    Position = 1,
                    Id = 1
                };
                storage.CredentialTypes.Add(credentialType);
            }

            if (credentialType == null) 
                return new SignUpResult(success: false, error: SignUpResultError.CredentialTypeNotFound);
            
            Credential credential = new Credential();
            
            credential.UserId = user.Id;
            credential.CredentialTypeId = credentialType.Id;
            credential.Identifier = identifier;
            
            if (!string.IsNullOrEmpty(secret))
            {
                byte[] salt = Pbkdf2Hasher.GenerateRandomSalt();
                string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
                
                credential.Secret = hash;
                credential.Extra = Convert.ToBase64String(salt);
            }
        
            storage.Credentials.Add(credential);
            storage.SaveChanges();
            return new SignUpResult(user: user, success: true);
        }
        
        public void AddToRole(User user, string roleCode)
        {
            Role role = this.storage.Roles.FirstOrDefault(r => string.Equals(r.Code, roleCode, StringComparison.OrdinalIgnoreCase));
            
            if (role == null)
            return;
            
            AddToRole(user, role);
        }
        
        public void AddToRole(User user, Role role)
        {
            UserRole userRole = this.storage.UserRoles.Find(user.Id, role.Id);
            
            if (userRole != null)
            return;
            
            userRole = new UserRole();
            userRole.UserId = user.Id;
            userRole.RoleId = role.Id;
            this.storage.UserRoles.Add(userRole);
            this.storage.SaveChanges();
        }
        
        public void RemoveFromRole(User user, string roleCode)
        {
            Role role = this.storage.Roles.FirstOrDefault(r => string.Equals(r.Code, roleCode, StringComparison.OrdinalIgnoreCase));
            
            if (role == null)
            return;
            
            this.RemoveFromRole(user, role);
        }
        
        public void RemoveFromRole(User user, Role role)
        {
            UserRole userRole = this.storage.UserRoles.Find(user.Id, role.Id);
            
            if (userRole == null)
            return;
            
            this.storage.UserRoles.Remove(userRole);
            this.storage.SaveChanges();
        }
        
        public ChangeSecretResult ChangeSecret(string credentialTypeCode, string identifier, string secret)
        {
            CredentialType credentialType = this.storage.CredentialTypes.FirstOrDefault(ct => string.Equals(ct.Code, credentialTypeCode, StringComparison.OrdinalIgnoreCase));
            
            if (credentialType == null)
            return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialTypeNotFound);
            
            Credential credential = this.storage.Credentials.FirstOrDefault(c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
            
            if (credential == null)
            return new ChangeSecretResult(success: false, error: ChangeSecretResultError.CredentialNotFound);
            
            byte[] salt = Pbkdf2Hasher.GenerateRandomSalt();
            string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
            
            credential.Secret = hash;
            credential.Extra = Convert.ToBase64String(salt);
            this.storage.Credentials.Update(credential);
            this.storage.SaveChanges();
            return new ChangeSecretResult(success: true);
        }
        
        public ValidateResult Validate(string credentialTypeCode, string identifier)
        {
            return this.Validate(credentialTypeCode, identifier, null);
        }
        
        public ValidateResult Validate(string credentialTypeCode, string identifier, string secret)
        {
            CredentialType credentialType = this.storage.CredentialTypes.FirstOrDefault(ct => string.Equals(ct.Code, credentialTypeCode, StringComparison.OrdinalIgnoreCase));
            
            if (credentialType == null)
                return new ValidateResult(success: false, error: ValidateResultError.CredentialTypeNotFound);
            
            Credential credential = this.storage.Credentials.FirstOrDefault(c => c.CredentialTypeId == credentialType.Id && c.Identifier == identifier);
            
            if (credential == null)
                return new ValidateResult(success: false, error: ValidateResultError.CredentialNotFound);
            
            if (!string.IsNullOrEmpty(secret))
            {
                byte[] salt = Convert.FromBase64String(credential.Extra);
                string hash = Pbkdf2Hasher.ComputeHash(secret, salt);
                
                if (credential.Secret != hash)
                    return new ValidateResult(success: false, error: ValidateResultError.SecretNotValid);
            }
            
            return new ValidateResult(user: this.storage.Users.Find(credential.UserId), success: true);
        }
        
        public async Task SignIn(HttpContext httpContext, User user, bool isPersistent = false)
        {
            ClaimsIdentity identity = new ClaimsIdentity(this.GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
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
            return -1;
            
            Claim claim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            
            if (claim == null)
            return -1;
            
            int currentUserId;
            
            if (!int.TryParse(claim.Value, out currentUserId))
            return -1;
            
            return currentUserId;
        }
        
        public User GetCurrentUser(HttpContext httpContext)
        {
            int currentUserId = this.GetCurrentUserId(httpContext);
            
            if (currentUserId == -1)
            return null;
            
            return this.storage.Users.Find(currentUserId);
            }
            
            private IEnumerable<Claim> GetUserClaims(User user)
            {
            List<Claim> claims = new List<Claim>();
            
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }
        
        private IEnumerable<Claim> GetUserRoleClaims(User user)
        {
            List<Claim> claims = new List<Claim>();
            IEnumerable<int> roleIds = this.storage.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
            
            if (roleIds != null)
            {
                foreach (int roleId in roleIds)
                {
                    Role role = this.storage.Roles.Find(roleId);
                    
                    claims.Add(new Claim(ClaimTypes.Role, role.Code));
                    claims.AddRange(this.GetUserPermissionClaims(role));
                }
            }
            return claims;
        }
        
        private IEnumerable<Claim> GetUserPermissionClaims(Role role)
        {
            List<Claim> claims = new List<Claim>();
            IEnumerable<int> permissionIds = this.storage.RolePermissions.Where(rp => rp.RoleId == role.Id).Select(rp => rp.PermissionId).ToList();
            
            if (permissionIds != null)
            {
                foreach (int permissionId in permissionIds)
                {
                    Permission permission = this.storage.Permissions.Find(permissionId);
                    
                    claims.Add(new Claim("Permission", permission.Code));
                }
            }
        
            return claims;
        }
    }
}