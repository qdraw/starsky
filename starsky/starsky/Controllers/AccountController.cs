using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Models.Account;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.Helpers;

namespace starsky.Controllers
{
	public sealed class AccountController : Controller
	{
		private readonly IUserManager _userManager;
		private readonly AppSettings _appSettings;
		private readonly IAntiforgery _antiForgery;
		private readonly IStorage _storageHostFullPathFilesystem;
		private readonly ApplicationDbContext _dbContext;
		private readonly ITenantSlugValidator _tenantSlugValidator;
		private readonly ITenantSessionStore _tenantSessionStore;

		private const string ModelError = "Model is not correct";

		public AccountController(IUserManager userManager, AppSettings appSettings,
			IAntiforgery antiForgery, ISelectorStorage selectorStorage,
			ApplicationDbContext dbContext, ITenantSlugValidator tenantSlugValidator,
			ITenantSessionStore tenantSessionStore)
		{
			_userManager = userManager;
			_appSettings = appSettings;
			_antiForgery = antiForgery;
			_dbContext = dbContext;
			_tenantSlugValidator = tenantSlugValidator;
			_tenantSessionStore = tenantSessionStore;
			_storageHostFullPathFilesystem =
				selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		}

		[HttpGet("/api/account/status")]
		[ProducesResponseType(typeof(UserIdentifierStatusModel), 200)]
		[ProducesResponseType(typeof(string), 401)]
		[ProducesResponseType(typeof(string), 406)]
		[ProducesResponseType(typeof(string), 503)]
		[Produces("application/json")]
		[AllowAnonymous]
		public async Task<IActionResult> Status()
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			var userOverview = await _userManager.AllUsersAsync();
			if ( !userOverview.IsSuccess )
			{
				Response.StatusCode = 503;
				return Json("Database error");
			}

			if ( userOverview.Users.Count == 0 && _appSettings.NoAccountLocalhost != true )
			{
				Response.StatusCode = 406;
				return Json("There are no accounts, you must create an account first");
			}

			if ( User.Identity?.IsAuthenticated != true )
			{
				return Unauthorized("false");
			}

			var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if ( !int.TryParse(idValue, out var userId) )
			{
				return Unauthorized("false");
			}

			var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if ( currentUser == null )
			{
				return Conflict("Current User does not exist in database");
			}

			var model = new UserIdentifierStatusModel
			{
				Name = currentUser.Name,
				Id = currentUser.Id,
				Created = currentUser.Created,
				RoleCode = User.FindFirstValue(TenantAuthenticationConstants.TenantRoleClaimType)
			};

			var credentials = _userManager.GetCredentialsByUserId(currentUser.Id);
			if ( credentials == null )
			{
				model.CredentialsIdentifiers = null;
				model.CredentialTypeIds = null;
				return Json(model);
			}

			model.CredentialsIdentifiers?.Add(credentials.Identifier!);
			model.CredentialTypeIds?.Add(credentials.CredentialTypeId);
			return Json(model);
		}

		[HttpGet("/account/login")]
		[HttpHead("/account/login")]
		[ProducesResponseType(200)]
		[Produces("text/html")]
		[SuppressMessage("ReSharper", "UnusedParameter.Global")]
		[SuppressMessage("Usage", "IDE0060:Remove unused parameter")]
		[AllowAnonymous]
		public IActionResult LoginGet(string? returnUrl = null, bool? fromLogout = null)
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			if ( !ModelState.IsValid )
			{
				return BadRequest(ModelError);
			}

			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			var clientApp = Path.Combine(_appSettings.BaseDirectoryProject,
				"clientapp", "build", "index.html");

			if ( !_storageHostFullPathFilesystem.ExistFile(clientApp) )
			{
				return Content("Please check if the client code exist");
			}

			return PhysicalFile(clientApp, "text/html");
		}

		[HttpPost("/api/account/login")]
		[ProducesResponseType(typeof(string), 200)]
		[ProducesResponseType(typeof(string), 401)]
		[ProducesResponseType(typeof(string), 405)]
#if !DEBUG
        [ValidateAntiForgeryToken]
#endif
		[Produces("application/json")]
		[AllowAnonymous]
		public async Task<IActionResult> LoginPost(LoginViewModel model)
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			if ( !ModelState.IsValid )
			{
				return BadRequest(ModelError);
			}

			var validateResult =
				await _userManager.ValidateAsync("Email", model.Email, model.Password);

			if ( !validateResult.Success )
			{
				Response.StatusCode = 401;
				if ( validateResult.Error == ValidateResultError.Lockout )
				{
					Response.StatusCode = 423;
				}

				return Json("Login failed");
			}

			var user = validateResult.User;
			if ( user == null )
			{
				Response.StatusCode = 401;
				return Json("Login failed");
			}

			var tenantEntity = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == tenant);
			if ( tenantEntity == null )
			{
				var anyTenantExists = await _dbContext.Tenants.AnyAsync();
				if ( anyTenantExists )
				{
					return NotFound("Tenant not found");
				}

				tenantEntity = new Tenant
				{
					Slug = tenant,
					Name = tenant,
					IsEnabled = true,
					Created = System.DateTime.UtcNow
				};
				await _dbContext.Tenants.AddAsync(tenantEntity);
				await _dbContext.SaveChangesAsync();
			}

			if ( !tenantEntity.IsEnabled )
			{
				return Forbid();
			}

			var membership = await _dbContext.TenantUsers
				.FirstOrDefaultAsync(m => m.TenantId == tenantEntity.Id && m.UserId == user.Id);

			if ( membership == null )
			{
				var hasMembers = await _dbContext.TenantUsers.AnyAsync(m => m.TenantId == tenantEntity.Id);
				if ( hasMembers )
				{
					return StatusCode(StatusCodes.Status403Forbidden,
						"User is not a member of this tenant");
				}

				membership = new TenantUser
				{
					TenantId = tenantEntity.Id,
					UserId = user.Id,
					Role = TenantRole.Admin,
					Created = System.DateTime.UtcNow
				};

				await _dbContext.TenantUsers.AddAsync(membership);
				await _dbContext.SaveChangesAsync();
			}

			Request.Cookies.TryGetValue(TenantAuthenticationConstants.SessionCookieName,
				out var existingSessionId);

			var webSession = await _tenantSessionStore.CreateOrRefreshSessionAsync(user.Id,
				existingSessionId);
			await _tenantSessionStore.ActivateTenantAsync(webSession.Id, tenantEntity.Id);

			Response.Cookies.Append(TenantAuthenticationConstants.SessionCookieName,
				webSession.SessionId,
				new CookieOptions
				{
					HttpOnly = true,
					Path = "/",
					SameSite = SameSiteMode.Lax,
					Secure = _appSettings.HttpsOn == true,
					Expires = webSession.ExpiresAt,
					IsEssential = true
				});

			return Json("Login Success");
		}

		[HttpPost("/api/account/logout")]
		[ProducesResponseType(200)]
		[AllowAnonymous]
		public async Task<IActionResult> LogoutJson()
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			if ( Request.Cookies.TryGetValue(TenantAuthenticationConstants.SessionCookieName,
					 out var sessionId) && !string.IsNullOrWhiteSpace(sessionId) )
			{
				var tenantEntity = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == tenant);
				var webSession = await _tenantSessionStore.GetValidSessionAsync(sessionId);
				if ( tenantEntity != null && webSession != null )
				{
					await _tenantSessionStore.DeactivateTenantAsync(webSession.Id, tenantEntity.Id);
				}
			}

			return Json("your logged out");
		}

		[HttpGet("/account/logout")]
		[ProducesResponseType(200)]
		[AllowAnonymous]
		public IActionResult Logout(string? returnUrl = null)
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			if ( !ModelState.IsValid )
			{
				return BadRequest(ModelError);
			}

			return RedirectToAction(nameof(LoginGet),
				new { ReturnUrl = returnUrl, fromLogout = true });
		}

		[HttpPost("/api/account/change-secret")]
		[ProducesResponseType(typeof(string), 200)]
		[ProducesResponseType(typeof(string), 400)]
		[ProducesResponseType(typeof(string), 401)]
		[Produces("application/json")]
		[Authorize]
		public async Task<IActionResult> ChangeSecret(ChangePasswordViewModel model)
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			if ( User.Identity?.IsAuthenticated != true )
			{
				return Unauthorized("please login first");
			}

			if ( !ModelState.IsValid ||
				 model.ChangedPassword != model.ChangedConfirmPassword )
			{
				return BadRequest(ModelError);
			}

			var currentUserId = _userManager.GetCurrentUser(HttpContext)?.Id;
			currentUserId ??= -1;
			var credential = _userManager.GetCredentialsByUserId(( int ) currentUserId);

			var validateResult = await _userManager.ValidateAsync(
				"Email",
				credential?.Identifier,
				model.Password);

			if ( !validateResult.Success )
			{
				return Unauthorized("Password is not correct");
			}

			var changeSecretResult =
				_userManager.ChangeSecret("Email", credential?.Identifier,
					model.ChangedPassword);

			return Json(changeSecretResult);
		}

		[HttpPost("/api/account/register")]
		[ProducesResponseType(typeof(string), 200)]
		[ProducesResponseType(typeof(string), 400)]
		[ProducesResponseType(typeof(string), 403)]
		[ProducesResponseType(typeof(string), 405)]
		[Produces("application/json")]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			if ( await IsAccountRegisterClosed(User.Identity?.IsAuthenticated == true) )
			{
				Response.StatusCode = 403;
				return Json("Account Register page is closed");
			}

			if ( ModelState.IsValid && model.ConfirmPassword == model.Password )
			{
				await _userManager.SignUpAsync(model.Name, "email", model.Email, model.Password);
				return Json("Account Created");
			}

			Response.StatusCode = 400;
			return Json(ModelError);
		}

		private async Task<bool> IsAccountRegisterClosed(bool userIdentityIsAuthenticated)
		{
			if ( userIdentityIsAuthenticated )
			{
				return false;
			}

			return _appSettings.IsAccountRegisterOpen != true &&
				   ( await _userManager.AllUsersAsync() ).Users.Count != 0;
		}

		[HttpGet("/api/account/register/status")]
		[ProducesResponseType(typeof(string), 200)]
		[ProducesResponseType(typeof(string), 403)]
		[Produces("application/json")]
		[AllowAnonymous]
		public async Task<IActionResult> RegisterStatus()
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			if ( ( await _userManager.AllUsersAsync() ).Users.Count == 0 )
			{
				Response.StatusCode = 202;
			}

			if ( !await IsAccountRegisterClosed(
					User.Identity?.IsAuthenticated == true) )
			{
				return Json("RegisterStatus open");
			}

			Response.StatusCode = 403;
			return Json("Account Register page is closed");
		}

		[HttpGet("/api/account/permissions")]
		[Authorize]
		[ProducesResponseType(typeof(List<string>), 200)]
		[ProducesResponseType(401)]
		public IActionResult Permissions()
		{
			var tenant = GetTenantSlug();
			if ( !_tenantSlugValidator.IsValid(tenant) )
			{
				return NotFound("Tenant not found");
			}

			var claims = User.Claims.Where(p => p.Type == "Permission").Select(p => p.Value);
			return Json(claims);
		}

		[HttpPost("/api/account/logout-all")]
		[AllowAnonymous]
		public async Task<IActionResult> LogoutAll()
		{
			if ( Request.Cookies.TryGetValue(TenantAuthenticationConstants.SessionCookieName,
					 out var sessionId) && !string.IsNullOrWhiteSpace(sessionId) )
			{
				var webSession = await _tenantSessionStore.GetValidSessionAsync(sessionId);
				if ( webSession != null )
				{
					await _tenantSessionStore.RevokeSessionAsync(webSession.Id);
				}
			}

			Response.Cookies.Delete(TenantAuthenticationConstants.SessionCookieName,
				new CookieOptions { Path = "/" });
			return Json("Logged out from all tenants");
		}

		private string? GetTenantSlug()
		{
			var tenant = HttpContext.Items[TenantAuthenticationConstants.TenantSlugItemKey] as string;
			if ( !string.IsNullOrWhiteSpace(tenant) )
			{
				return tenant;
			}

			return "main";
		}
	}
}
