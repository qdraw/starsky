using System.ComponentModel.DataAnnotations;

namespace starsky.foundation.accountmanagement.Models.Account
{
	public sealed class LoginViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; } = true;
	}
}
