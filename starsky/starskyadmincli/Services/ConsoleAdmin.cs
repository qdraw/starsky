using System;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskyAdminCli.Models;

namespace starskyAdminCli.Services
{
	public class ConsoleAdmin
	{
		private readonly IUserManager _userManager;
		private readonly IConsole _console;

		public ConsoleAdmin(IUserManager userManager, IConsole console)
		{
			_userManager = userManager;
			_console = console;
		}
		public void Tool(string userName, string password)
		{
			if (string.IsNullOrEmpty(userName))
			{
				_console.WriteLine("\nWhat is the username/email?\n ");
				userName = _console.ReadLine();
				if (string.IsNullOrEmpty(userName))
				{
					_console.WriteLine("No input selected");
					return;
				}
			}

			if ( _userManager.Exist(userName) == null)
			{
				if ( string.IsNullOrEmpty(password) )
				{
					_console.WriteLine("\nWe are going to create an account.\n What is the password?\n ");
					password = _console.ReadLine();
					if (string.IsNullOrEmpty(password))
					{
						_console.WriteLine("No input selected");
						return;
					}
				}

				if ( !_userManager.PreflightValidate(userName, password,password) )
				{
					_console.WriteLine("username / password is not valid");
					return;
				}
				
				_userManager.SignUp(string.Empty, "email", userName, password);
				_console.WriteLine($"User {userName} is created");
				return;
			}
			
			_console.WriteLine("\nDo you want to \n2. remove account \n3. Toggle User Role \n \n(Enter only the number)");
			var option = _console.ReadLine();

			Enum.TryParse<ManageAdminOptions>(option, out var selectedOption);

			switch ( selectedOption )
			{
				case ManageAdminOptions.RemoveAccount :
					_userManager.RemoveUser("Email", userName);
					_console.WriteLine($"User {userName} is removed");
					return;
				case ManageAdminOptions.ToggleUserAdminRole:
					ToggleUserAdminRole(userName);
					return;
			}
		}

		private void ToggleUserAdminRole(string userName)
		{
			var user = _userManager.GetUser("Email", userName);
			var currentRole = _userManager.GetRole("Email", userName);

			_userManager.RemoveFromRole(user,currentRole);
			if ( currentRole.Code == AccountRoles.AppAccountRoles.User.ToString() )
			{
				_userManager.AddToRole(user,AccountRoles.AppAccountRoles.Administrator.ToString());
				_console.WriteLine($"User {userName} has now the role Administrator");
				return;
			}
			_userManager.AddToRole(user,AccountRoles.AppAccountRoles.User.ToString());
			_console.WriteLine($"User {userName} has now the role User");
		}
	}
}
