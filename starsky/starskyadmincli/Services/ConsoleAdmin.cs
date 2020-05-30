using System;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskyAdminCli.Models;
using starskycore.Interfaces;

namespace starskyAdminCli.Services
{
	public class ConsoleAdmin
	{
		private readonly AppSettings _appSettings;
		private readonly IUserManager _userManager;
		private readonly IConsole _console;

		public ConsoleAdmin(AppSettings appSettings, IUserManager userManager, IConsole console)
		{
			_appSettings = appSettings;
			_userManager = userManager;
			_console = console;
		}
		public void Tool()
		{
			if (_appSettings.Name == new AppSettings().Name)
			{
				_console.WriteLine("\nWhat is the username/email?\n ");
				var name = _console.ReadLine();
				_appSettings.Name = name;
				if (string.IsNullOrEmpty(name))
				{
					_console.WriteLine("No input selected");
					return;
				}
			}

			if ( _userManager.Exist(_appSettings.Name) == null)
			{
				_console.WriteLine($"User {_appSettings.Name} does not exist");
				return;
			}
			
			_console.WriteLine("\nDo you want to \n2. remove account \n3. Toggle User Role \n \n(Enter only the number)");
			var option = _console.ReadLine();

			Enum.TryParse<ManageAdminOptions>(option, out var selectedOption);

			switch ( selectedOption )
			{
				case ManageAdminOptions.RemoveAccount :
					_userManager.RemoveUser("Email", _appSettings.Name);
					_console.WriteLine($"User {_appSettings.Name} is removed");
					return;
				case ManageAdminOptions.ToggleUserAdminRole:
					ToggleUserAdminRole();
					return;
			}
		}

		private void ToggleUserAdminRole()
		{
			var user = _userManager.GetUser("Email", _appSettings.Name);
			var currentRole = _userManager.GetRole("Email", _appSettings.Name);

			_userManager.RemoveFromRole(user,currentRole);
			if ( currentRole.Code == AccountRoles.AppAccountRoles.User.ToString() )
			{
				_userManager.AddToRole(user,AccountRoles.AppAccountRoles.Administrator.ToString());
				_console.WriteLine($"User {_appSettings.Name} has now the role Administrator");
				return;
			}
			_userManager.AddToRole(user,AccountRoles.AppAccountRoles.User.ToString());
			_console.WriteLine($"User {_appSettings.Name} has now the role User");
		}
	}
}
