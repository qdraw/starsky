using System;
using System.Threading.Tasks;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Models.Account;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskyAdminCli.Models;

namespace starskyAdminCli.Services;

public class ConsoleAdmin
{
	private readonly IConsole _console;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IUserManager _userManager;

	public ConsoleAdmin(IUserManager userManager, IConsole console,
		IHttpClientHelper httpClientHelper)
	{
		_userManager = userManager;
		_console = console;
		_httpClientHelper = httpClientHelper;
	}

	public async Task Tool(string userName, string password)
	{
		if ( string.IsNullOrEmpty(userName) )
		{
			_console.WriteLine("\nWhat is the username/email?\n ");
			var consoleUserName = _console.ReadLine();
			if ( string.IsNullOrEmpty(consoleUserName) )
			{
				_console.WriteLine("No input selected");
				return;
			}

			userName = consoleUserName;
		}

		if ( _userManager.Exist(userName) == null )
		{
			if ( string.IsNullOrEmpty(password) )
			{
				_console.WriteLine(
					"\nWe are going to create an account. \nWhat is the password?\n ");
				var consolePassword = _console.ReadLine();
				if ( string.IsNullOrEmpty(consolePassword) )
				{
					_console.WriteLine("No input selected");
					return;
				}

				password = consolePassword;
			}

			if ( !_userManager.PreflightValidate(userName, password, password) )
			{
				_console.WriteLine("username / password is not valid");
				return;
			}

			await _userManager.SignUpAsync(string.Empty, "email", userName, password);
			_console.WriteLine($"User {userName} is created");
			return;
		}

		_console.WriteLine(
			"\nDo you want to \n" +
			"2. Remove account \n" +
			"3. Toggle User Role \n" +
			"4. Setup Dropbox \n " +
			"\n(Enter only the number)");
		var option = _console.ReadLine();

		if ( !Enum.TryParse<ManageAdminOptions>(option,
			    out var selectedOption) )
		{
			_console.WriteLine("No input selected ends now");
			return;
		}

		switch ( selectedOption )
		{
			case ManageAdminOptions.RemoveAccount:
				await _userManager.RemoveUser("Email", userName);
				_console.WriteLine($"User {userName} is removed");
				return;
			case ManageAdminOptions.ToggleUserAdminRole:
				await ToggleUserAdminRole(userName);
				return;
			case ManageAdminOptions.SetupDropbox:
				await new DropboxSetup(_console, _httpClientHelper).Setup();
				return;
		}
	}

	private async Task ToggleUserAdminRole(string userName)
	{
		var user = _userManager.GetUser("Email", userName);
		var currentRole = _userManager.GetRole("Email", userName);

		if ( user == null || currentRole == null )
		{
			return;
		}

		_userManager.RemoveFromRole(user, currentRole);

		if ( currentRole.Code == AccountRoles.AppAccountRoles.User.ToString() )
		{
			_userManager.AddToRole(user, AccountRoles.AppAccountRoles.Administrator.ToString());
			_console.WriteLine($"User {userName} has now the role Administrator");
			await TogglePlatformAdmin(user, userName);
			return;
		}

		_userManager.AddToRole(user, AccountRoles.AppAccountRoles.User.ToString());
		_console.WriteLine($"User {userName} has now the role User");

		// a platform admin that is no longer an administrator would keep tenant management rights
		if ( user.IsPlatformAdmin &&
		     await _userManager.SetPlatformAdminAsync(user.Id, false) )
		{
			_console.WriteLine($"User {userName} is no longer platform admin");
		}
	}

	private async Task TogglePlatformAdmin(User user, string userName)
	{
		var makeAdmin = !user.IsPlatformAdmin;
		_console.WriteLine(makeAdmin
			? "\nMake this user also platform admin? (y/N)\n "
			: "\nThis user is platform admin. Remove platform admin? (y/N)\n ");

		var answer = _console.ReadLine();
		if ( !string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase) )
		{
			return;
		}

		if ( !await _userManager.SetPlatformAdminAsync(user.Id, makeAdmin) )
		{
			_console.WriteLine($"User {userName} is not found");
			return;
		}

		_console.WriteLine(makeAdmin
			? $"User {userName} is now platform admin"
			: $"User {userName} is no longer platform admin");
	}
}
